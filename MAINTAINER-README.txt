================================================================================
MAINTAINER-README: CodeBrix.Platform.Unicode
Notes for people and agents MAINTAINING this repository — not for package
consumers
================================================================================

Consumers of either NuGet package should read AGENT-README.txt instead; it is
the consumer contract and it covers both packages. This file covers building,
testing, packaging, provenance and the repository's own conventions.


PURPOSE AND SCOPE
=================

This repository redistributes the International Components for Unicode (ICU)
version 77 native runtime to .NET 10 applications, as TWO NuGet packages built
from one repository:

  CodeBrix.Platform.Unicode.ApacheLicenseForever
      src/CodeBrix.Platform.Unicode/
      Windows: icuuc77.dll + icudt77.dll for win-x64 and win-arm64.

  CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever
      src/CodeBrix.Platform.UnicodeMacOs/
      macOS: universal (x86_64 + arm64) libicuuc.dylib + libicudata.dylib under
      the single osx runtime identifier.

Both packages additionally carry a copy of the ICU data archive icudt.dat
(byte-identical between the two - verified by checksum) and a buildTransitive
.targets pair that delivers it to the consuming build.

Documentation map: ONE root AGENT-README.txt covers BOTH packages, and both
csprojs pack that same root file into their .nupkg. There is no per-project
AGENT-README. README-INDEX.txt maps every README file in the repository.

Kept in one repository on purpose: the two packages bundle binaries from the
same ICU release under the same licenses, so a shared repository keeps
provenance, licensing and version stamping in one place.


REPOSITORY LAYOUT
=================

    CodeBrix.Platform.Unicode.slnx        solution (both src projects, a Tests
                                          folder with both test projects, and a
                                          Solution Items folder)
    icon-codebrix-128.png                 NuGet icon for both packages
    LICENSE                               Apache-2.0 (packaging, .targets,
                                          carrier assemblies)
    UNICODE-LICENSE.txt                   Unicode License Version 3 (bundled ICU
                                          binaries and icudt.dat)
    THIRD-PARTY-NOTICES.txt               file-by-file third-party provenance
    README.md                             human-facing overview (GitHub +
                                          nuget.org)
    AGENT-README.txt                      consumer documentation, both packages
    MAINTAINER-README.txt                 this file
    EXTRAS-README.txt                     non-package content
    README-INDEX.txt                      map of the README files

    src/CodeBrix.Platform.Unicode/
        CodeBrix.Platform.Unicode.csproj
        InternalsVisibleTo.cs
        buildTransitive/
            CodeBrix.Platform.Unicode.ApacheLicenseForever.targets
            CodeBrix.Platform.Unicode.Common.targets
            icudt.dat
        runtimes/win-x64/native/{icuuc77.dll, icudt77.dll}
        runtimes/win-arm64/native/{icuuc77.dll, icudt77.dll}

    src/CodeBrix.Platform.UnicodeMacOs/
        CodeBrix.Platform.UnicodeMacOs.csproj
        InternalsVisibleTo.cs
        buildTransitive/
            CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever.targets
            CodeBrix.Platform.UnicodeMacOs.Common.targets
            icudt.dat
        runtimes/osx/native/{libicuuc.dylib, libicudata.dylib}

    tests/CodeBrix.Platform.Unicode.Tests/
    tests/CodeBrix.Platform.UnicodeMacOs.Tests/
        Each: TestAssetPaths.cs, NativeBinaryPresenceTests.cs,
        DataArchivePresenceTests.cs, TargetsFileTests.cs,
        AssemblyMetadataTests.cs and the .csproj.

Neither src project has any C# source beyond InternalsVisibleTo.cs: the
produced assemblies are metadata-only carriers, and a test asserts that they
export zero public types. Do not add public types without updating that test
and AGENT-README.txt.

The on-disk filename of each auto-imported .targets file MUST equal the
PackageId - that name-match is what makes NuGet auto-import it, and it is also
what avoids NU5129. Renaming a package therefore means renaming its .targets
file and the Import inside it, and the corresponding TestAssetPaths constant
and TargetsFileTests assertion.


BUILDING
========

    dotnet restore CodeBrix.Platform.Unicode.slnx
    dotnet build   CodeBrix.Platform.Unicode.slnx

Both src projects set GeneratePackageOnBuild=true, so a plain build also
produces a .nupkg per project under src/<Project>/bin/<Config>/. There is no
build script and no code generation; the "build" is essentially packaging plus
the two empty carrier assemblies.

The repository builds on any OS - nothing in the build executes the bundled
native binaries.


TESTING
=======

    dotnet test CodeBrix.Platform.Unicode.slnx

Two suites, one per package, xUnit v3 with SilverAssertions. There are no
opt-in environment variables and no special preparation.

The suites do NOT load ICU or call into the native binaries. They are asset and
contract tests: each test csproj copies its package's payload into a
TestAssets/ mirror in the test output (via None items with Link metadata and
CopyToOutputDirectory=PreserveNewest, mirroring the in-package folder shape),
and TestAssetPaths.cs resolves paths under AppContext.BaseDirectory/TestAssets.

What is asserted, per suite:

  * Native binary presence and file-format sanity - "MZ" for the Windows PE
    images, the Mach-O universal FAT magic 0xCA 0xFE 0xBA 0xBE for the dylibs -
    plus a >1 MB floor on the common library and a <100 KB ceiling on the data
    shim.
  * icudt.dat presence, a 4 MB..20 MB size window (deliberately loose, so ICU
    patch releases do not break the suite), and the ICU DataHeader magic bytes
    0xDA 0x27 at offsets 2-3.
  * .targets content: that the auto-imported file imports Common.targets, that
    the embed target carries the expected Name, BeforeTargets
    "BeforeBuild;BeforeCompile", the '$(IsCodeBrixHead)' == 'true' gate, the
    '$(CodeBrixIcuDataIncluded)' != 'true' guard, the
    <CodeBrixIcuDataIncluded>true</CodeBrixIcuDataIncluded> write and the
    <EmbeddedResource ...icudt.dat> item; that the upstream target name is not
    declared; and (macOS suite only) that the Windows package's target name is
    not declared either, which is the guarantee that referencing both packages
    produces no target redefinition.
  * Assembly metadata: loadable by simple name, TargetFrameworkAttribute
    starting with ".NETCoreApp,Version=v10.", zero exported public types.

Known coverage gap: TargetsFileTests covers only the EMBED target of each
Common.targets. The second target - AddCodeBrixPlatformUnicodeDataFile /
AddCodeBrixPlatformUnicodeMacOsDataFile, BeforeTargets="AssignTargetPaths",
which copies icudt.dat beside a non-head Exe/WinExe consumer - has no
assertions. Adding them (name, BeforeTargets, the OutputType clause, the
Link/CopyToOutputDirectory metadata) is the obvious next test increment, and it
would have caught the documentation drift that this file records under NOTES.


PACKAGING AND PUBLISHING
========================

Both csprojs are self-contained package definitions; there is no Directory.
Build.props and no shared pack driver.

Common metadata in both:

    PackageLicenseExpression   Apache-2.0 AND Unicode-3.0
    PackageRequireLicenseAcceptance  true
    PackageIcon                icon-codebrix-128.png
    PackageReadmeFile          README.md
    GeneratePackageOnBuild     true
    IncludeContentInPack       true
    GenerateDocumentationFile  true
    Copyright                  the standard CodeBrix line plus the ICU 77
                               Unicode, Inc. attribution
    Authors                    Jeremy Ellis

Root files packed by BOTH csprojs (Pack="true" PackagePath=""), from ..\..\:

    icon-codebrix-128.png
    README.md
    AGENT-README.txt          <- the single root file, packed into both nupkgs
    THIRD-PARTY-NOTICES.txt
    UNICODE-LICENSE.txt

LICENSE itself is not packed - the license is expressed through
PackageLicenseExpression, and UNICODE-LICENSE.txt travels for the Unicode-3.0
half.

Payload packed per project:

    buildTransitive\**\*  ->  buildTransitive/     (both .targets + icudt.dat)
    runtimes\win-x64\native\{icuuc77,icudt77}.dll
    runtimes\win-arm64\native\{icuuc77,icudt77}.dll
                          ->  runtimes/<rid>/native/<file>     (Windows)
    runtimes\osx\native\{libicuuc,libicudata}.dylib
                          ->  runtimes/osx/native/<file>       (macOS)

Versioning scheme (identical in both csprojs, documented at length in the
csproj comment): 1.<x>.<y>.<z> computed from System.DateTime.UtcNow at build
time - x = whole years since _VersionBaseYear (2026), y = day of year (1-based,
UTC), z = minute of day (0..1439, floored). Version, AssemblyVersion and
FileVersion all take that value. Consequences:

  * Every build produces a new version and, with GeneratePackageOnBuild, a new
    .nupkg. Two builds within the same UTC minute produce the SAME version - do
    not publish two packages from within one minute.
  * This is date-stamp versioning, not SemVer: major is pinned to 1 and minor
    encodes the year, so neither signals API compatibility.
  * Re-baseline by changing _VersionBaseYear.

Publishing: the two packages are part of the CodeBrix.Platform family and are
shipped together with the rest of that family as one version and one event.
Bumping the ICU payload means republishing both packages, since they share the
icudt.dat archive.


PROVENANCE AND VENDORED SOURCES
===============================

Upstream: the packaging shape and the .targets logic derive from the upstream
platform's two ICU redistribution NuGet packages (version 77.2.1, one for
Windows and one for macOS); the native binaries and icudt.dat are the
unmodified ICU 77 builds those packages carried, sourced from the ICU project
(https://github.com/unicode-org/icu, branch maint/maint-77).

Text files derived from upstream carry an inline provenance header comment in
the form

    <!-- was previously: <upstream-path> -->

Binary files (.dll, .dylib, .dat) cannot carry inline provenance, so their
file-by-file provenance - path, description, size, and the checksum findings -
lives in THIRD-PARTY-NOTICES.txt, together with the full list of modifications
made during the port and the license-compatibility statement.

Refresh history recorded in THIRD-PARTY-NOTICES.txt: the payload was refreshed
from upstream 77.1.2 to 77.2.1. All four Windows DLLs changed and were
replaced; both macOS dylibs were byte-identical across the two upstream
versions and were left in place; icudt.dat is byte-identical across both
upstream versions and across the two packages.

Regenerating the payload (what a refresh means in practice):

  1. Obtain the new upstream ICU binaries for Windows (icuuc<NN>.dll +
     icudt<NN>.dll, x64 and arm64) and macOS (universal libicuuc.dylib +
     libicudata.dylib), and the matching icudt.dat.
  2. Replace the files in place under src/<Project>/runtimes/... and
     src/<Project>/buildTransitive/icudt.dat. Keep the two icudt.dat copies
     byte-identical and record the checksum comparison.
  3. If the ICU major version changes, the Windows file NAMES change
     (icuuc77.dll -> icuuc<NN>.dll). Those names appear in: both csproj
     packaging item groups, both test csproj TestAssets item groups,
     NativeBinaryPresenceTests.cs, README.md and AGENT-README.txt. Update all
     of them together. The macOS dylib names and icudt.dat are version-neutral.
  4. Update THIRD-PARTY-NOTICES.txt (sizes, checksum findings, refresh note).
  5. Re-run the test suites - the size floors/ceilings and the ICU DataHeader
     magic check are the guard rails.
  6. Do not modify the binaries. They are redistributed bit-for-bit; any
     modification would change the licensing and trademark position described
     in THIRD-PARTY-NOTICES.txt.

Editing the .targets files: the two Common.targets are the consumer contract.
Their target names must stay distinct between the two packages, and their
sentinel property must stay the SAME in both. Any change to a property name, a
target name, a BeforeTargets value or a condition is a breaking change for
consumers and must be mirrored in TargetsFileTests.cs and in AGENT-README.txt
in the same commit.


CODING CONVENTIONS
==================

Standard CodeBrix family conventions apply. Points that matter here:

  * net10.0 only. No multi-targeting, ever.
  * Nullable reference types OFF - no <Nullable>enable</Nullable>, no `?` on
    reference types, no `!` null-forgiveness. Value-type nullables are fine.
  * No global usings.
  * GenerateDocumentationFile is on in both src projects; every public member of
    a public type needs an XML doc comment and CS1591 is fixed at source, never
    suppressed. With zero public types today this is trivially clean - it stops
    being trivial the moment someone adds a public type.
  * No project-level warning suppression (<NoWarn>, <TreatWarningsAsErrors>
    false, WarningLevel 0 and friends are all forbidden).
  * Each packaging project ships an InternalsVisibleTo.cs granting its own
    .Tests assembly access.
  * Tests: xUnit v3 plus SilverAssertions; test class file names are
    <Class>Tests.cs; test method names are snake_case; bodies carry
    //Arrange //Act //Assert comments. Any cancellable call inside a test
    threads TestContext.Current.CancellationToken (xUnit1051).
  * The word for the upstream project is "upstream" - the upstream platform's
    product name is not used in CodeBrix documentation or type names.


NOTES
=====

Documentation drift to be aware of (these files are outside the scope of the
current documentation pass and were left untouched):

  * README.md still describes the MSBuild contract with the PRE-RENAME upstream
    property names (an upstream head flag and an upstream ICU sentinel) and
    still frames both packages as drop-in replacements for the upstream ICU
    packages. The shipped .targets use '$(IsCodeBrixHead)' and
    $(CodeBrixIcuDataIncluded), and README.md also predates the second
    (copy-beside) target. It needs the same correction that AGENT-README.txt
    has now received.

  * THIRD-PARTY-NOTICES.txt, under "Modifications made during port", states
    that the upstream MSBuild PROPERTY names were preserved verbatim so that a
    build seeing both an upstream ICU package and a CodeBrix one would embed
    icudt.dat once. That is no longer true: the properties were renamed to the
    CodeBrix names, and the embed-once guarantee now spans only the two
    CodeBrix packages in this repository. The rest of that file (binary
    provenance, sizes, licensing) is accurate.

  * The comment block at the top of each Common.targets is the authoritative
    description of the two delivery routes, the class-library rule and the
    U_MISSING_RESOURCE_ERROR symptom. Treat it as the source of truth and keep
    AGENT-README.txt in step with it.

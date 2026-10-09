================================================================================
AGENT-README: CodeBrix.Platform.Unicode
A Guide for AI Coding Agents — CONSUMING the CodeBrix.Platform.Unicode and
CodeBrix.Platform.UnicodeMacOs NuGet packages
================================================================================


OVERVIEW
========

These two packages deliver the International Components for Unicode (ICU)
version 77 native runtime to a .NET 10 (or later) application on Windows and on
macOS. Neither package has a managed API: each is a metadata-only assembly that
acts as a carrier for

  * the ICU "common" native library and its data-shim companion, laid out under
    NuGet's runtimes/<rid>/native/ convention, and
  * icudt.dat, the ~5.5 MB ICU data archive (Unicode character properties,
    normalization, BiDi, case mapping, break iteration, CLDR locale data, time
    zones, transliteration), plus a buildTransitive .targets pair that delivers
    that archive into the consuming build automatically.

Windows and macOS have no system ICU. Without one of the two delivery routes
described below, the first call into anything that needs a Unicode property
table - line breaking, for example - fails with U_MISSING_RESOURCE_ERROR.
Linux ships its own ICU, so neither package is used there (see WHAT THESE
PACKAGES DO NOT DO).

Target framework: .NET 10 or later. There is no multi-targeting and there are
no managed NuGet dependencies.

Provenance: the packaging shape and the .targets logic derive from the upstream
platform's ICU redistribution packages; the native binaries and the data archive
are the unmodified ICU 77 builds those upstream packages carried.
The CodeBrix packages are not framed as substitutes for anything upstream -
they are the CodeBrix family's ICU carriers, with CodeBrix-named MSBuild
properties and targets, and are what a CodeBrix.Platform application
references. File-by-file provenance is recorded in THIRD-PARTY-NOTICES.txt,
which ships inside both packages.


INSTALLATION
============

    dotnet add package CodeBrix.Platform.Unicode.ApacheLicenseForever
    dotnet add package CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever

PackageIds:

  CodeBrix.Platform.Unicode.ApacheLicenseForever
      Windows. Ships win-x64 and win-arm64 native binaries.
      Assembly / product name: CodeBrix.Platform.Unicode

  CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever
      macOS. Ships universal (x86_64 + arm64) dylibs under the single osx RID.
      Assembly / product name: CodeBrix.Platform.UnicodeMacOs

The .ApacheLicenseForever suffix exists only on the PackageId, as a
license-disambiguation marker used across the CodeBrix family; the assemblies
themselves are named CodeBrix.Platform.Unicode and
CodeBrix.Platform.UnicodeMacOs.

NuGet dependencies: none. Neither package pulls in any other NuGet package.

License: Apache-2.0 AND Unicode-3.0. The packaging, the .targets files and the
carrier assemblies are Apache-2.0; the bundled ICU 77 native binaries and
icudt.dat are Unicode-3.0. Both packages set PackageRequireLicenseAcceptance,
and both ship UNICODE-LICENSE.txt (the full Unicode License Version 3) and
THIRD-PARTY-NOTICES.txt inside the .nupkg, which is what satisfies the
Unicode-3.0 redistribution conditions - do not strip those files from a
redistribution of your own application.

Requirements and OS limits:

  * .NET 10 or later.
  * Windows package: a build that resolves a win-x64 or win-arm64 runtime
    identifier if you need the native DLLs in your output (see RUNTIME
    IDENTIFIERS below). The icudt.dat delivery does NOT need a RID.
  * macOS package: the osx RID, which is the RID-graph parent of both osx-x64
    and osx-arm64; the shipped dylibs are Mach-O universal binaries carrying
    both slices, so one folder serves Intel and Apple-silicon Macs.
  * No Linux, Android, iOS, or other RID is shipped by either package.


WHICH PACKAGE DO I REFERENCE
============================

By target OS - the two packages are per-OS, not alternatives:

    Consumer                                     Reference
    -----------------------------------------    ----------------------------
    Windows head / Windows console tool /        CodeBrix.Platform.Unicode
      Windows test project                         .ApacheLicenseForever
    macOS head / macOS console tool /            CodeBrix.Platform.UnicodeMacOs
      macOS test project                           .ApacheLicenseForever
    Linux head (X11, Wayland, Frame Buffer)      neither - Linux has a system
                                                   ICU (see below)
    Android / iOS / browser-wasm                 neither - not shipped

A CodeBrix.Platform head needs no reference of its own: the Windows head
packages (CodeBrix.Platform.Runtime.Skia.Win32.ApacheLicenseForever and
CodeBrix.Platform.Runtime.Skia.Wpf.ApacheLicenseForever) depend on
CodeBrix.Platform.Unicode.ApacheLicenseForever, and the macOS head package
(CodeBrix.Platform.Runtime.Skia.MacOS.ApacheLicenseForever) depends on
CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever. The head receives the
package transitively and, because the .targets sit in buildTransitive/, route 1
still fires in the head. A direct reference in a head is harmless (the shared
sentinel delivers icudt.dat once); it is only needed to pin a different package
version. The table above matters most for console tools and test projects,
which reference no head package.

A multi-head solution (a Windows head and a macOS head in the same solution):
each head already receives its OS's package through its head package. A
project that references the packages directly may reference BOTH; that is also
safe and is explicitly supported: the two packages share one
$(CodeBrixIcuDataIncluded) sentinel so icudt.dat is delivered exactly once, and
their MSBuild target names are deliberately distinct
(AddCodeBrixPlatformUnicode* vs AddCodeBrixPlatformUnicodeMacOs*) so importing
both produces no target-redefinition error. Which package's copy of icudt.dat
wins is not defined and does not matter: the two archives are byte-identical.

Class libraries: a class library may reference the package - that is how a
host-free CodeBrix.Platform add-in makes ICU available to whatever consumes it,
because the .targets sit in buildTransitive/ and therefore flow on to the
downstream project - but the library's OWN build gets neither delivery route.
Only an application-shaped project at the root of the build receives icudt.dat.
See "CLASS LIBRARIES GET NEITHER ROUTE" below.


KEY NAMESPACES / USINGS
=======================

None. Neither package exposes a public managed type - the carrier assemblies
export zero public types by design, which their test suites assert. You write
no `using` directive for these packages and call no API on them. Everything
these packages do happens at build time (MSBuild) and at native-load time
(NuGet's runtimes/ mechanism).


CORE API REFERENCE (THE MSBUILD CONTRACT)
=========================================

The "API" of these packages is the MSBuild contract in their buildTransitive/
folder. Each package ships an auto-imported .targets file named exactly after
its PackageId - NuGet auto-imports it on that name-match - which does nothing
but import the package's Common.targets:

    buildTransitive/CodeBrix.Platform.Unicode.ApacheLicenseForever.targets
        -> imports CodeBrix.Platform.Unicode.Common.targets

    buildTransitive/CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever.targets
        -> imports CodeBrix.Platform.UnicodeMacOs.Common.targets

Each Common.targets defines TWO targets: one per delivery route. Both routes
are gated on the same two properties.

PROPERTIES YOU CAN SEE AND SET
------------------------------

    $(IsCodeBrixHead)
        'true' when the consuming project is a CodeBrix.Platform application
        head. Set by the head's own build; these packages only read it. Route
        selector: 'true' picks the embed route, anything else picks the
        copy-beside route.

    $(CodeBrixIcuDataIncluded)
        The embed-once sentinel. Both routes require it to be != 'true' before
        they fire, and both set it to 'true' after firing. Shared verbatim
        between the Windows and macOS packages so that a build seeing both
        delivers icudt.dat exactly once.

    $(OutputType)
        Read by the copy-beside route only, which requires 'Exe' or 'WinExe'.

ROUTE 1 - EMBED INTO A HEAD
---------------------------

    Target name:   AddCodeBrixPlatformUnicodeEmbeddedResource        (Windows)
                   AddCodeBrixPlatformUnicodeMacOsEmbeddedResource   (macOS)
    BeforeTargets: BeforeBuild;BeforeCompile
    Condition:     '$(IsCodeBrixHead)' == 'true'
                   and '$(CodeBrixIcuDataIncluded)' != 'true'
    Effect:        <EmbeddedResource Include="...buildTransitive/icudt.dat" />
                   then sets <CodeBrixIcuDataIncluded>true</...>

    The archive becomes part of the head assembly. The head's own build also
    generates a module initializer that names that assembly to the platform's
    ICU engine, so nothing in your code has to locate or register the archive.
    These packages contribute only the EmbeddedResource item.

ROUTE 2 - COPY BESIDE A NON-HEAD EXECUTABLE
-------------------------------------------

    Target name:   AddCodeBrixPlatformUnicodeDataFile               (Windows)
                   AddCodeBrixPlatformUnicodeMacOsDataFile          (macOS)
    BeforeTargets: AssignTargetPaths
    Condition:     '$(IsCodeBrixHead)' != 'true'
                   and '$(CodeBrixIcuDataIncluded)' != 'true'
                   and ('$(OutputType)' == 'Exe'
                        or '$(OutputType)' == 'WinExe')
    Effect:        <None Include="...buildTransitive/icudt.dat"
                         Link="icudt.dat"
                         CopyToOutputDirectory="PreserveNewest"
                         Visible="false" />
                   then sets <CodeBrixIcuDataIncluded>true</...>

    icudt.dat lands beside the built application (bin/<config>/net10.0/
    icudt.dat), where the ICU engine looks for it when no assembly carries an
    embedded copy. This is the route that makes host-free consumers work: a
    test project, a console tool, an image or text pipeline - anything that is
    an executable but is not a CodeBrix.Platform head. It is what lets the
    host-free CodeBrix.Platform text add-ins named in the shipped targets
    comments (TextLayout, AdvancedTextEdit) be exercised outside an
    application.

    The BeforeTargets choice is load-bearing and is documented in the shipped
    .targets: a None item must carry a target path before
    GetCopyToOutputDirectoryItems reads it, so the target hooks
    AssignTargetPaths rather than BeforeBuild. Hooking it later would silently
    skip the copy.

CLASS LIBRARIES GET NEITHER ROUTE
---------------------------------

A project with OutputType Library matches neither condition: it is not a head,
and it is not Exe/WinExe. That is deliberate - whatever roots the build
supplies the archive once, instead of every library in the dependency graph
dropping its own 5.5 MB copy into the output folder. If a library needs ICU at
test time, its TEST project (an executable) picks up route 2.

ROUTE SELECTION AT A GLANCE
---------------------------

    IsCodeBrixHead   OutputType       Result
    --------------   --------------   ---------------------------------------
    true             (any)            icudt.dat embedded in the head assembly
    not true         Exe / WinExe     icudt.dat copied beside the executable
    not true         Library          nothing delivered (by design)

    In every row the sentinel must be != 'true' when the target runs. If
    $(CodeBrixIcuDataIncluded) is already 'true' - because the sibling package
    got there first, or because the project set it - nothing is delivered.


HOW THE RUNTIME FINDS icudt.dat
===============================

The ICU engine looks for the data archive in this order:

  1. As a manifest resource of the assembly that the head's generated module
     initializer named to it (route 1).
  2. As a file named icudt.dat sitting beside the running application
     (route 2).

If neither is present, ICU has no data tables at all and the first operation
that needs one fails. The symptom documented in the shipped .targets is a
failure with U_MISSING_RESOURCE_ERROR on the first line-breaking call. That
error is the canonical signal that icudt.dat was not delivered - i.e. that the
project is a Library, or that $(CodeBrixIcuDataIncluded) was already 'true'
when the targets ran, or that the package reference is missing from the
project that roots the build.

Do not rename, relocate, or compress icudt.dat in your output folder: route 2
places it beside the application under exactly that name, and that is where the
engine looks.


RUNTIME IDENTIFIERS AND THE NATIVE BINARIES
===========================================

icudt.dat delivery (both routes) is RID-independent - it happens on any build.
The NATIVE binaries are different: they sit under runtimes/<rid>/native/, and
NuGet copies those to your output only when the build resolves a matching
runtime identifier. There is no AnyCPU or RID-less fallback copy in either
package.

  Windows: two RIDs, win-x64 and win-arm64, with separate binaries (the
  ARM64 icuuc77.dll is a distinct PE32+ ARM64 image, not a copy of the x64
  one). A build that targets both must produce RID-specific outputs, e.g.

      <RuntimeIdentifiers>win-x64;win-arm64</RuntimeIdentifiers>

  and publish per RID; a single RID-less build cannot carry both.

  macOS: one RID, osx. Because osx is the RID-graph parent of osx-x64 and
  osx-arm64, a build for either of those resolves the osx folder, and the
  universal dylibs contain both architecture slices. You never need an
  osx-x64/osx-arm64 variant of this package.


OPTING OUT AND OVERRIDING
=========================

There is no dedicated opt-out property; the sentinel is the lever. Set

    <PropertyGroup>
      <CodeBrixIcuDataIncluded>true</CodeBrixIcuDataIncluded>
    </PropertyGroup>

at project scope (a plain PropertyGroup in the csproj, so it is evaluated
before targets run) and BOTH targets are skipped for that project. Do this only
when you are delivering icudt.dat yourself - for example when a custom publish
step places the archive beside the application, or when a wrapper package
already embedded it. Setting it without providing the archive some other way
produces the U_MISSING_RESOURCE_ERROR symptom above.

To force the embed route on a project that is not a CodeBrix.Platform head, set
<IsCodeBrixHead>true</IsCodeBrixHead> at project scope; be aware that route 1
only adds the EmbeddedResource - the module initializer that names the
carrying assembly to the engine comes from the head build, so a hand-rolled
"head" must arrange that itself. Prefer route 2 for non-head executables.

There is no property that turns the native runtimes/ assets off; use standard
NuGet ExcludeAssets on the PackageReference if you need that.


WHAT EACH PACKAGE SHIPS
=======================

CodeBrix.Platform.Unicode.ApacheLicenseForever (Windows), in-package paths:

    lib/net10.0/CodeBrix.Platform.Unicode.dll        carrier assembly, no
                                                     public types
    runtimes/win-x64/native/icuuc77.dll              ICU 77 "common" library,
                                                     PE32+ x86-64, ~3.0 MB
    runtimes/win-x64/native/icudt77.dll              data-shim DLL, ~15 KB
    runtimes/win-arm64/native/icuuc77.dll            ICU 77 "common" library,
                                                     PE32+ ARM64, ~2.9 MB
    runtimes/win-arm64/native/icudt77.dll            data-shim DLL, ~15 KB
    buildTransitive/icudt.dat                        ICU data archive, ~5.5 MB
    buildTransitive/CodeBrix.Platform.Unicode
        .ApacheLicenseForever.targets                auto-imported by NuGet
    buildTransitive/CodeBrix.Platform.Unicode
        .Common.targets                              the two delivery targets
    README.md, AGENT-README.txt, UNICODE-LICENSE.txt,
    THIRD-PARTY-NOTICES.txt, icon-codebrix-128.png

CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever (macOS), in-package paths:

    lib/net10.0/CodeBrix.Platform.UnicodeMacOs.dll   carrier assembly, no
                                                     public types
    runtimes/osx/native/libicuuc.dylib               ICU 77 "common" library,
                                                     Mach-O universal
                                                     (x86_64 + arm64), ~4 MB
    runtimes/osx/native/libicudata.dylib             data-shim library,
                                                     universal, ~33 KB
    buildTransitive/icudt.dat                        ICU data archive, ~5.5 MB,
                                                     byte-identical to the
                                                     Windows package's copy
    buildTransitive/CodeBrix.Platform.UnicodeMacOs
        .ApacheLicenseForever.targets                auto-imported by NuGet
    buildTransitive/CodeBrix.Platform.UnicodeMacOs
        .Common.targets                              the two delivery targets
    README.md, AGENT-README.txt, UNICODE-LICENSE.txt,
    THIRD-PARTY-NOTICES.txt, icon-codebrix-128.png

Note the split of responsibilities: icudt77.dll / libicudata.dylib are tiny
SHIMS, not the data. All ICU data lives in icudt.dat. Copying only the native
binaries and omitting icudt.dat leaves ICU with no tables.


COMPLETE EXAMPLES
=================

1. Windows application head
---------------------------

    <PackageReference
        Include="CodeBrix.Platform.Unicode.ApacheLicenseForever" />

    A head that references a Windows head package already receives this
    reference transitively (see WHICH PACKAGE DO I REFERENCE); writing it
    directly is optional. That is the whole integration. $(IsCodeBrixHead) is already 'true' in a
    CodeBrix.Platform head, so AddCodeBrixPlatformUnicodeEmbeddedResource
    fires during BeforeBuild/BeforeCompile and icudt.dat is embedded in the
    head assembly. No code changes, no runtime configuration, no using
    directive.

2. macOS application head
-------------------------

    <PackageReference
        Include="CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever" />

    Identical mechanism via AddCodeBrixPlatformUnicodeMacOsEmbeddedResource.
    As on Windows, the macOS head package already carries this reference
    transitively.

3. One project that must build for both desktop OSes
----------------------------------------------------

    <ItemGroup Condition="'$(OS)' == 'Windows_NT'">
      <PackageReference
          Include="CodeBrix.Platform.Unicode.ApacheLicenseForever" />
    </ItemGroup>
    <ItemGroup Condition="'$(OS)' != 'Windows_NT'">
      <PackageReference
          Include="CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever" />
    </ItemGroup>

    Conditioning is an optimisation (it keeps the unused OS's natives out of
    the graph), not a requirement - referencing both unconditionally is safe
    because of the shared sentinel and the distinct target names.

4. A test project that exercises text layout on Windows
-------------------------------------------------------

    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
      </PropertyGroup>
      <ItemGroup>
        <PackageReference Include="xunit.v3" Version="..." />
        <PackageReference
            Include="CodeBrix.Platform.Unicode.ApacheLicenseForever" />
      </ItemGroup>
    </Project>

    A xunit.v3 test project is an executable, so it is not a head and it is
    OutputType Exe: AddCodeBrixPlatformUnicodeDataFile fires and icudt.dat is
    copied next to the test binary. Without the package reference the first
    line-breaking call inside a test fails with U_MISSING_RESOURCE_ERROR.

5. Suppressing delivery in a project that ships its own archive
---------------------------------------------------------------

    <PropertyGroup>
      <CodeBrixIcuDataIncluded>true</CodeBrixIcuDataIncluded>
    </PropertyGroup>


MINIMUM VIABLE PROJECT
======================

Route 1 - head app (nothing but the reference; a head that references a
Windows head package already has it transitively):

    <!-- MyApp.Win32Skia.csproj (a CodeBrix.Platform head) -->
    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <OutputType>WinExe</OutputType>
        <RuntimeIdentifier>win-x64</RuntimeIdentifier>
      </PropertyGroup>
      <ItemGroup>
        <PackageReference
            Include="CodeBrix.Platform.Unicode.ApacheLicenseForever" />
      </ItemGroup>
    </Project>

    Result: icudt.dat embedded in MyApp.Win32Skia.dll; icuuc77.dll and
    icudt77.dll copied to the output from runtimes/win-x64/native/ because the
    RID resolves.

Route 2 - console tool:

    <!-- TextTool.csproj -->
    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <OutputType>Exe</OutputType>
        <RuntimeIdentifier>win-x64</RuntimeIdentifier>
      </PropertyGroup>
      <ItemGroup>
        <PackageReference
            Include="CodeBrix.Platform.Unicode.ApacheLicenseForever" />
      </ItemGroup>
    </Project>

    // Program.cs - no ICU code of your own is needed; the tool simply calls
    // whatever CodeBrix.Platform text API it uses. The package's only job is
    // that the archive is on disk before that call happens.

    Result in bin/Debug/net10.0/win-x64/:

        TextTool.exe
        TextTool.dll
        icudt.dat            <- route 2 (PreserveNewest)
        icuuc77.dll          <- runtimes/win-x64/native
        icudt77.dll          <- runtimes/win-x64/native

    Swap the PackageId for CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever
    and the RID for osx-arm64 (or osx-x64) to get the macOS equivalent, with
    libicuuc.dylib / libicudata.dylib in place of the DLLs.


PERFORMANCE TIPS
================

There are no performance-sensitive code paths here - these are static asset
carriers with no managed API. The only cost-relevant facts:

  * icudt.dat is ~5.5 MB. Route 1 adds that to the head assembly; route 2 adds
    it to the output folder. It is delivered ONCE per build root, which is
    exactly why class libraries are excluded and why the sentinel is shared
    between the two packages - do not defeat either mechanism by copying the
    archive yourself in addition.
  * The native payload is another ~3 MB per Windows RID and ~4 MB for the
    macOS universal dylib. If your publish targets one architecture, publish
    with that single RID rather than shipping both Windows RID folders.
  * Nothing is loaded or parsed at build time; both targets only add an item.


COMMON PITFALLS TO AVOID
========================

  * Assuming a class library gets the archive. It does not, by design. If your
    library's consumers hit U_MISSING_RESOURCE_ERROR, the fix is a package
    reference in the application or test project that roots the build, not a
    workaround in the library.

  * Expecting the native DLLs without a runtime identifier. runtimes/<rid>/
    native/ assets are RID-resolved. A RID-less build still gets icudt.dat
    (that route is RID-independent) but no icuuc77.dll - a combination that
    looks like "the package did nothing".

  * Setting $(CodeBrixIcuDataIncluded) to 'true' as a "cleanup" of a duplicate
    reference. It suppresses delivery entirely for that project. Duplicate
    references are already safe; the sentinel exists to make them safe.

  * Reaching for the old property names. The gate is '$(IsCodeBrixHead)' and
    the sentinel is $(CodeBrixIcuDataIncluded). Any documentation or csproj
    still referring to pre-rename property names is stale and will not fire
    these targets.

  * Copying icudt77.dll (or libicudata.dylib) and calling it the data. It is a
    ~15 KB shim; icudt.dat is the data.

  * Renaming or relocating icudt.dat in the output folder, or setting
    CopyToOutputDirectory on your own duplicate item. Route 2 places it beside
    the application under that exact name.

  * Adding an osx-x64 / osx-arm64 specific package. There is only an osx RID
    folder, and the dylibs are universal binaries; a more specific RID resolves
    to it through the RID graph.

  * Expecting ICU formatting/collation entry points. The shipped binaries are
    the ICU "common" library only - verified exports include ubrk_*, unorm2_*,
    ucnv_*, uloc_* and u_*, but NOT ucol_open, udat_open or unum_open, which
    live in ICU's i18n library and are not shipped.

  * Expecting .NET's own globalization to change. Referencing these packages
    does not switch the .NET runtime to app-local ICU: nothing in either
    package sets the app-local ICU switch or InvariantGlobalization, and the
    binaries are not placed where the .NET host looks for an app-local ICU.
    CultureInfo, string comparison and DateTime formatting behave exactly as
    they did before. These packages serve the CodeBrix.Platform ICU engine,
    not System.Globalization.

  * Stripping UNICODE-LICENSE.txt or THIRD-PARTY-NOTICES.txt from a
    redistribution. The Unicode-3.0 half of the license expression requires
    the notice to travel with the binaries.


WHAT THESE PACKAGES DO NOT DO
=============================

  * No Linux, Android, iOS, or browser-wasm binaries. Linux distributions ship
    their own ICU and a CodeBrix.Platform Linux head uses that system ICU; this
    repository does not name or bundle a Linux ICU package, so if a Linux
    target reports missing ICU, install the distribution's own ICU runtime
    package rather than referencing anything from here.

  * No managed API of any kind: no P/Invoke wrappers, no typed accessors over
    the data archive, no ICU facade. The carrier assemblies export zero public
    types.

  * No ICU "i18n" library (icuin / libicui18n): number, date, message and
    collation formatting entry points are absent. Only the "common" library
    and the data shim ship.

  * No switch of .NET globalization to app-local ICU, and no runtimeconfig
    change of any kind.

  * No delivery to class libraries, and no second delivery once the sentinel is
    set.

  * No Windows AnyCPU/RID-less native copy, and no per-architecture macOS
    packages.

  * No ICU trademark grant. The Unicode Consortium's "ICU" mark is not reused
    for these packages, and a modified redistribution must not imply
    endorsement by the Consortium.


WORKING EXAMPLES ON GITHUB
==========================

The test suites are the executable specification of everything above - each
one is itself a route-2 consumer of its package.

  https://github.com/ellisnet/CodeBrix.Platform.Unicode/tree/main/tests/CodeBrix.Platform.Unicode.Tests
      TargetsFileTests.cs        - asserts the auto-imported .targets imports
                                   Common.targets, that the embed target is
                                   named
                                   AddCodeBrixPlatformUnicodeEmbeddedResource,
                                   that it runs BeforeTargets
                                   "BeforeBuild;BeforeCompile", that it gates on
                                   '$(IsCodeBrixHead)' == 'true', and that both
                                   the '$(CodeBrixIcuDataIncluded)' != 'true'
                                   guard and the
                                   <CodeBrixIcuDataIncluded>true</...> write are
                                   present.
      NativeBinaryPresenceTests.cs - asserts icuuc77.dll + icudt77.dll exist for
                                   win-x64 and win-arm64, that icuuc77.dll
                                   starts with the PE "MZ" magic and is over
                                   1 MB, and that icudt77.dll is under 100 KB
                                   (it is a shim).
      DataArchivePresenceTests.cs - asserts icudt.dat exists, is between 4 MB
                                   and 20 MB, and starts with the ICU
                                   DataHeader magic bytes 0xDA 0x27 at offsets
                                   2-3.
      AssemblyMetadataTests.cs   - asserts the carrier assembly loads by the
                                   name CodeBrix.Platform.Unicode, targets
                                   .NETCoreApp v10, and exports no public types.

  https://github.com/ellisnet/CodeBrix.Platform.Unicode/tree/main/tests/CodeBrix.Platform.UnicodeMacOs.Tests
      The same four files for the macOS package: the Mach-O universal FAT magic
      (0xCA 0xFE 0xBA 0xBE) in place of "MZ", libicuuc.dylib /
      libicudata.dylib in place of the DLLs, the target name
      AddCodeBrixPlatformUnicodeMacOsEmbeddedResource, plus an extra assertion
      that the macOS Common.targets does NOT declare the Windows package's
      target name - the guarantee that referencing both packages is safe.

  The shipped .targets themselves, with the comment block that documents both
  routes, the class-library rule and the U_MISSING_RESOURCE_ERROR symptom:
  https://github.com/ellisnet/CodeBrix.Platform.Unicode/tree/main/src/CodeBrix.Platform.Unicode/buildTransitive
  https://github.com/ellisnet/CodeBrix.Platform.Unicode/tree/main/src/CodeBrix.Platform.UnicodeMacOs/buildTransitive


QUICK REFERENCE CARD
====================

    Windows PackageId    CodeBrix.Platform.Unicode.ApacheLicenseForever
    macOS PackageId      CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever
    License              Apache-2.0 AND Unicode-3.0
    TFM                  net10.0 (or later)
    NuGet dependencies   none
    Public managed API   none (zero exported types)

    RIDs                 win-x64, win-arm64  |  osx (universal x86_64+arm64)
    Native files         icuuc77.dll + icudt77.dll  |  libicuuc.dylib +
                         libicudata.dylib
    Data archive         buildTransitive/icudt.dat (~5.5 MB, identical in both
                         packages)

    Gate property        $(IsCodeBrixHead)
    Sentinel property    $(CodeBrixIcuDataIncluded)

    Head target          AddCodeBrixPlatformUnicodeEmbeddedResource
                         AddCodeBrixPlatformUnicodeMacOsEmbeddedResource
                         BeforeTargets="BeforeBuild;BeforeCompile"
                         -> <EmbeddedResource> icudt.dat

    Exe target           AddCodeBrixPlatformUnicodeDataFile
                         AddCodeBrixPlatformUnicodeMacOsDataFile
                         BeforeTargets="AssignTargetPaths"
                         -> <None> icudt.dat, CopyToOutputDirectory
                            PreserveNewest, beside the app

    Library projects     no delivery (by design)
    Opt out              <CodeBrixIcuDataIncluded>true</CodeBrixIcuDataIncluded>
    Missing-archive      U_MISSING_RESOURCE_ERROR on the first line-breaking
      symptom            call
    Not shipped          Linux/Android/iOS binaries, ICU i18n library,
                         app-local .NET globalization, any managed API

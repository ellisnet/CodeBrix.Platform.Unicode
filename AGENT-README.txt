========================================================================
AGENT-README: CodeBrix.Platform.Unicode
A Comprehensive Guide for AI Coding Agents
========================================================================


OVERVIEW
========================================================================

CodeBrix.Platform.Unicode is a .NET 10 redistribution of the
International Components for Unicode (ICU) version 77 native binaries
for Windows, packaged for the CodeBrix family. It is a namespace-renamed
port of `Uno.icu-win` 77.1.2, intended as a drop-in replacement for that
package in CodeBrix.Platform-forked Uno applications.

The library has effectively no managed code: the assembly is a metadata-
only .NET 10 DLL whose sole purpose is to host the bundled native ICU
binaries and the embedded-resource MSBuild logic. The interesting
payload lives in:

  - 4 native DLLs under `runtimes/win-{x64|arm64}/native/`:
      * `icuuc77.dll` — ICU 77 "common" library
      * `icudt77.dll` — ICU 77 data-shim DLL
  - 1 ICU data archive (`icudt.dat`) under `buildTransitive/`,
    containing the full Unicode/CLDR/timezone data tables.
  - 2 MSBuild `.targets` files under `buildTransitive/` that automatically
    embed `icudt.dat` as an `<EmbeddedResource>` at consumer-build time
    when `$(IsUnoHead) == 'True'` and `$(UnoIcuDataIncluded) != 'true'`.


INSTALLATION
========================================================================

NuGet package: CodeBrix.Platform.Unicode.ApacheLicenseForever

  dotnet add package CodeBrix.Platform.Unicode.ApacheLicenseForever

The library namespace inside the assembly is `CodeBrix.Platform.Unicode`
(without the `.ApacheLicenseForever` suffix; that suffix exists only on
the NuGet PackageId for license-disambiguation across the CodeBrix family).

Target framework: .NET 10.0 or higher.


KEY NAMESPACE
========================================================================

The library exposes no public managed types in its first iteration —
the assembly is metadata-only, matching the shape of upstream
Uno.icu-win. Consumers don't write any C# code against this library
directly; they reference it as a NuGet package and the ICU native DLLs
+ data archive flow into the consumer's runtime output via standard
NuGet `runtimes/` and `buildTransitive/` mechanisms.


BUNDLED ASSET INVENTORY
========================================================================

Native binaries (per Windows architecture, identical bytes vs upstream
Uno.icu-win 77.1.2):

  Windows x64 (runtimes/win-x64/native/):
    - icuuc77.dll  (ICU 77 common library)
    - icudt77.dll  (data-shim DLL)

  Windows ARM64 (runtimes/win-arm64/native/):
    - icuuc77.dll  (ICU 77 common library, ARM64)
    - icudt77.dll  (data-shim DLL)

ICU data archive:

  buildTransitive/icudt.dat  (ICU 77 data tables: Unicode character
    properties, collation, normalization, BiDi, case mapping, locale
    data (CLDR), time zones, transliteration, break iteration, etc.)

MSBuild logic:

  buildTransitive/CodeBrix.Platform.Unicode.ApacheLicenseForever.targets
    Auto-imported by NuGet because its filename matches the package's
    PackageId. Imports the Common.targets file below.

  buildTransitive/CodeBrix.Platform.Unicode.Common.targets
    Defines the target `AddCodeBrixPlatformUnicodeEmbeddedResource` that
    runs `BeforeTargets="BeforeBuild;BeforeCompile"` and embeds
    `icudt.dat` as an `<EmbeddedResource>` when the consumer project is
    an Uno head (`$(IsUnoHead) == 'True'`) and no other package has
    already embedded the data (`$(UnoIcuDataIncluded) != 'true'`).
    Sets `$(UnoIcuDataIncluded) = 'true'` after embedding so that any
    sibling Uno.icu-* package skips its own embedded-resource step.


CORE API REFERENCE
========================================================================

This library has no public managed API. Consumers interact with it
exclusively through:

  1. NuGet's `runtimes/<rid>/native/` mechanism — the ICU native DLLs
     (`icuuc77.dll` and `icudt77.dll`) are automatically copied into
     consumer output for `win-x64` and `win-arm64` runtime IDs.

  2. The MSBuild `.targets` files under `buildTransitive/` — when the
     consumer is an Uno head, these auto-import and embed `icudt.dat`
     as an `<EmbeddedResource>` in the consumer's output assembly.

If a future iteration of this library exposes a managed Unicode/ICU
facade (e.g. typed accessors over the embedded data, or P/Invoke
wrappers around `icuuc77.dll`), it will live under the
`CodeBrix.Platform.Unicode` root namespace and be documented here.


ARCHITECTURE
========================================================================

Repository layout:

  CodeBrix.Platform.Unicode/
    src/CodeBrix.Platform.Unicode/
      CodeBrix.Platform.Unicode.csproj
      InternalsVisibleTo.cs
      buildTransitive/
        CodeBrix.Platform.Unicode.ApacheLicenseForever.targets   (auto-imported)
        CodeBrix.Platform.Unicode.Common.targets                  (imported by the above)
        icudt.dat                                                 (~5.5 MB)
      runtimes/
        win-x64/native/
          icuuc77.dll
          icudt77.dll
        win-arm64/native/
          icuuc77.dll
          icudt77.dll
    tests/CodeBrix.Platform.Unicode.Tests/
      CodeBrix.Platform.Unicode.Tests.csproj
      TestAssetPaths.cs
      NativeBinaryPresenceTests.cs
      DataArchivePresenceTests.cs
      TargetsFileTests.cs
      AssemblyMetadataTests.cs
    AGENT-README.txt
    LICENSE                  (Apache-2.0; library packaging + .targets)
    UNICODE-LICENSE.txt      (Unicode-3.0; bundled ICU 77 binaries)
    README.md
    THIRD-PARTY-NOTICES.txt

Inside the produced NuGet (.nupkg), the file layout is:
  buildTransitive/CodeBrix.Platform.Unicode.ApacheLicenseForever.targets
  buildTransitive/CodeBrix.Platform.Unicode.Common.targets
  buildTransitive/icudt.dat
  lib/net10.0/CodeBrix.Platform.Unicode.dll      (metadata-only)
  runtimes/win-x64/native/icuuc77.dll
  runtimes/win-x64/native/icudt77.dll
  runtimes/win-arm64/native/icuuc77.dll
  runtimes/win-arm64/native/icudt77.dll
  AGENT-README.txt
  README.md
  UNICODE-LICENSE.txt
  THIRD-PARTY-NOTICES.txt
  icon-codebrix-128.png

The `runtimes/<rid>/native/` content layout is dictated by NuGet's
runtime-asset mechanism — that's how native DLLs flow into consumer
output automatically for the matching RuntimeIdentifier.


CODING CONVENTIONS (CodeBrix family)
========================================================================

This repository follows every CodeBrix family convention. Key points:

  * Target framework: net10.0 only. No multi-targeting.
  * Nullable reference types (NRT): OFF (do not set <Nullable>enable</Nullable>).
    No `?` annotations on reference types; no `!` null-forgiveness operator.
    Value-type nullables (`int?`, `DateOnly?`, etc.) are fine.
  * No global usings.
  * `<GenerateDocumentationFile>true</GenerateDocumentationFile>` is on.
    Every public/protected member of a public type needs an XML doc
    comment. CS1591 is fixed at source, never suppressed. (In this
    library's first iteration there are no public types, so CS1591
    is trivially clean.)
  * Tests use xUnit v3 + SilverAssertions; coverlet.collector
    for coverage; `TestContext.Current.CancellationToken` is threaded
    through any cancellable call inside a test.
  * No project-level warning suppression (`<NoWarn>`, `<WarningLevel>0</>`,
    `<TreatWarningsAsErrors>false</>`, etc. are all forbidden).
  * Copyright string in the csproj prepends the upstream attributions
    (Uno Platform Inc. for the packaging shape, Unicode Inc. for the
    bundled ICU binaries) to the standard CodeBrix copyright line, per
    the family's porting-guidance rule.


TESTING
========================================================================

Tests live under tests/CodeBrix.Platform.Unicode.Tests/. Run with:

  dotnet test CodeBrix.Platform.Unicode.slnx

The test suite covers:

  * Native binary presence: that the 4 ICU 77 native DLLs
    (`icuuc77.dll` + `icudt77.dll` for both win-x64 and win-arm64) are
    present at their NuGet `runtimes/<rid>/native/` paths in the
    test-output `TestAssets/` mirror.
  * Native binary file-format sanity: that each .dll begins with the
    PE32+ "MZ" magic bytes.
  * Data archive presence: that `icudt.dat` is present and is a
    plausibly-sized ICU data archive (sanity floor only — exact size
    will drift across ICU patch releases).
  * Targets file content: that the auto-imported .targets file imports
    the Common.targets file by relative path; that the Common.targets
    file declares the `AddCodeBrixPlatformUnicodeEmbeddedResource` target
    with the right `BeforeTargets`, `Condition`, and `EmbeddedResource`
    item; that both `$(IsUnoHead)` and `$(UnoIcuDataIncluded)` property
    references are preserved verbatim from upstream so cross-package
    sharing of the embed-once sentinel still works.
  * Assembly metadata: that the produced library assembly is named
    `CodeBrix.Platform.Unicode`, targets net10, and exports zero
    public types (matching the metadata-only design).


PROVENANCE
========================================================================

Files derived from upstream Uno.icu-win 77.1.2 carry a
`<!-- was previously: <upstream-path> -->` provenance comment in
their file header (for the .targets files). Binary files (.dll, .dat)
cannot carry inline provenance, so the file-by-file provenance for
those is recorded in THIRD-PARTY-NOTICES.txt instead.


KNOWN GOTCHAS
========================================================================

  * The .targets file's `$(IsUnoHead)` and `$(UnoIcuDataIncluded)`
    property references are preserved verbatim from upstream. If the
    CodeBrix-fork of Uno eventually renames those internal MSBuild
    properties, this .targets file must be updated in lockstep —
    otherwise the embed-once mechanism will silently stop firing.

  * The library only ships Windows native binaries (win-x64 + win-arm64).
    Linux, macOS, and Android are NOT covered by this library —
    consumers on those platforms should use the OS's native ICU
    (Linux/Android) or Apple's libicucore (macOS) instead. If a future
    iteration extends this library to bundle ICU 77 binaries for those
    platforms, the corresponding `runtimes/linux-*/native/` and
    `runtimes/osx-*/native/` folders would be added — but doing so
    typically requires a CodeBrix.Platform.Unicode.Linux,
    CodeBrix.Platform.Unicode.macOS, etc. companion package to keep
    each native-binary-bundle separately versioned.

  * The Reserved Trademark "ICU" must not be reused for any modified
    redistribution that would suggest endorsement by the Unicode
    Consortium. The library and its NuGet package are NOT trademarked
    under "ICU" — the package name is `CodeBrix.Platform.Unicode`.

# CodeBrix.Platform.Unicode

A redistribution of the International Components for Unicode (ICU) version 77 native binaries, packaged as CodeBrix-family NuGet libraries for .NET 10 applications.
This repository produces **two** NuGet packages — one per operating system:

| Package | OS |
|---|---|
| `CodeBrix.Platform.Unicode.ApacheLicenseForever` | Windows (x64 + ARM64) |
| `CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever` | macOS (universal: x86_64 + arm64) |

Either is usable in any .NET 10 project that needs the ICU 77 native runtime on the respective OS — Windows and macOS have no system ICU, so without one of these packages the first call that needs a Unicode property table fails.
The libraries have no managed dependencies other than .NET.

CodeBrix.Platform.Unicode supports applications and assemblies that target Microsoft .NET version 10.0 and later.
Microsoft .NET version 10.0 is a Long-Term Supported (LTS) version of .NET, and was released on Nov 11, 2025; and will be actively supported by Microsoft until Nov 14, 2028.
Please update your C#/.NET code and projects to the latest LTS version of Microsoft .NET.

## Installation

Add the package that matches the operating system you are targeting; the ICU native libraries are then made available to the runtime through the standard NuGet `runtimes/<rid>/native/` mechanism.

```
# Windows apps:
dotnet add package CodeBrix.Platform.Unicode.ApacheLicenseForever

# macOS apps:
dotnet add package CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever
```

**Which one do I reference?** One per target OS: reference the Windows package from a project that runs on Windows, the macOS package from a project that runs on macOS, and both from a solution that ships both. Referencing both in the same build is safe — the two packages share a sentinel property, so the ICU data archive is still delivered exactly once.

Note that the NuGet package IDs and the assembly names are different — there is no package named plain `CodeBrix.Platform.Unicode`:

* NuGet package ID: `CodeBrix.Platform.Unicode.ApacheLicenseForever` - assembly `CodeBrix.Platform.Unicode`
* NuGet package ID: `CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever` - assembly `CodeBrix.Platform.UnicodeMacOs`

These are runtime-binaries packages: neither exposes a managed API, so there is no namespace to `using` and nothing to call. Everything a consumer touches is a native library or an MSBuild property.

## CodeBrix.Platform.Unicode (Windows) supports:

* The ICU 77 "common" native runtime DLL (`icuuc77.dll`) for Windows on x64 and ARM64.
* The ICU 77 data shim DLL (`icudt77.dll`) for both Windows architectures.
* The full ICU 77 data archive (`icudt.dat`) — covering Unicode collation, normalization, BiDi, character properties, locale (CLDR) data, time zones, transliteration, and the rest of ICU's runtime data.
* A `buildTransitive` MSBuild `.targets` pair that automatically embeds `icudt.dat` as an `<EmbeddedResource>` at consumer-build time, when the consumer project is a CodeBrix.Platform head (`$(IsCodeBrixHead) == 'true'`) and the data has not yet been embedded by another package in the same build (`$(CodeBrixIcuDataIncluded) != 'true'`). A second target in the same `.targets` copies `icudt.dat` beside the output of non-head `Exe`/`WinExe` consumers (test projects, console tools); class libraries get neither route.

## CodeBrix.Platform.UnicodeMacOs supports:

* The ICU 77 "common" native runtime library (`libicuuc.dylib`) for macOS as a universal binary (x86_64 + arm64, one file for Intel and Apple-silicon Macs).
* The ICU 77 data shim library (`libicudata.dylib`), also universal.
* The same full ICU 77 data archive (`icudt.dat`) — byte-identical to the one in the Windows package.
* The same auto-embedding `buildTransitive` MSBuild `.targets` mechanism, using the same `$(IsCodeBrixHead)` / `$(CodeBrixIcuDataIncluded)` properties — so a build that sees both packages still embeds `icudt.dat` exactly once.

## Sample Code

### Suppress the automatic data delivery

Set the shared sentinel in your project when something else in the build already supplies `icudt.dat`; both packages honour it, and neither target then fires.

```xml
<PropertyGroup>
  <CodeBrixIcuDataIncluded>true</CodeBrixIcuDataIncluded>
</PropertyGroup>
```

### Use the ICU data from a console tool or a test project

No project change is needed. Any consumer whose `OutputType` is `Exe` or `WinExe` and that is not a CodeBrix.Platform head gets `icudt.dat` copied beside its output automatically; a CodeBrix.Platform head (`$(IsCodeBrixHead) == 'true'`) gets it embedded as a resource instead. Class libraries get neither route by design, so a library never drops its own copy of the archive into the output folder — whatever roots the build supplies it once.

## Documentation

Both NuGet packages include the same `AGENT-README.txt`, a complete usage guide written for AI coding agents - point your agent at that file when it is wiring either package into a project. It covers the per-RID native payloads, the MSBuild contract and the delivery routes.

Additional usage examples are available in the two test projects, which exercise both delivery routes:
https://github.com/ellisnet/CodeBrix.Platform.Unicode/tree/main/tests/CodeBrix.Platform.Unicode.Tests
https://github.com/ellisnet/CodeBrix.Platform.Unicode/tree/main/tests/CodeBrix.Platform.UnicodeMacOs.Tests

## License

CodeBrix.Platform.Unicode and CodeBrix.Platform.UnicodeMacOs are licensed under the Apache License 2.0 - see the
[LICENSE](https://github.com/ellisnet/CodeBrix.Platform.Unicode/blob/main/LICENSE) file. That covers the library
packaging, the `.targets` files and the wrapper assemblies.

The ICU 77 native binaries (`icuuc77.dll`, `icudt77.dll`, `libicuuc.dylib`, `libicudata.dylib`, `icudt.dat`)
themselves are licensed under the Unicode License, Version 3 — see the bundled `UNICODE-LICENSE.txt` file. Both
NuGet packages are published under the SPDX expression `Apache-2.0 AND Unicode-3.0`.

For licensing and provenance information about the open source code included in
these packages, see [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.Unicode/blob/main/THIRD-PARTY-NOTICES.txt).

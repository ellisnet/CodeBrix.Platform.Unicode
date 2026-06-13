# CodeBrix.Platform.Unicode

A redistribution of the International Components for Unicode (ICU) version 77 native binaries, packaged as CodeBrix-family NuGet libraries for .NET 10 applications.
This repository produces **two** NuGet packages — one per operating system:

| Package | OS | Replaces (upstream) |
|---|---|---|
| `CodeBrix.Platform.Unicode.ApacheLicenseForever` | Windows (x64 + ARM64) | `Uno.icu-win` 77.1.2 |
| `CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever` | macOS (universal: x86_64 + arm64) | `Uno.icu-macos` 77.1.2 |

Both are namespace-renamed, .NET 10 ports of their upstream Uno package — intended as drop-in replacements in CodeBrix.Platform-forked Uno applications, and usable in any .NET 10 project that needs the ICU 77 native runtime on the respective OS.
The libraries have no managed dependencies other than .NET.

CodeBrix.Platform.Unicode supports applications and assemblies that target Microsoft .NET version 10.0 and later.
Microsoft .NET version 10.0 is a Long-Term Supported (LTS) version of .NET, and was released on Nov 11, 2025; and will be actively supported by Microsoft until Nov 14, 2028.
Please update your C#/.NET code and projects to the latest LTS version of Microsoft .NET.

## CodeBrix.Platform.Unicode (Windows) supports:

* The ICU 77 "common" native runtime DLL (`icuuc77.dll`) for Windows on x64 and ARM64 — same `.dll` that upstream `Uno.icu-win` 77.1.2 ships, byte-for-byte.
* The ICU 77 data shim DLL (`icudt77.dll`) for both Windows architectures.
* The full ICU 77 data archive (`icudt.dat`, ~5.5 MB) — covering Unicode collation, normalization, BiDi, character properties, locale (CLDR) data, time zones, transliteration, and the rest of ICU's runtime data.
* A `buildTransitive` MSBuild `.targets` pair that automatically embeds `icudt.dat` as an `<EmbeddedResource>` at consumer-build time, when the consumer project is an Uno head (`$(IsUnoHead) == 'True'`) and the data has not yet been embedded by another package in the same build (`$(UnoIcuDataIncluded) != 'true'`).

## CodeBrix.Platform.UnicodeMacOs supports:

* The ICU 77 "common" native runtime library (`libicuuc.dylib`) for macOS as a universal binary (x86_64 + arm64, one file for Intel and Apple-silicon Macs) — same `.dylib` that upstream `Uno.icu-macos` 77.1.2 ships, byte-for-byte.
* The ICU 77 data shim library (`libicudata.dylib`), also universal.
* The same full ICU 77 data archive (`icudt.dat`) — byte-identical to the one in the Windows package.
* The same auto-embedding `buildTransitive` MSBuild `.targets` mechanism, using the same `$(IsUnoHead)` / `$(UnoIcuDataIncluded)` properties — so a build that somehow sees both packages (or an upstream `Uno.icu-*` package) still embeds `icudt.dat` exactly once.

## Sample Usage

These are runtime-binaries packages. Add the one matching your target OS and the ICU native libraries are made available to the runtime via the standard NuGet `runtimes/<rid>/native/` mechanism.

```bash
# Windows apps:
dotnet add package CodeBrix.Platform.Unicode.ApacheLicenseForever

# macOS apps:
dotnet add package CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever
```

For Uno-fork apps that previously used `Uno.icu-win` or `Uno.icu-macos`, the migration is a NuGet-reference swap — no managed-API change is required, since neither these packages nor their upstreams expose a managed API.

### Migrating from Uno.icu-win

Replace the `<PackageReference Include="Uno.icu-win" Version="77.1.2" />` line in your csproj with `<PackageReference Include="CodeBrix.Platform.Unicode.ApacheLicenseForever" Version="{current version}" />`. The MSBuild property names `$(IsUnoHead)` and `$(UnoIcuDataIncluded)` are preserved verbatim from upstream so the embedded-resource trigger fires identically.

### Migrating from Uno.icu-macos

Replace the `<PackageReference Include="Uno.icu-macos" Version="77.1.2" />` line in your csproj with `<PackageReference Include="CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever" Version="{current version}" />`. The same upstream MSBuild property names are preserved verbatim.

## License

The library packaging, the `.targets` files, and the wrapper assemblies are licensed under the Apache License, Version 2.0. see: https://en.wikipedia.org/wiki/Apache_License

The ICU 77 native binaries (`icuuc77.dll`, `icudt77.dll`, `libicuuc.dylib`, `libicudata.dylib`, `icudt.dat`) themselves are licensed under the Unicode License, Version 3 — see the bundled `UNICODE-LICENSE.txt` file. Both NuGet packages are published under the SPDX expression `Apache-2.0 AND Unicode-3.0`.

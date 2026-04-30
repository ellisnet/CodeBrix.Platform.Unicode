# CodeBrix.Platform.Unicode

A redistribution of the International Components for Unicode (ICU) version 77 native binaries for Windows, packaged as a CodeBrix-family NuGet library for .NET 10 applications.
CodeBrix.Platform.Unicode is a namespace-renamed, .NET 10 port of `Uno.icu-win` 77.1.2 — intended as a drop-in replacement for that package in CodeBrix.Platform-forked Uno applications, and usable in any .NET 10 project that needs the ICU 77 native runtime on Windows.
The library has no managed dependencies other than .NET, and is provided as a .NET 10 library and associated `CodeBrix.Platform.Unicode.ApacheLicenseForever` NuGet package.

CodeBrix.Platform.Unicode supports applications and assemblies that target Microsoft .NET version 10.0 and later.
Microsoft .NET version 10.0 is a Long-Term Supported (LTS) version of .NET, and was released on Nov 11, 2025; and will be actively supported by Microsoft until Nov 14, 2028.
Please update your C#/.NET code and projects to the latest LTS version of Microsoft .NET.

## CodeBrix.Platform.Unicode supports:

* The ICU 77 "common" native runtime DLL (`icuuc77.dll`) for Windows on x64 and ARM64 — same `.dll` that upstream `Uno.icu-win` 77.1.2 ships, byte-for-byte.
* The ICU 77 data shim DLL (`icudt77.dll`) for both Windows architectures.
* The full ICU 77 data archive (`icudt.dat`, ~5.5 MB) — covering Unicode collation, normalization, BiDi, character properties, locale (CLDR) data, time zones, transliteration, and the rest of ICU's runtime data.
* A `buildTransitive` MSBuild `.targets` pair that automatically embeds `icudt.dat` as an `<EmbeddedResource>` at consumer-build time, when the consumer project is an Uno head (`$(IsUnoHead) == 'True'`) and the data has not yet been embedded by another package in the same build (`$(UnoIcuDataIncluded) != 'true'`).

## Sample Usage

This is a runtime-binaries package. Add it to your project and the ICU native DLLs are made available to the runtime via the standard NuGet `runtimes/<rid>/native/` mechanism.

```bash
dotnet add package CodeBrix.Platform.Unicode.ApacheLicenseForever
```

For Uno-fork apps that previously used `Uno.icu-win`, the migration is a NuGet-reference swap plus the namespace rename in any direct path references — no managed-API change is required, since neither this package nor upstream `Uno.icu-win` exposes a managed API.

### Migrating from Uno.icu-win

Replace the `<PackageReference Include="Uno.icu-win" Version="77.1.2" />` line in your csproj with `<PackageReference Include="CodeBrix.Platform.Unicode.ApacheLicenseForever" Version="1.0.117" />`. The MSBuild property names `$(IsUnoHead)` and `$(UnoIcuDataIncluded)` are preserved verbatim from upstream so the embedded-resource trigger fires identically.

## License

The library packaging, the `.targets` files, and the wrapper assembly are licensed under the Apache License, Version 2.0. see: https://en.wikipedia.org/wiki/Apache_License

The ICU 77 native binaries (`icuuc77.dll`, `icudt77.dll`, `icudt.dat`) themselves are licensed under the Unicode License, Version 3 — see the bundled `UNICODE-LICENSE.txt` file. The combined NuGet package is published under the SPDX expression `Apache-2.0 AND Unicode-3.0`.

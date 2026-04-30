using System;
using System.IO;

namespace CodeBrix.Platform.Unicode.Tests;

internal static class TestAssetPaths
{
    public static string TestAssetsRoot { get; } =
        Path.Combine(AppContext.BaseDirectory, "TestAssets");

    public static string RuntimesRoot { get; } =
        Path.Combine(TestAssetsRoot, "runtimes");

    public static string WinX64NativeFolder { get; } =
        Path.Combine(RuntimesRoot, "win-x64", "native");

    public static string WinArm64NativeFolder { get; } =
        Path.Combine(RuntimesRoot, "win-arm64", "native");

    public static string BuildTransitiveFolder { get; } =
        Path.Combine(TestAssetsRoot, "buildTransitive");

    public static string DataArchivePath { get; } =
        Path.Combine(BuildTransitiveFolder, "icudt.dat");

    public static string MainTargetsPath { get; } =
        Path.Combine(BuildTransitiveFolder, "CodeBrix.Platform.Unicode.ApacheLicenseForever.targets");

    public static string CommonTargetsPath { get; } =
        Path.Combine(BuildTransitiveFolder, "CodeBrix.Platform.Unicode.Common.targets");
}

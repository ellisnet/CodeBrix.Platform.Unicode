using System;
using System.IO;

namespace CodeBrix.Platform.UnicodeMacOs.Tests;

internal static class TestAssetPaths
{
    public static string TestAssetsRoot { get; } =
        Path.Combine(AppContext.BaseDirectory, "TestAssets");

    public static string RuntimesRoot { get; } =
        Path.Combine(TestAssetsRoot, "runtimes");

    public static string OsxNativeFolder { get; } =
        Path.Combine(RuntimesRoot, "osx", "native");

    public static string BuildTransitiveFolder { get; } =
        Path.Combine(TestAssetsRoot, "buildTransitive");

    public static string DataArchivePath { get; } =
        Path.Combine(BuildTransitiveFolder, "icudt.dat");

    public static string MainTargetsPath { get; } =
        Path.Combine(BuildTransitiveFolder, "CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever.targets");

    public static string CommonTargetsPath { get; } =
        Path.Combine(BuildTransitiveFolder, "CodeBrix.Platform.UnicodeMacOs.Common.targets");
}

using System.IO;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Platform.Unicode.Tests;

public class NativeBinaryPresenceTests
{
    [Fact]
    public void WinX64_icuuc77_dll_is_present()
        => File.Exists(Path.Combine(TestAssetPaths.WinX64NativeFolder, "icuuc77.dll")).Should().BeTrue();

    [Fact]
    public void WinX64_icudt77_dll_is_present()
        => File.Exists(Path.Combine(TestAssetPaths.WinX64NativeFolder, "icudt77.dll")).Should().BeTrue();

    [Fact]
    public void WinArm64_icuuc77_dll_is_present()
        => File.Exists(Path.Combine(TestAssetPaths.WinArm64NativeFolder, "icuuc77.dll")).Should().BeTrue();

    [Fact]
    public void WinArm64_icudt77_dll_is_present()
        => File.Exists(Path.Combine(TestAssetPaths.WinArm64NativeFolder, "icudt77.dll")).Should().BeTrue();

    [Fact]
    public void WinX64_icuuc77_dll_starts_with_PE_magic()
    {
        //Arrange
        var path = Path.Combine(TestAssetPaths.WinX64NativeFolder, "icuuc77.dll");

        //Act
        var firstTwoBytes = ReadFirstBytes(path, 2);

        //Assert
        firstTwoBytes[0].Should().Be((byte)'M');
        firstTwoBytes[1].Should().Be((byte)'Z');
    }

    [Fact]
    public void WinArm64_icuuc77_dll_starts_with_PE_magic()
    {
        //Arrange
        var path = Path.Combine(TestAssetPaths.WinArm64NativeFolder, "icuuc77.dll");

        //Act
        var firstTwoBytes = ReadFirstBytes(path, 2);

        //Assert
        firstTwoBytes[0].Should().Be((byte)'M');
        firstTwoBytes[1].Should().Be((byte)'Z');
    }

    [Fact]
    public void WinX64_icuuc77_dll_size_is_realistic()
    {
        //Arrange
        var info = new FileInfo(Path.Combine(TestAssetPaths.WinX64NativeFolder, "icuuc77.dll"));

        //Assert — ICU 77 common library is several MB; require at least 1 MB
        info.Length.Should().BeGreaterThan(1_000_000L);
    }

    [Fact]
    public void WinArm64_icuuc77_dll_size_is_realistic()
    {
        //Arrange
        var info = new FileInfo(Path.Combine(TestAssetPaths.WinArm64NativeFolder, "icuuc77.dll"));

        //Assert
        info.Length.Should().BeGreaterThan(1_000_000L);
    }

    [Fact]
    public void icudt77_data_shim_dll_is_small()
    {
        //Arrange
        var x64 = new FileInfo(Path.Combine(TestAssetPaths.WinX64NativeFolder, "icudt77.dll"));
        var arm = new FileInfo(Path.Combine(TestAssetPaths.WinArm64NativeFolder, "icudt77.dll"));

        //Assert — the data SHIM dll is a tiny stub; the real data is in icudt.dat
        x64.Length.Should().BeLessThan(100_000L);
        arm.Length.Should().BeLessThan(100_000L);
    }

    private static byte[] ReadFirstBytes(string path, int count)
    {
        using var stream = File.OpenRead(path);
        var buffer = new byte[count];
        var read = stream.Read(buffer, 0, count);
        read.Should().Be(count);
        return buffer;
    }
}

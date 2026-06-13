using System.IO;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Platform.UnicodeMacOs.Tests;

public class NativeBinaryPresenceTests
{
    [Fact]
    public void Osx_libicuuc_dylib_is_present()
        => File.Exists(Path.Combine(TestAssetPaths.OsxNativeFolder, "libicuuc.dylib")).Should().BeTrue();

    [Fact]
    public void Osx_libicudata_dylib_is_present()
        => File.Exists(Path.Combine(TestAssetPaths.OsxNativeFolder, "libicudata.dylib")).Should().BeTrue();

    [Fact]
    public void Osx_libicuuc_dylib_starts_with_universal_macho_magic()
    {
        //Arrange — both dylibs are universal (x86_64 + arm64) Mach-O binaries;
        // FAT/universal binaries start with the big-endian magic 0xCA 0xFE 0xBA 0xBE
        var path = Path.Combine(TestAssetPaths.OsxNativeFolder, "libicuuc.dylib");

        //Act
        var firstFourBytes = ReadFirstBytes(path, 4);

        //Assert
        firstFourBytes[0].Should().Be((byte)0xCA);
        firstFourBytes[1].Should().Be((byte)0xFE);
        firstFourBytes[2].Should().Be((byte)0xBA);
        firstFourBytes[3].Should().Be((byte)0xBE);
    }

    [Fact]
    public void Osx_libicudata_dylib_starts_with_universal_macho_magic()
    {
        //Arrange
        var path = Path.Combine(TestAssetPaths.OsxNativeFolder, "libicudata.dylib");

        //Act
        var firstFourBytes = ReadFirstBytes(path, 4);

        //Assert
        firstFourBytes[0].Should().Be((byte)0xCA);
        firstFourBytes[1].Should().Be((byte)0xFE);
        firstFourBytes[2].Should().Be((byte)0xBA);
        firstFourBytes[3].Should().Be((byte)0xBE);
    }

    [Fact]
    public void Osx_libicuuc_dylib_size_is_realistic()
    {
        //Arrange
        var info = new FileInfo(Path.Combine(TestAssetPaths.OsxNativeFolder, "libicuuc.dylib"));

        //Assert — the universal ICU 77 common library is ~4 MB; require at least 1 MB
        info.Length.Should().BeGreaterThan(1_000_000L);
    }

    [Fact]
    public void libicudata_data_shim_dylib_is_small()
    {
        //Arrange
        var info = new FileInfo(Path.Combine(TestAssetPaths.OsxNativeFolder, "libicudata.dylib"));

        //Assert — the data SHIM dylib is a tiny stub; the real data is in icudt.dat
        info.Length.Should().BeLessThan(100_000L);
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

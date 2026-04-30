using System.IO;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Platform.Unicode.Tests;

public class DataArchivePresenceTests
{
    [Fact]
    public void Data_archive_icudt_dat_is_present()
        => File.Exists(TestAssetPaths.DataArchivePath).Should().BeTrue();

    [Fact]
    public void Data_archive_size_is_realistic()
    {
        //Arrange
        var info = new FileInfo(TestAssetPaths.DataArchivePath);

        //Assert — the upstream ICU 77 data archive is ~5.5 MB; require at least 4 MB
        info.Length.Should().BeGreaterThan(4_000_000L);
    }

    [Fact]
    public void Data_archive_size_is_not_unreasonable()
    {
        //Arrange
        var info = new FileInfo(TestAssetPaths.DataArchivePath);

        //Assert — guard against accidental reapplication of debug or symbol bloat
        info.Length.Should().BeLessThan(20_000_000L);
    }

    [Fact]
    public void Data_archive_starts_with_icu_data_magic()
    {
        //Arrange — ICU data files use a "DataHeader" — first byte is the size of
        // the header, and then bytes 2-3 are the magic word 0xDA 0x27.
        using var stream = File.OpenRead(TestAssetPaths.DataArchivePath);
        var header = new byte[4];
        var read = stream.Read(header, 0, 4);

        //Assert
        read.Should().Be(4);
        header[2].Should().Be((byte)0xDA);
        header[3].Should().Be((byte)0x27);
    }
}

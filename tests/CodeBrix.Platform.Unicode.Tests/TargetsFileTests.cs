using System.IO;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Platform.Unicode.Tests;

public class TargetsFileTests
{
    [Fact]
    public void Main_targets_file_is_present()
        => File.Exists(TestAssetPaths.MainTargetsPath).Should().BeTrue();

    [Fact]
    public void Common_targets_file_is_present()
        => File.Exists(TestAssetPaths.CommonTargetsPath).Should().BeTrue();

    [Fact]
    public void Main_targets_imports_common_targets()
    {
        //Arrange
        var content = File.ReadAllText(TestAssetPaths.MainTargetsPath);

        //Assert
        content.Should().Contain("<Import Project=");
        content.Should().Contain("CodeBrix.Platform.Unicode.Common.targets");
    }

    [Fact]
    public void Common_targets_declares_codebrix_target_name()
    {
        //Arrange
        var content = File.ReadAllText(TestAssetPaths.CommonTargetsPath);

        //Assert
        content.Should().Contain("Name=\"AddCodeBrixPlatformUnicodeEmbeddedResource\"");
    }

    [Fact]
    public void Common_targets_runs_before_compile()
    {
        //Arrange
        var content = File.ReadAllText(TestAssetPaths.CommonTargetsPath);

        //Assert
        content.Should().Contain("BeforeTargets=\"BeforeBuild;BeforeCompile\"");
    }

    [Fact]
    public void Common_targets_preserves_isunohead_property_reference()
    {
        //Arrange
        var content = File.ReadAllText(TestAssetPaths.CommonTargetsPath);

        //Assert — verbatim from upstream so cross-package guard still works
        content.Should().Contain("'$(IsUnoHead)' == 'True'");
    }

    [Fact]
    public void Common_targets_preserves_unoicudataincluded_sentinel()
    {
        //Arrange
        var content = File.ReadAllText(TestAssetPaths.CommonTargetsPath);

        //Assert — verbatim from upstream so a consumer with both packages still embeds icudt.dat exactly once
        content.Should().Contain("'$(UnoIcuDataIncluded)' != 'true'");
        content.Should().Contain("<UnoIcuDataIncluded>true</UnoIcuDataIncluded>");
    }

    [Fact]
    public void Common_targets_embeds_icudt_dat_resource()
    {
        //Arrange
        var content = File.ReadAllText(TestAssetPaths.CommonTargetsPath);

        //Assert
        content.Should().Contain("<EmbeddedResource Include=");
        content.Should().Contain("icudt.dat");
    }

    [Fact]
    public void Common_targets_does_not_declare_upstream_target_name()
    {
        //Arrange
        var content = File.ReadAllText(TestAssetPaths.CommonTargetsPath);

        //Assert — the inner target was renamed. The bare token may still appear
        // in the provenance comment header, but the actual <Target Name="..."> declaration
        // must NOT use the upstream name.
        content.Should().NotContain("Name=\"AddUnoIcuEmbeddedResource\"");
    }
}

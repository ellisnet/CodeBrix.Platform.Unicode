using System.IO;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Platform.UnicodeMacOs.Tests;

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
        content.Should().Contain("CodeBrix.Platform.UnicodeMacOs.Common.targets");
    }

    [Fact]
    public void Common_targets_declares_codebrix_target_name()
    {
        //Arrange
        var content = File.ReadAllText(TestAssetPaths.CommonTargetsPath);

        //Assert
        content.Should().Contain("Name=\"AddCodeBrixPlatformUnicodeMacOsEmbeddedResource\"");
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
    public void Common_targets_gates_on_iscodebrixhead()
    {
        //Arrange
        var content = File.ReadAllText(TestAssetPaths.CommonTargetsPath);

        //Assert — embedding is gated on a CodeBrix.Platform head
        content.Should().Contain("'$(IsCodeBrixHead)' == 'true'");
    }

    [Fact]
    public void Common_targets_uses_codebrixicudataincluded_sentinel()
    {
        //Arrange
        var content = File.ReadAllText(TestAssetPaths.CommonTargetsPath);

        //Assert — the shared sentinel ensures a consumer with multiple icu packages embeds icudt.dat exactly once
        content.Should().Contain("'$(CodeBrixIcuDataIncluded)' != 'true'");
        content.Should().Contain("<CodeBrixIcuDataIncluded>true</CodeBrixIcuDataIncluded>");
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

    [Fact]
    public void Common_targets_does_not_declare_sibling_windows_target_name()
    {
        //Arrange
        var content = File.ReadAllText(TestAssetPaths.CommonTargetsPath);

        //Assert — distinct from the sibling CodeBrix.Platform.Unicode (Windows)
        // package's target so a consumer importing both gets no redefinition.
        content.Should().NotContain("Name=\"AddCodeBrixPlatformUnicodeEmbeddedResource\"");
    }
}

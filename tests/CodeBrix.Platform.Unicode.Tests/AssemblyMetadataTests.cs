using System.Reflection;
using System.Runtime.Versioning;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Platform.Unicode.Tests;

public class AssemblyMetadataTests
{
    private const string ExpectedAssemblyName = "CodeBrix.Platform.Unicode";

    [Fact]
    public void Library_assembly_can_be_loaded_by_name()
    {
        //Arrange/Act
        var asm = Assembly.Load(ExpectedAssemblyName);

        //Assert
        asm.Should().NotBeNull();
    }

    [Fact]
    public void Library_assembly_simple_name_matches()
    {
        //Arrange
        var asm = Assembly.Load(ExpectedAssemblyName);

        //Act
        var simpleName = asm.GetName().Name;

        //Assert
        simpleName.Should().Be(ExpectedAssemblyName);
    }

    [Fact]
    public void Library_assembly_targets_net10()
    {
        //Arrange
        var asm = Assembly.Load(ExpectedAssemblyName);

        //Act
        var attr = asm.GetCustomAttribute<TargetFrameworkAttribute>();

        //Assert
        attr.Should().NotBeNull();
        attr.FrameworkName.Should().StartWith(".NETCoreApp,Version=v10.");
    }

    [Fact]
    public void Library_assembly_exports_no_public_types()
    {
        //Arrange
        var asm = Assembly.Load(ExpectedAssemblyName);

        //Act
        var exportedTypes = asm.GetExportedTypes();

        //Assert
        exportedTypes.Should().BeEmpty();
    }
}

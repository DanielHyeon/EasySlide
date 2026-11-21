using System.Reflection;
using Axicultus.Wpf;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Startup;

public class OutputArtifactTests
{
    [Fact]
    public void Wpf_Binaries_Use_Axicultus_Identity()
    {
        var assembly = typeof(App).Assembly;
        assembly.GetName().Name.Should().Be("Axicultus");
        (assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product).Should().Be("Axicultus");
        (assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title).Should().Be("Axicultus");
        typeof(OutputArtifactTests).Assembly.GetName().Name.Should().Be("Axicultus.Tests");
    }

    [Fact]
    public void Wpf_Namespace_Stays_AxicultusWpf()
    {
        // AssemblyName 변경이 코드 네임스페이스까지 바꾸지 않았는지 확인.
        typeof(App).Namespace.Should().Be("Axicultus.Wpf");
    }
}

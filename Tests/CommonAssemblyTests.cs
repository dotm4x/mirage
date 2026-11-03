using System.Reflection;

namespace Mirage.Tests;

public class CommonAssemblyTests
{
    [Fact]
    public void CommonAssembly_CanBeLoaded()
    {
        var assembly = Assembly.Load("Mirage.Common");

        Assert.NotNull(assembly);
    }
}

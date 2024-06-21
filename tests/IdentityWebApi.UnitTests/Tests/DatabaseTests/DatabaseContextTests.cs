using IdentityWebApi.UnitTests.Infrastructure;

using NUnit.Framework;
using NUnit.Framework.Legacy;

using System.Threading.Tasks;

namespace IdentityWebApi.UnitTests.Tests.DatabaseTests;

public class DatabaseContextTests : SqliteConfiguration
{
    [Test]
    [Category("Positive")]
    public async Task ShouldConnectToDbSuccessfully()
    {
        // Arrange & Act
        var result = await this.DatabaseContext.Database.CanConnectAsync();

        // Assert
        ClassicAssert.True(result);
    }
}

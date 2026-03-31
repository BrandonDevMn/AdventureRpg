using AdventureRpg.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace UnitTests.Controllers;

public class HealthControllerTests
{
    [Fact]
    public void Get_ReturnsOk()
    {
        var result = new HealthController().Get();

        Assert.IsType<OkObjectResult>(result);
    }
}

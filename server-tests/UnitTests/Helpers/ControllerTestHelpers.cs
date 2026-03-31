using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UnitTests.Helpers;

public static class ControllerTestHelpers
{
    public static void SetUser(this ControllerBase controller, string userId)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId)
                ], "test"))
            }
        };
    }
}

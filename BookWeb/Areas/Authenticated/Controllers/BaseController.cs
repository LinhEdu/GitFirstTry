using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace BookWeb.Areas.Authenticated.Controllers;

public class BaseController : Controller
{
    public string? GetCurrentUserId()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        return claims?.Value;
    }
}
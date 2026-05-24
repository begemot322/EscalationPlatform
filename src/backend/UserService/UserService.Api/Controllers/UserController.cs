using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Common.Interfaces.Identity;
using UserService.Application.Services.Interfaces;

namespace UserService.Api.Controllers;

[Authorize]
public class UserController : BaseController
{
    private readonly IUserService _userService;
    private readonly IUserContext _userContext;

    public UserController(IUserService userService, IUserContext userContext)
    {
        _userService = userService;
        _userContext = userContext;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var result = await _userService.GetCurrentAsync(_userContext.UserId, ct);
        return OkOrError(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _userService.GetByIdAsync(id, ct);
        return OkOrError(result);
    }
}

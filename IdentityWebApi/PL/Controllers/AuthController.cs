using IdentityWebApi.BL.Constants;
using IdentityWebApi.BL.Enums;
using IdentityWebApi.BL.Interfaces;
using IdentityWebApi.PL.Models.Action;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using System.Threading.Tasks;

namespace IdentityWebApi.PL.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;
    private readonly IClaimsService _claimsService;

    public AuthController(IAuthService authService, IEmailService emailService, IClaimsService claimsService)
    {
        _authService = authService;
        _emailService = emailService;
        _claimsService = claimsService;
    }

    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUpUser([FromBody, BindRequired] UserRegistrationActionModel userModel)
    {
        var creationResult = await _authService.SignUpUserAsync(userModel);
        
        if (creationResult.Result is ServiceResultType.NotFound)
        {
            return NotFound(creationResult.Message);
        }

        var confirmationLink = Url.Action(
            "ConfirmEmail", 
            "Auth",
            new { email = creationResult.Data.userDto.Email, creationResult.Data.token }, 
            Request.Scheme
        );

        await _emailService.SendEmailAsync(
            creationResult.Data.userDto.Email, 
            EmailSubjects.AccountConfirmation,
            $"<a href='{confirmationLink}'>confirm</a>"
        );

        var getUserLink = Url.Action(
            "GetUser", 
            "User", 
            new { id = creationResult.Data.userDto.Id }, 
            Request.Scheme
        );

        return Created(getUserLink!, creationResult.Data.userDto);
    }

    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody, BindRequired] UserSignInActionModel userModel)
    {
        var signInResult = await _authService.SignInUserAsync(userModel);
        
        if (signInResult.Result is ServiceResultType.InvalidData)
        {
            return BadRequest(signInResult.Message);
        }

        var claims = _claimsService.AssignClaims(signInResult.Data);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claims);

        return Ok(signInResult.Data);
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery, BindRequired] string email, [FromQuery, BindRequired] string token)
    {
        var confirmationResult = await _authService.ConfirmUserEmailAsync(email, token);
     
        if (confirmationResult.Result is not ServiceResultType.Success)
        {
            return BadRequest(confirmationResult.Message);
        }

        return NoContent();
    }
}
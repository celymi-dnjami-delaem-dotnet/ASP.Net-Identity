using IdentityWebApi.ApplicationLogic.Models.Action;
using IdentityWebApi.Core.Constants;
using IdentityWebApi.Core.Interfaces.ApplicationLogic;
using IdentityWebApi.Core.Interfaces.Presentation;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using System.Threading.Tasks;

namespace IdentityWebApi.Presentation.Controllers;

/// <summary>
/// Authentication controller.
/// </summary>
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;
    private readonly IEmailService emailService;
    private readonly IClaimsService claimsService;
    private readonly IHttpContextService httpContextService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="authService"><see cref="IAuthService"/>.</param>
    /// <param name="emailService"><see cref="IEmailService"/>.</param>
    /// <param name="claimsService"><see cref="IClaimsService"/>.</param>
    /// <param name="httpContextService"><see cref="IHttpContextService"/>.</param>
    public AuthController(
        IAuthService authService,
        IEmailService emailService,
        IClaimsService claimsService,
        IHttpContextService httpContextService
    )
    {
        this.authService = authService;
        this.emailService = emailService;
        this.claimsService = claimsService;
        this.httpContextService = httpContextService;
    }

    /// <summary>
    /// User account registration.
    /// </summary>
    /// <param name="userModel"><see cref="UserRegistrationDto"/>.</param>
    /// <response code="201">Created user.</response>
    /// <response code="404">Unable to create user due to missing role.</response>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPost("sign-up")]
    [ProducesResponseType(typeof(UserResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SignUpUser([FromBody, BindRequired] UserRegistrationDto userModel)
    {
        var creationResult = await this.authService.SignUpUserAsync(userModel);

        if (creationResult.IsResultNotFound)
        {
            // todo: replace 404 with 400
            return this.GetFailedResponseByServiceResult(creationResult);
        }

        var confirmationLink = this.httpContextService.GenerateConfirmEmailLink(
            creationResult.Data.userDto.Email,
            creationResult.Data.token
        );

        await this.emailService.SendEmailAsync(
            creationResult.Data.userDto.Email,
            EmailSubjects.AccountConfirmation,
            $"<a href='{confirmationLink}'>confirm</a>"
        );

        var getUserLink = this.httpContextService.GenerateGetUserLink(creationResult.Data.userDto.Id);

        return this.Created(getUserLink, creationResult.Data.userDto);
    }

    /// <summary>
    /// User account authentication.
    /// </summary>
    /// <param name="userModel"><see cref="UserSignInDto"/>.</param>
    /// <response code="200">User has authenticated.</response>
    /// <response code="400">Unable to authenticate with provided credentials.</response>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPost("sign-in")]
    [ProducesResponseType(typeof(UserResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResultDto>> SignIn([FromBody, BindRequired] UserSignInDto userModel)
    {
        var signInResult = await this.authService.SignInUserAsync(userModel);

        if (signInResult.IsResultFailed)
        {
            return this.GetFailedResponseByServiceResult(signInResult);
        }

        var claims = this.claimsService.AssignClaims(signInResult.Data);

        await this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claims);

        return signInResult.Data;
    }

    /// <summary>
    /// User account email confirmation.
    /// </summary>
    /// <param name="email">User email.</param>
    /// <param name="token">Confirmation token.</param>
    /// <response code="204">Email has been confirmed.</response>
    /// <response code="404">User with provided email is not found.</response>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet("confirm-email")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery, BindRequired] string email,
        [FromQuery, BindRequired] string token
    )
    {
        var confirmationResult = await this.authService.ConfirmUserEmailAsync(email, token);

        if (confirmationResult.IsResultFailed)
        {
            return this.GetFailedResponseByServiceResult(confirmationResult);
        }

        return this.NoContent();
    }
}
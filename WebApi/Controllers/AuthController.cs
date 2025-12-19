using AutoMapper;
using DAL.Models;
using DAL.Security;
using DAL.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs.Auth;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _service;
        private readonly IMapper _mapper;

        public AuthController(
            IConfiguration configuration,
            IUserService service,
            IMapper mapper)
        {
            _configuration = configuration;
            _service = service;
            _mapper = mapper;
        }


        [HttpPost("register")]
        public ActionResult<UserRegisterDto> Register([FromBody] UserRegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var username = dto.UserName.Trim();

                if (_service.GetByUsername(username) != null)
                    return BadRequest("Username already taken");

    
                var user = _mapper.Map<User>(dto);

                user.UserName = username;
                user.Email = dto.Email.Trim();
                user.FirstName = dto.FirstName?.Trim();
                user.LastName = dto.LastName?.Trim();
                user.Phone = dto.Phone?.Trim();
                user.Role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role.Trim();
                user.IsActive = true;

                _service.Register(user, dto.Password);

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody] UserLoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var genericMessage = "Incorrect username or password";
                var input = dto.UserNameOrEmail.Trim();

                var user = _service.Login(input, dto.Password);
                if (user == null)
                    return BadRequest(genericMessage);

                var secureKey = _configuration["Jwt:SecureKey"];
                var expirationMinutes =
                    int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

                var token = JwtTokenProvider.CreateToken(
                    secureKey,
                    expirationMinutes,
                    user.UserName,
                    user.Role
                );

                return Ok(token);
            }
            catch
            {
                return BadRequest("Incorrect username or password");
            }
        }

        // =========================
        // CHANGE PASSWORD
        // =========================
        [Authorize]
        [HttpPost("changepassword")]
        public ActionResult ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _service.ChangePassword(
                    dto.UserName.Trim(),
                    dto.OldPassword,
                    dto.NewPassword
                );

                return Ok("Password changed successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

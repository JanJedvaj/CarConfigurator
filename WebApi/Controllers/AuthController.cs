using AutoMapper;
using DAL.Models;
using DAL.Repositories.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs.Auth;
using WebApi.Security;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public AuthController(IConfiguration configuration, IUserRepository userRepository, IMapper mapper)
        {
            _configuration = configuration;
            _userRepository = userRepository;
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
                var email = dto.Email.Trim();

                if (_userRepository.ExistsUsername(username))
                    return BadRequest("Username already taken");

                if (_userRepository.ExistsEmail(email))
                    return BadRequest("Email already taken");

                var user = _mapper.Map<User>(dto);

                user.UserName = username;
                user.Email = email;
                user.FirstName = dto.FirstName?.Trim();
                user.LastName = dto.LastName?.Trim();
                user.Phone = dto.Phone?.Trim();
                user.Role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role.Trim();
                user.IsActive = true;
                user.CreatedAt = DateTime.UtcNow;

                var salt = PasswordHashProvider.GetSalt();
                var hash = PasswordHashProvider.GetHash(dto.Password, salt);

                user.PasswordSalt = salt;
                user.PasswordHash = hash;

                _userRepository.Add(user);

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

                var user = input.Contains("@")
                    ? _userRepository.GetByEmail(input)
                    : _userRepository.GetByUsername(input);

                if (user == null || !user.IsActive)
                    return BadRequest(genericMessage);

                var computedHash = PasswordHashProvider.GetHash(dto.Password, user.PasswordSalt);
                if (!string.Equals(computedHash, user.PasswordHash, StringComparison.Ordinal))
                    return BadRequest(genericMessage);

                user.LastLoginAt = DateTime.UtcNow;
                _userRepository.Update(user);

                var secureKey = _configuration["Jwt:SecureKey"];
                var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

                var token = JwtTokenProvider.CreateToken(
                    secureKey!,
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

        [Authorize]
        [HttpPost("changepassword")]
        public ActionResult ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var username = dto.UserName.Trim();
                var user = _userRepository.GetByUsername(username);

                if (user == null)
                    return BadRequest("User not found.");

                var currentHash = PasswordHashProvider.GetHash(dto.OldPassword, user.PasswordSalt);
                if (!string.Equals(currentHash, user.PasswordHash, StringComparison.Ordinal))
                    return BadRequest("Old password is incorrect.");

                if (dto.NewPassword.Length < 8)
                    return BadRequest("New password should be at least 8 characters long.");

                var newSalt = PasswordHashProvider.GetSalt();
                var newHash = PasswordHashProvider.GetHash(dto.NewPassword, newSalt);

                user.PasswordSalt = newSalt;
                user.PasswordHash = newHash;

                _userRepository.Update(user);

                return Ok("Password changed successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

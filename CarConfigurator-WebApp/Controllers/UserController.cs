using AutoMapper;
using CarConfigurator_WebApp.Security;
using CarConfigurator_WebApp.ViewModels;
using DAL.Models;
using DAL.Repositories.Users;
using DAL.Services.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarConfigurator_WebApp.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;   // AUTH radi direktno preko repo
        private readonly IUserService _userService;         
        private readonly IMapper _mapper;

        public UserController(IUserRepository userRepository, IUserService userService, IMapper mapper)
        {
            _userRepository = userRepository;
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var input = vm.UserNameOrEmail.Trim();

                var user = input.Contains("@")
                    ? _userRepository.GetByEmail(input)
                    : _userRepository.GetByUsername(input);

                if (user == null)
                    throw new InvalidOperationException("Invalid credentials.");

                if (!user.IsActive)
                    throw new InvalidOperationException("User is not active.");

                var computedHash = PasswordHashProvider.GetHash(vm.Password, user.PasswordSalt);
                if (!string.Equals(computedHash, user.PasswordHash, StringComparison.Ordinal))
                    throw new InvalidOperationException("Invalid credentials.");

                user.LastLoginAt = DateTime.UtcNow;
                _userRepository.Update(user);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.Role, user.Role ?? "User")
                };

                var identity = new ClaimsIdentity(claims, "Cookies");
                var principal = new ClaimsPrincipal(identity);

                HttpContext.SignInAsync(principal).GetAwaiter().GetResult();

                if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                    return Redirect(vm.ReturnUrl);

                if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction("Index", "Components");

                return RedirectToAction("Index", "Items");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Došlo je do pogreške prilikom prijave.");
                return View(vm);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User?.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View(new RegisterVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var username = vm.UserName.Trim();
                var email = vm.Email.Trim();

                if (_userRepository.ExistsUsername(username))
                    throw new InvalidOperationException("Username already exists.");

                if (_userRepository.ExistsEmail(email))
                    throw new InvalidOperationException("Email already exists.");

                var salt = PasswordHashProvider.GetSalt();
                var hash = PasswordHashProvider.GetHash(vm.Password, salt);

                var user = new User
                {
                    UserName = username,
                    Email = email,
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PasswordSalt = salt,
                    PasswordHash = hash
                };

                _userRepository.Add(user);

                TempData["Success"] = "Registration successful. Please log in.";
                return RedirectToAction(nameof(Login));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Došlo je do greške prilikom registracije.");
                return View(vm);
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordVM());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var username = User.Identity!.Name!;
                var user = _userRepository.GetByUsername(username.Trim());

                if (user == null)
                    throw new InvalidOperationException("User not found.");

                var currentHash = PasswordHashProvider.GetHash(vm.OldPassword, user.PasswordSalt);
                if (!string.Equals(currentHash, user.PasswordHash, StringComparison.Ordinal))
                    throw new InvalidOperationException("Old password is incorrect.");

                var newSalt = PasswordHashProvider.GetSalt();
                var newHash = PasswordHashProvider.GetHash(vm.NewPassword, newSalt);

                user.PasswordSalt = newSalt;
                user.PasswordHash = newHash;

                _userRepository.Update(user);

                TempData["Success"] = "Password changed successfully.";
                return RedirectToAction("Index", "Home");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Došlo je do greške prilikom promjene lozinke.");
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync().GetAwaiter().GetResult();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Forbidden()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Profile()
        {
            var userId = GetUserIdOrThrow();
            var user = _userService.GetUser(userId);
            if (user == null) return NotFound();

            var vm = _mapper.Map<AdminProfileVM>(user);
            return View(vm);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(AdminProfileVM vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Validation error." });

            var userId = GetUserIdOrThrow();
            if (vm.Id != userId) return Forbid();

            try
            {
                var user = _userService.GetUser(userId);
                if (user == null) return NotFound(new { message = "User not found." });

                user.Email = vm.Email.Trim();
                user.FirstName = string.IsNullOrWhiteSpace(vm.FirstName) ? null : vm.FirstName.Trim();
                user.LastName = string.IsNullOrWhiteSpace(vm.LastName) ? null : vm.LastName.Trim();
                user.Phone = string.IsNullOrWhiteSpace(vm.Phone) ? null : vm.Phone.Trim();

                _userService.UpdateUser(user);

                return Ok(new { message = "Profile updated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult UserProfile()
        {
            var userId = GetUserIdOrThrow();
            var user = _userService.GetUser(userId);
            if (user == null) return NotFound();

            var vm = _mapper.Map<AdminProfileVM>(user);
            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateUserProfile(AdminProfileVM vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Validation error." });

            var userId = GetUserIdOrThrow();
            if (vm.Id != userId) return Forbid();

            try
            {
                var user = _userService.GetUser(userId);
                if (user == null) return NotFound(new { message = "User not found." });

                user.Email = vm.Email.Trim();
                user.FirstName = string.IsNullOrWhiteSpace(vm.FirstName) ? null : vm.FirstName.Trim();
                user.LastName = string.IsNullOrWhiteSpace(vm.LastName) ? null : vm.LastName.Trim();
                user.Phone = string.IsNullOrWhiteSpace(vm.Phone) ? null : vm.Phone.Trim();

                _userService.UpdateUser(user);

                return Ok(new { message = "Profile updated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private int GetUserIdOrThrow()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(idClaim) || !int.TryParse(idClaim, out var userId))
                throw new InvalidOperationException("User not authenticated properly.");
            return userId;
        }
    }
}

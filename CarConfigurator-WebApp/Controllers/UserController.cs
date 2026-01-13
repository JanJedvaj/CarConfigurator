using AutoMapper;
using CarConfigurator_WebApp.ViewModels;
using DAL.Services.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarConfigurator_WebApp.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IMapper mapper)
        {
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
                var user = _userService.Login(vm.UserNameOrEmail, vm.Password);

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

                // Redirect nakon login-a
                if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                    return Redirect(vm.ReturnUrl);

                // Role-based redirect
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
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        k => k.Key,
                        v => v.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(new { message = "Validation error.", errors });
            }

            var userId = GetUserIdOrThrow();

            // Admin na ovoj stranici mijenja samo svoj profil
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
            catch
            {
                return StatusCode(500, new { message = "Unexpected error while updating profile." });
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
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        k => k.Key,
                        v => v.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(new { message = "Validation error.", errors });
            }

            var userId = GetUserIdOrThrow();

            // User može mijenjati samo svoj profil
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
            catch
            {
                return StatusCode(500, new { message = "Unexpected error while updating profile." });
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

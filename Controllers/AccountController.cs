using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scholarship.Web.Services.ApiClient;
using Scholarship.Web.ViewModels;

namespace Scholarship.Web.Controllers;

public class AccountController : Controller
{
    private readonly IScholarshipApiClient _apiClient;

    public AccountController(IScholarshipApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AdminLogin(string? returnUrl = null)
    {
        return RedirectToAction("Login", "Admin", new { returnUrl });
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null, string? registeredUserId = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        string captcha = GenerateCaptchaCode();
        HttpContext.Session.SetString("CaptchaCode", captcha);

        var model = new LoginViewModel
        {
            CaptchaToken = captcha,
            UserName = registeredUserId ?? string.Empty
        };
        ViewBag.ReturnUrl = returnUrl;
        ViewBag.RegisteredUserId = registeredUserId;
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        string? storedCaptcha = HttpContext.Session.GetString("CaptchaCode");
        if (string.IsNullOrEmpty(storedCaptcha) || model.CaptchaInput?.Trim().ToUpperInvariant() != storedCaptcha.ToUpperInvariant())
        {
            ModelState.AddModelError("CaptchaInput", "Invalid Captcha entered. Please try again.");
            model.CaptchaToken = GenerateCaptchaCode();
            HttpContext.Session.SetString("CaptchaCode", model.CaptchaToken);
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            model.CaptchaToken = GenerateCaptchaCode();
            HttpContext.Session.SetString("CaptchaCode", model.CaptchaToken);
            return View(model);
        }

        var response = await _apiClient.LoginStudentAsync(model);
        if (response != null && response.Success && response.Data != null)
        {
            var auth = response.Data;
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, auth.StudentId?.ToString() ?? "0"),
                new(ClaimTypes.Name, auth.UserName),
                new(ClaimTypes.Role, auth.Role),
                new("AccessToken", auth.Token),
                new("StudentCode", auth.StudentCode ?? string.Empty),
                new("FullName", auth.FullName ?? auth.UserName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            HttpContext.Session.SetString("JWToken", auth.Token);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard");
        }

        if (response?.Errors != null && response.Errors.Count > 0)
        {
            foreach (var err in response.Errors)
            {
                ModelState.AddModelError(string.Empty, err);
            }
        }
        else
        {
            ModelState.AddModelError(string.Empty, response?.Message ?? "Invalid User ID or Password.");
        }

        model.CaptchaToken = GenerateCaptchaCode();
        HttpContext.Session.SetString("CaptchaCode", model.CaptchaToken);
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Register()
    {
        await LoadRegistrationMastersAsync();
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadRegistrationMastersAsync();
            return View(model);
        }

        var response = await _apiClient.RegisterStudentAsync(model);
        if (response != null && response.Success && response.Data != null)
        {
            string generatedId = response.Data.UserName;
            TempData["SuccessMessage"] = $"Registration successful! Your generated 11-digit User ID is {generatedId}. Please note it down and login.";
            return RedirectToAction("Login", new { registeredUserId = generatedId });
        }

        if (response?.Errors != null && response.Errors.Any())
        {
            foreach (var err in response.Errors)
            {
                ModelState.AddModelError(string.Empty, err);
            }
        }
        else
        {
            ModelState.AddModelError(string.Empty, response?.Message ?? "Registration failed. Please check your details.");
        }

        await LoadRegistrationMastersAsync();
        return View(model);
    }

    [HttpGet]
    public IActionResult RefreshCaptcha()
    {
        string code = GenerateCaptchaCode();
        HttpContext.Session.SetString("CaptchaCode", code);
        return Json(new { captcha = code });
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotUserId()
    {
        return View(new ForgotUserIdViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotUserId(ForgotUserIdViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var response = await _apiClient.ForgotUserIdAsync(model.EmailOrMobile.Trim());
        if (response != null && response.Success && response.Data != null)
        {
            ViewBag.UserIdResult = response.Data;
            return View(model);
        }

        ModelState.AddModelError(string.Empty, response?.Message ?? "No registered student account found with the provided Email ID or Mobile Number.");
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var response = await _apiClient.ForgotPasswordAsync(model.UserName.Trim(), model.MobileOrEmail.Trim());
        if (response != null && response.Success)
        {
            TempData["SuccessMessage"] = response.Message ?? "OTP has been sent to your registered contact.";
            return RedirectToAction("ResetPassword", new { userName = model.UserName.Trim() });
        }

        ModelState.AddModelError(string.Empty, response?.Message ?? "No matching user found with the provided details.");
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string userName)
    {
        return View(new ResetPasswordViewModel { UserName = userName });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var response = await _apiClient.ResetPasswordAsync(model.UserName.Trim(), model.OtpCode.Trim(), model.NewPassword);
        if (response != null && response.Success)
        {
            TempData["SuccessMessage"] = "Your password has been successfully reset. Please login with your new credentials.";
            return RedirectToAction("Login");
        }

        ModelState.AddModelError(string.Empty, response?.Message ?? "Password reset failed. Invalid or expired OTP.");
        return View(model);
    }

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        TempData["SuccessMessage"] = "Password has been updated successfully.";
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    private async Task LoadRegistrationMastersAsync()
    {
        var cats = await _apiClient.GetCategoriesAsync();
        var genders = await _apiClient.GetGendersAsync();
        var religions = await _apiClient.GetReligionsAsync();
        var states = await _apiClient.GetStatesAsync();
        var districts = await _apiClient.GetDistrictsAsync(1); // Chhattisgarh

        ViewBag.Categories = cats?.Data ?? new();
        ViewBag.Genders = genders?.Data ?? new();
        ViewBag.Religions = religions?.Data ?? new();
        ViewBag.States = states?.Data ?? new();
        ViewBag.Districts = districts?.Data ?? new();
    }

    private static string GenerateCaptchaCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new char[5];
        for (int i = 0; i < random.Length; i++)
        {
            random[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
        }
        return new string(random);
    }
}

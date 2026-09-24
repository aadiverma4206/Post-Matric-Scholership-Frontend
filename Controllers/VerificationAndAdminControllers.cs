using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scholarship.Web.Services.ApiClient;
using Scholarship.Web.ViewModels;

namespace Scholarship.Web.Controllers;

[Authorize(Roles = "INSTITUTE_ADMIN,DISTRICT_ADMIN,DEPARTMENT_ADMIN,SUPER_ADMIN")]
public class VerificationController : Controller
{
    private readonly IScholarshipApiClient _apiClient;

    public VerificationController(IScholarshipApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> InstituteQueue(int? statusId, int page = 1, int pageSize = 10)
    {
        ulong instituteId = 1;
        var resp = await _apiClient.GetInstituteQueueAsync(instituteId, statusId, page, pageSize);
        ViewBag.Queue = resp?.Data;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> DistrictQueue(int? statusId, int page = 1, int pageSize = 10)
    {
        ulong districtId = 1;
        var resp = await _apiClient.GetDistrictQueueAsync(districtId, statusId, page, pageSize);
        ViewBag.Queue = resp?.Data;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Review(ulong id, string level = "INSTITUTE")
    {
        ViewBag.ApplicationId = id;
        ViewBag.Level = level;

        var appResp = await _apiClient.GetCurrentApplicationAsync(2);
        var profileResp = await _apiClient.GetProfileAsync();
        var certsResp = await _apiClient.GetCertificatesAsync();
        var acadResp = await _apiClient.GetAcademicDetailsAsync();
        var bankResp = await _apiClient.GetBankAccountAsync();

        ViewBag.Application = appResp?.Data;
        ViewBag.Profile = profileResp?.Data;
        ViewBag.Certificates = certsResp?.Data ?? new();
        ViewBag.Academic = acadResp?.Data;
        ViewBag.Bank = bankResp?.Data;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitVerdict(ScrutinyDecisionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Review", model);
        }

        var resp = await _apiClient.SubmitVerificationDecisionAsync(model.ApplicationId, model.VerificationLevel, model.Status, model.Remarks);
        if (resp != null && resp.Success)
        {
            TempData["SuccessMessage"] = $"Decision '{model.Status}' recorded successfully.";
            return model.VerificationLevel == "DISTRICT" 
                ? RedirectToAction("DistrictQueue") 
                : RedirectToAction("InstituteQueue");
        }

        TempData["ErrorMessage"] = resp?.Message ?? "Failed to record decision.";
        return RedirectToAction("Review", new { id = model.ApplicationId, level = model.VerificationLevel });
    }
}

public class AdminController : Controller
{
    private readonly IScholarshipApiClient _apiClient;

    public AdminController(IScholarshipApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // ==========================================
    // 1. DEDICATED ADMIN / OFFICIAL LOGIN
    // ==========================================
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true && 
            (User.IsInRole("SUPER_ADMIN") || User.IsInRole("DEPARTMENT_ADMIN") || User.IsInRole("DISTRICT_ADMIN") || User.IsInRole("INSTITUTE_ADMIN") || User.IsInRole("VERIFIER")))
        {
            return RedirectToAction("Students");
        }

        string captcha = GenerateCaptchaCode();
        HttpContext.Session.SetString("AdminCaptchaCode", captcha);

        var model = new OfficialLoginViewModel
        {
            CaptchaToken = captcha
        };

        ViewBag.ReturnUrl = returnUrl;
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(OfficialLoginViewModel model, string? returnUrl = null)
    {
        string? storedCaptcha = HttpContext.Session.GetString("AdminCaptchaCode");
        if (string.IsNullOrEmpty(storedCaptcha) || model.CaptchaInput?.Trim().ToUpperInvariant() != storedCaptcha.ToUpperInvariant())
        {
            ModelState.AddModelError("CaptchaInput", "Invalid security captcha entered. Please try again.");
            model.CaptchaToken = GenerateCaptchaCode();
            HttpContext.Session.SetString("AdminCaptchaCode", model.CaptchaToken);
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            model.CaptchaToken = GenerateCaptchaCode();
            HttpContext.Session.SetString("AdminCaptchaCode", model.CaptchaToken);
            return View(model);
        }

        var response = await _apiClient.LoginOfficialAsync(model);
        if (response != null && response.Success && response.Data != null)
        {
            var auth = response.Data;
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, auth.UserId?.ToString() ?? "0"),
                new(ClaimTypes.Name, auth.UserName),
                new(ClaimTypes.Role, auth.Role),
                new("AccessToken", auth.Token),
                new("FullName", auth.FullName ?? auth.UserName),
                new("UserType", "OFFICIAL")
            };

            if (auth.InstituteId.HasValue)
            {
                claims.Add(new Claim("InstituteId", auth.InstituteId.Value.ToString()));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            HttpContext.Session.SetString("JWToken", auth.Token);

            TempData["SuccessMessage"] = $"Welcome to State Portal, {auth.UserName} ({auth.Role})!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Students");
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
            ModelState.AddModelError(string.Empty, response?.Message ?? "Invalid Official User ID or Password. Verification failed.");
        }

        model.CaptchaToken = GenerateCaptchaCode();
        HttpContext.Session.SetString("AdminCaptchaCode", model.CaptchaToken);
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RefreshCaptcha()
    {
        string code = GenerateCaptchaCode();
        HttpContext.Session.SetString("AdminCaptchaCode", code);
        return Json(new { captcha = code });
    }

    // ==========================================
    // 2. ADMIN DASHBOARD OVERVIEW
    // ==========================================
    [Authorize(Roles = "SUPER_ADMIN,DEPARTMENT_ADMIN,DISTRICT_ADMIN,INSTITUTE_ADMIN,VERIFIER,PAYMENT_ADMIN")]
    public async Task<IActionResult> Index()
    {
        var statsResp = await _apiClient.GetAdminStatsAsync();
        var recentStudentsResp = await _apiClient.GetAdminStudentsAsync(null, null, null, null, 1, 6);

        ViewBag.Stats = statsResp?.Data ?? new AdminDashboardStatsSummaryViewModel();
        ViewBag.RecentStudents = recentStudentsResp?.Data?.Items ?? new List<AdminStudentListItemViewModel>();

        return View();
    }

    // ==========================================
    // 3. STUDENT DIRECTORY / LIST VIEW
    // ==========================================
    [Authorize(Roles = "SUPER_ADMIN,DEPARTMENT_ADMIN,DISTRICT_ADMIN,INSTITUTE_ADMIN,VERIFIER,PAYMENT_ADMIN")]
    public async Task<IActionResult> Students(
        string? search, 
        ulong? districtId, 
        int? statusId, 
        uint? categoryId, 
        int page = 1, 
        int pageSize = 10)
    {
        var studentsResp = await _apiClient.GetAdminStudentsAsync(search, districtId, statusId, categoryId, page, pageSize);
        var statsResp = await _apiClient.GetAdminStatsAsync();

        // Masters for Filter Dropdowns
        var districtsResp = await _apiClient.GetDistrictsAsync(1); // Chhattisgarh
        var statusesResp = await _apiClient.GetStatusesAsync();
        var categoriesResp = await _apiClient.GetCategoriesAsync();

        ViewBag.Districts = districtsResp?.Data ?? new();
        ViewBag.Statuses = statusesResp?.Data ?? new();
        ViewBag.Categories = categoriesResp?.Data ?? new();

        ViewBag.Search = search;
        ViewBag.DistrictId = districtId;
        ViewBag.StatusId = statusId;
        ViewBag.CategoryId = categoryId;
        ViewBag.Stats = statsResp?.Data ?? new AdminDashboardStatsSummaryViewModel();

        var pagedData = studentsResp?.Data ?? new PagedResultViewModel<AdminStudentListItemViewModel>
        {
            Items = new(),
            TotalCount = 0,
            PageNumber = page,
            PageSize = pageSize
        };

        return View(pagedData);
    }

    // ==========================================
    // 4. STUDENT DETAILS / COMPLETE DOSSIER
    // ==========================================
    [Authorize(Roles = "SUPER_ADMIN,DEPARTMENT_ADMIN,DISTRICT_ADMIN,INSTITUTE_ADMIN,VERIFIER,PAYMENT_ADMIN")]
    public async Task<IActionResult> StudentDetails(ulong id)
    {
        var resp = await _apiClient.GetAdminStudentDetailsAsync(id);
        if (resp == null || !resp.Success || resp.Data == null)
        {
            TempData["ErrorMessage"] = resp?.Message ?? $"Student record with ID #{id} could not be loaded.";
            return RedirectToAction("Students");
        }

        return View(resp.Data);
    }

    // ==========================================
    // 5. QUICK VIEW MODAL (AJAX)
    // ==========================================
    [Authorize(Roles = "SUPER_ADMIN,DEPARTMENT_ADMIN,DISTRICT_ADMIN,INSTITUTE_ADMIN,VERIFIER,PAYMENT_ADMIN")]
    public async Task<IActionResult> StudentQuickView(ulong id)
    {
        var resp = await _apiClient.GetAdminStudentDetailsAsync(id);
        if (resp == null || !resp.Success || resp.Data == null)
        {
            return NotFound("Student details not found.");
        }

        return PartialView("_StudentDetailsModal", resp.Data);
    }

    // ==========================================
    // 6. MASTER SCHEMES
    // ==========================================
    [Authorize(Roles = "SUPER_ADMIN,DEPARTMENT_ADMIN")]
    public async Task<IActionResult> MasterSchemes()
    {
        var schemes = await _apiClient.GetSchemesAsync();
        ViewBag.Schemes = schemes?.Data ?? new();
        return View();
    }

    // ==========================================
    // UTILITIES
    // ==========================================
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

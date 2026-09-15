using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scholarship.Web.Services.ApiClient;
using Scholarship.Web.ViewModels;

namespace Scholarship.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IScholarshipApiClient _apiClient;

    public DashboardController(IScholarshipApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        // 1. Fetch real student dashboard metrics from API
        var dashResponse = await _apiClient.GetStudentDashboardStatsAsync(2);
        var appResponse = await _apiClient.GetCurrentApplicationAsync(2);
        var profileResponse = await _apiClient.GetProfileAsync();
        var bankResponse = await _apiClient.GetBankAccountAsync();
        var certsResponse = await _apiClient.GetCertificatesAsync();

        ViewBag.StudentDashboard = dashResponse?.Data;
        ViewBag.Application = appResponse?.Data;
        ViewBag.Profile = profileResponse?.Data;
        ViewBag.Bank = bankResponse?.Data;
        ViewBag.Certificates = certsResponse?.Data ?? new();

        ViewBag.ProfileCompletion = dashResponse?.Data?.ProfileCompletion ?? 0;
        ViewBag.AppCompletion = appResponse?.Data?.IsLocked == true ? 100 : (appResponse?.Data?.CurrentStep ?? 1) * 20;

        return View();
    }
}

[Authorize]
public class ApplicationController : Controller
{
    private readonly IScholarshipApiClient _apiClient;

    public ApplicationController(IScholarshipApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Wizard(int step = 1)
    {
        // Current application
        var appResp = await _apiClient.GetCurrentApplicationAsync(2);
        if (appResp?.Data?.IsLocked == true && step < 6)
        {
            TempData["InfoMessage"] = "Your application is locked and cannot be modified. You can view the final preview.";
            return RedirectToAction("Preview");
        }

        ViewBag.CurrentStep = step;
        ViewBag.Application = appResp?.Data;

        // Load masters needed for the step
        if (step == 1) // Personal & Address
        {
            var profileResp = await _apiClient.GetProfileAsync();
            var addrResp = await _apiClient.GetAddressAsync("PERMANENT");
            var hhCats = await _apiClient.GetHouseholdCategoriesAsync();
            ViewBag.Profile = profileResp?.Data;
            ViewBag.Address = addrResp?.Data;
            ViewBag.HouseholdCategories = hhCats?.Data ?? new();
        }
        else if (step == 2) // Current Course
        {
            var acadResp = await _apiClient.GetAcademicDetailsAsync();
            ViewBag.Academic = acadResp?.Data;

            var institutes = await _apiClient.GetInstitutesAsync(0);
            var courses = await _apiClient.GetCoursesAsync(null);
            var courseTypes = await _apiClient.GetCourseTypesAsync();
            var schemes = await _apiClient.GetSchemesAsync();
            var admissionTypes = await _apiClient.GetAdmissionTypesAsync();
            var studyModes = await _apiClient.GetStudyModesAsync();
            ViewBag.Institutes = institutes?.Data ?? new();
            ViewBag.Courses = courses?.Data ?? new();
            ViewBag.CourseTypes = courseTypes?.Data ?? new();
            ViewBag.Schemes = schemes?.Data ?? new();
            ViewBag.AdmissionTypes = admissionTypes?.Data ?? new();
            ViewBag.StudyModes = studyModes?.Data ?? new();
        }
        else if (step == 3) // 10th & Previous Qualification
        {
            var acadResp = await _apiClient.GetAcademicDetailsAsync();
            var boards = await _apiClient.GetEducationBoardsAsync();
            ViewBag.Academic = acadResp?.Data;
            ViewBag.Boards = boards?.Data ?? new();
        }
        else if (step == 4) // Bank Details
        {
            var bankResp = await _apiClient.GetBankAccountAsync();
            ViewBag.Bank = bankResp?.Data;
            var banks = await _apiClient.GetBanksAsync();
            ViewBag.Banks = banks?.Data ?? new();
        }
        else if (step == 5) // Certificates
        {
            var certsResp = await _apiClient.GetCertificatesAsync();
            ViewBag.Certificates = certsResp?.Data ?? new();
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveStep(int step, [FromForm] IFormCollection form)
    {
        // Advance or update draft in API
        await _apiClient.SaveDraftApplicationAsync(2, 1, step + 1);

        if (step >= 5)
            return RedirectToAction("Preview");

        return RedirectToAction("Wizard", new { step = step + 1 });
    }

    [HttpGet]
    public async Task<IActionResult> Preview()
    {
        var appResp = await _apiClient.GetCurrentApplicationAsync(2);
        var profileResp = await _apiClient.GetProfileAsync();
        var addrResp = await _apiClient.GetAddressAsync("PERMANENT");
        var acadResp = await _apiClient.GetAcademicDetailsAsync();
        var bankResp = await _apiClient.GetBankAccountAsync();
        var certsResp = await _apiClient.GetCertificatesAsync();

        ViewBag.Application = appResp?.Data;
        ViewBag.Profile = profileResp?.Data;
        ViewBag.Address = addrResp?.Data;
        ViewBag.Academic = acadResp?.Data;
        ViewBag.Bank = bankResp?.Data;
        ViewBag.Certificates = certsResp?.Data ?? new();

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Lock()
    {
        var appResp = await _apiClient.GetCurrentApplicationAsync(2);
        if (appResp?.Data == null)
            return RedirectToAction("Wizard");

        if (appResp.Data.IsLocked)
        {
            TempData["InfoMessage"] = "Your application is already locked.";
            return RedirectToAction("Status");
        }

        ViewBag.Application = appResp.Data;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RequestLockOtp(ulong applicationId)
    {
        ulong studentId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var resp = await _apiClient.RequestOtpAsync(studentId, "APPLICATION_LOCK", applicationId);
        if (resp != null && resp.Success)
        {
            return Json(new { success = true, message = "OTP has been sent to your registered mobile number." });
        }
        return Json(new { success = false, message = resp?.Message ?? "Failed to send OTP. Please try again." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmLock(ulong applicationId, string otpCode)
    {
        if (string.IsNullOrWhiteSpace(otpCode))
        {
            TempData["ErrorMessage"] = "Please enter the 6-digit OTP.";
            return RedirectToAction("Lock");
        }

        var resp = await _apiClient.LockApplicationAsync(applicationId, otpCode.Trim());
        if (resp != null && resp.Success)
        {
            TempData["SuccessMessage"] = "Congratulations! Your application has been locked successfully and forwarded to your institute for verification.";
            return RedirectToAction("Status");
        }

        TempData["ErrorMessage"] = resp?.Message ?? "Invalid OTP code. Please try again.";
        return RedirectToAction("Lock");
    }

    [HttpGet]
    public async Task<IActionResult> Status()
    {
        var appResp = await _apiClient.GetCurrentApplicationAsync(2);
        ViewBag.Application = appResp?.Data;
        return View();
    }
}

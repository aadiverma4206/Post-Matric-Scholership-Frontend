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
        if (appResp?.Data?.IsLocked == true)
        {
            TempData["InfoMessage"] = "Aapka scholarship application submit aur lock ho chuka hai. Dubara form fill nahi kiya ja sakta. Niche aapka final printable dossier uplabdh hai.";
            return RedirectToAction("Preview");
        }

        ViewBag.CurrentStep = step;
        ViewBag.Application = appResp?.Data;
        ViewBag.IsApplicationLocked = false;

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
        var appResp = await _apiClient.GetCurrentApplicationAsync(2);
        if (appResp?.Data?.IsLocked == true)
        {
            TempData["ErrorMessage"] = "Aapka aavedan submit aur lock ho chuka hai. Form dubara submit nahi kiya ja sakta.";
            return RedirectToAction("Preview");
        }

        if (step == 1)
        {
            var addrLine = form["AddressLine"].ToString();
            var pincode = form["Pincode"].ToString();
            if (!string.IsNullOrWhiteSpace(addrLine) && !string.IsNullOrWhiteSpace(pincode))
            {
                var curAddr = (await _apiClient.GetAddressAsync("PERMANENT"))?.Data ?? new ViewModels.AddressViewModel();
                curAddr.AddressLine = addrLine;
                curAddr.Pincode = pincode;
                curAddr.AddressType = "PERMANENT";
                await _apiClient.SaveAddressAsync(curAddr);
            }

            var profResp = await _apiClient.GetProfileAsync();
            if (profResp?.Data != null)
            {
                var prof = profResp.Data;
                if (decimal.TryParse(form["AnnualIncome"], out var income)) prof.AnnualIncome = income;
                if (uint.TryParse(form["HouseholdCategoryId"], out var hhCatId)) prof.HouseholdCategoryId = hhCatId;
                await _apiClient.UpdateProfileAsync(prof);
            }
        }
        else if (step == 2 || step == 3)
        {
            var curAcad = (await _apiClient.GetAcademicDetailsAsync())?.Data ?? new ViewModels.AcademicDetailsViewModel();
            if (step == 2)
            {
                if (ulong.TryParse(form["InstituteId"], out var instId)) curAcad.InstituteId = instId;
                if (uint.TryParse(form["CourseTypeId"], out var ctId)) curAcad.CourseTypeId = ctId;
                if (ulong.TryParse(form["CourseId"], out var cId)) curAcad.CourseId = cId;
                if (uint.TryParse(form["SchemeId"], out var sId)) curAcad.SchemeId = sId;
                if (uint.TryParse(form["CourseYear"], out var cy)) curAcad.CourseYear = cy;
                if (uint.TryParse(form["AdmissionTypeId"], out var atId)) curAcad.AdmissionTypeId = atId;
                if (uint.TryParse(form["StudyModeId"], out var smId)) curAcad.StudyModeId = smId;
                if (DateTime.TryParse(form["AdmissionDate"], out var admDate)) curAcad.AdmissionDate = admDate;
                var enroll = form["EnrollmentNumber"].ToString();
                if (!string.IsNullOrWhiteSpace(enroll)) curAcad.EnrollmentNumber = enroll;
                var branch = form["BranchName"].ToString();
                if (!string.IsNullOrWhiteSpace(branch)) curAcad.BranchName = branch;
                if (bool.TryParse(form["IsHosteller"], out var host)) curAcad.IsHosteller = host;
            }
            else if (step == 3)
            {
                var roll = form["TenthRollNumber"].ToString();
                if (!string.IsNullOrWhiteSpace(roll)) curAcad.TenthRollNumber = roll;
                if (ushort.TryParse(form["TenthPassingYear"], out var ty)) curAcad.TenthPassingYear = ty;
                if (uint.TryParse(form["TenthBoardId"], out var tbId)) curAcad.TenthBoardId = tbId;
                if (decimal.TryParse(form["TenthPercentage"], out var tp)) curAcad.TenthPercentage = tp;

                var prevInst = form["PreviousInstituteName"].ToString();
                if (!string.IsNullOrWhiteSpace(prevInst)) curAcad.PreviousInstituteName = prevInst;
                var prevRoll = form["PreviousRollNumber"].ToString();
                if (!string.IsNullOrWhiteSpace(prevRoll)) curAcad.PreviousRollNumber = prevRoll;
                if (ushort.TryParse(form["PreviousPassingYear"], out var py)) curAcad.PreviousPassingYear = py;
                if (decimal.TryParse(form["PreviousPercentage"], out var pp)) curAcad.PreviousPercentage = pp;
            }
            curAcad.AcademicYearId = 2;
            await _apiClient.SaveAcademicDetailsAsync(curAcad);
        }
        else if (step == 4)
        {
            var acc = form["AccountNumber"].ToString();
            var conf = form["ConfirmAccountNumber"].ToString();
            ulong.TryParse(form["BankId"], out var bId);
            bool isSeeded = form["IsAadhaarSeeded"] == "true" || form["IsAadhaarSeeded"] == "on";
            if (!string.IsNullOrWhiteSpace(acc) && bId > 0)
            {
                var bModel = new ViewModels.BankAccountViewModel
                {
                    BankId = bId,
                    BranchId = 1,
                    AccountNumber = acc,
                    ConfirmAccountNumber = string.IsNullOrWhiteSpace(conf) ? acc : conf,
                    IsAadhaarSeeded = isSeeded
                };
                await _apiClient.SaveBankAccountAsync(bModel);
            }
        }
        else if (step == 5)
        {
            var casteRef = form["CasteRefNo"].ToString();
            bool casteOnline = form["IsOnlineCaste"] == "true";
            if (!string.IsNullOrWhiteSpace(casteRef))
            {
                await _apiClient.SaveCertificateAsync(new ViewModels.CertificateViewModel
                {
                    CertificateTypeId = 1,
                    IsOnlineGenerated = casteOnline,
                    GeneratedFrom = "eDistrict",
                    ReferenceNumber = casteRef
                });
            }

            var domRef = form["DomicileRefNo"].ToString();
            bool domOnline = form["IsOnlineDomicile"] == "true";
            if (!string.IsNullOrWhiteSpace(domRef))
            {
                await _apiClient.SaveCertificateAsync(new ViewModels.CertificateViewModel
                {
                    CertificateTypeId = 2,
                    IsOnlineGenerated = domOnline,
                    GeneratedFrom = "eDistrict",
                    ReferenceNumber = domRef
                });
            }
        }

        ulong schemeId = 1;
        if (ulong.TryParse(form["SchemeId"], out ulong sid) && sid > 0)
        {
            schemeId = sid;
        }

        // Advance or update draft in API
        await _apiClient.SaveDraftApplicationAsync(2, schemeId, step + 1);

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
        ViewBag.IsApplicationLocked = (appResp?.Data?.IsLocked == true);

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
            TempData["InfoMessage"] = "Aapka application pehle hi submit aur lock ho chuka hai.";
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

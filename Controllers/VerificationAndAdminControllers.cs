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
        // Get InstituteId from User claim or default
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

[Authorize(Roles = "SUPER_ADMIN,DEPARTMENT_ADMIN")]
public class AdminController : Controller
{
    private readonly IScholarshipApiClient _apiClient;

    public AdminController(IScholarshipApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var statsResp = await _apiClient.GetDashboardStatsAsync();
        ViewBag.Stats = statsResp?.Data;
        return View();
    }

    public async Task<IActionResult> MasterSchemes()
    {
        var schemes = await _apiClient.GetSchemesAsync();
        ViewBag.Schemes = schemes?.Data ?? new();
        return View();
    }
}

using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Scholarship.Web.Models;
using Scholarship.Web.Services.ApiClient;

namespace Scholarship.Web.Controllers;

public class HomeController : Controller
{
    private readonly IScholarshipApiClient _apiClient;

    public HomeController(IScholarshipApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("/Home/Error")]
    [Route("/Home/Error/{statusCode?}")]
    public async Task<IActionResult> Error(int? statusCode = null)
    {
        var code = statusCode ?? Response.StatusCode;
        if (code < 400) code = 500;

        var statusFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        var model = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = code,
            OriginalPath = statusFeature?.OriginalPath ?? exceptionFeature?.Path ?? HttpContext.Request.Path
        };

        switch (code)
        {
            case 404:
                model.Title = "404 - पृष्ठ नहीं मिला | Page Not Found";
                model.Description = "The requested web page or portal feature could not be found. It may have been moved or the URL might be mistyped.";
                model.SolutionHint = "Please verify the URL or return to the Student Dashboard.";
                break;

            case 403:
                model.Title = "403 - अनधिकृत पहुंच | Access Forbidden";
                model.Description = "You do not have permission to view this section or perform this action with your current role.";
                model.SolutionHint = "If you believe this is an error, please contact your Institute Nodal Officer or District Admin.";
                break;

            case 401:
                model.Title = "401 - प्रमाणीकरण आवश्यक | Authentication Required";
                model.Description = "Your login session has expired or you need to authenticate to access this portal section.";
                model.SolutionHint = "Please log in again with your User ID and Password.";
                break;

            case 502:
            case 503:
            case 504:
                model.Title = $"{code} - सेवा अनुपलब्ध | Service Unavailable";
                model.Description = "The scholarship backend service or database is currently unavailable or undergoing maintenance.";
                model.SolutionHint = "Ensure the Scholarship.Api service is running on http://localhost:5001.";
                break;

            default:
                model.Title = $"{code} - सर्वर त्रुटि | Server Processing Error";
                model.Description = "A technical exception occurred while processing this portal request.";
                model.SolutionHint = "Please try again in a few moments. If the issue persists, verify backend connectivity.";
                break;
        }

        // Check if backend API is reachable
        try
        {
            var (isHealthy, healthMsg) = await _apiClient.CheckApiHealthAsync();
            model.IsApiDown = !isHealthy;
            ViewBag.ApiHealthMessage = healthMsg;
        }
        catch
        {
            model.IsApiDown = true;
            ViewBag.ApiHealthMessage = "Backend API is unreachable.";
        }

        return View(model);
    }
}

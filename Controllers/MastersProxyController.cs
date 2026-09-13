using Microsoft.AspNetCore.Mvc;
using Scholarship.Web.Services.ApiClient;

namespace Scholarship.Web.Controllers;

[ApiController]
[Route("api/masters")]
public class MastersProxyController : ControllerBase
{
    private readonly IScholarshipApiClient _apiClient;

    public MastersProxyController(IScholarshipApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet("academic-years")]
    public async Task<IActionResult> GetAcademicYears()
    {
        var res = await _apiClient.GetAcademicYearsAsync();
        return Ok(res?.Data ?? new());
    }

    [HttpGet("schemes")]
    public async Task<IActionResult> GetSchemes([FromQuery] uint? academicYearId)
    {
        var res = await _apiClient.GetSchemesAsync(academicYearId);
        return Ok(res?.Data ?? new());
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var res = await _apiClient.GetCategoriesAsync();
        return Ok(res?.Data ?? new());
    }

    [HttpGet("genders")]
    public async Task<IActionResult> GetGenders()
    {
        var res = await _apiClient.GetGendersAsync();
        return Ok(res?.Data ?? new());
    }

    [HttpGet("religions")]
    public async Task<IActionResult> GetReligions()
    {
        var res = await _apiClient.GetReligionsAsync();
        return Ok(res?.Data ?? new());
    }

    [HttpGet("states")]
    public async Task<IActionResult> GetStates()
    {
        var res = await _apiClient.GetStatesAsync();
        return Ok(res?.Data ?? new());
    }

    [HttpGet("districts")]
    public async Task<IActionResult> GetDistricts([FromQuery] ulong stateId = 1)
    {
        var res = await _apiClient.GetDistrictsAsync(stateId);
        return Ok(res?.Data ?? new());
    }

    [HttpGet("blocks")]
    public async Task<IActionResult> GetBlocks([FromQuery] ulong districtId)
    {
        var res = await _apiClient.GetBlocksAsync(districtId);
        return Ok(res?.Data ?? new());
    }

    [HttpGet("vidhansabhas")]
    public async Task<IActionResult> GetVidhansabhas([FromQuery] ulong districtId)
    {
        var res = await _apiClient.GetVidhansabhasAsync(districtId);
        return Ok(res?.Data ?? new());
    }

    [HttpGet("cities")]
    [HttpGet("cities-villages")]
    public async Task<IActionResult> GetCities([FromQuery] ulong districtId = 1, [FromQuery] ulong? blockId = null)
    {
        var res = await _apiClient.GetCitiesVillagesAsync(districtId, blockId);
        return Ok(res?.Data ?? new());
    }

    [HttpGet("post-offices")]
    public async Task<IActionResult> GetPostOffices([FromQuery] ulong districtId = 1, [FromQuery] string? pincode = null)
    {
        var res = await _apiClient.GetPostOfficesAsync(districtId);
        return Ok(res?.Data ?? new());
    }

    [HttpGet("banks")]
    public async Task<IActionResult> GetBanks()
    {
        var res = await _apiClient.GetBanksAsync();
        return Ok(res?.Data ?? new());
    }

    [HttpGet("bank-branches")]
    public async Task<IActionResult> GetBankBranches([FromQuery] ulong bankId)
    {
        var res = await _apiClient.GetBankBranchesAsync(bankId);
        return Ok(res?.Data ?? new());
    }

    [HttpGet("course-types")]
    public async Task<IActionResult> GetCourseTypes()
    {
        var res = await _apiClient.GetCourseTypesAsync();
        return Ok(res?.Data ?? new());
    }

    [HttpGet("courses")]
    public async Task<IActionResult> GetCourses([FromQuery] uint? courseTypeId)
    {
        var res = await _apiClient.GetCoursesAsync(courseTypeId);
        return Ok(res?.Data ?? new());
    }

    [HttpGet("branches")]
    [HttpGet("course-branches")]
    public async Task<IActionResult> GetBranches([FromQuery] ulong courseId)
    {
        var res = await _apiClient.GetCourseBranchesAsync(courseId);
        return Ok(res?.Data ?? new());
    }

    [HttpGet("institutes")]
    public async Task<IActionResult> GetInstitutes([FromQuery] ulong districtId)
    {
        var res = await _apiClient.GetInstitutesAsync(districtId);
        return Ok(res?.Data ?? new());
    }

    [HttpGet("institute-courses")]
    public async Task<IActionResult> GetInstituteCourses([FromQuery] ulong instituteId)
    {
        var res = await _apiClient.GetInstituteCoursesAsync(instituteId);
        return Ok(res?.Data ?? new());
    }

    [HttpGet("admission-types")]
    public async Task<IActionResult> GetAdmissionTypes()
    {
        var res = await _apiClient.GetAdmissionTypesAsync();
        return Ok(res?.Data ?? new());
    }

    [HttpGet("study-modes")]
    public async Task<IActionResult> GetStudyModes()
    {
        var res = await _apiClient.GetStudyModesAsync();
        return Ok(res?.Data ?? new());
    }

    [HttpGet("education-boards")]
    public async Task<IActionResult> GetEducationBoards()
    {
        var res = await _apiClient.GetEducationBoardsAsync();
        return Ok(res?.Data ?? new());
    }

    [HttpGet("document-types")]
    public async Task<IActionResult> GetDocumentTypes()
    {
        var res = await _apiClient.GetDocumentTypesAsync();
        return Ok(res?.Data ?? new());
    }
}

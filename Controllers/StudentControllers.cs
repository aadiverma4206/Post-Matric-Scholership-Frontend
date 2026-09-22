using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scholarship.Web.Services.ApiClient;
using Scholarship.Web.ViewModels;

namespace Scholarship.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IScholarshipApiClient _apiClient;

    public ProfileController(IScholarshipApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // View Basic Profile (Matching PDF Page 5)
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var profileResp = await _apiClient.GetProfileAsync();
        var addrResp = await _apiClient.GetAddressAsync("PERMANENT");
        var corrAddrResp = await _apiClient.GetAddressAsync("CORRESPONDENCE");
        var appResp = await _apiClient.GetCurrentApplicationAsync(2);

        ViewBag.Profile = profileResp?.Data;
        ViewBag.PermanentAddress = addrResp?.Data;
        ViewBag.CorrespondenceAddress = corrAddrResp?.Data ?? addrResp?.Data;
        ViewBag.IsApplicationLocked = appResp?.Data?.IsLocked == true;

        return View();
    }

    // Add / Update Profile (Matching PDF Pages 7-10)
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var appResp = await _apiClient.GetCurrentApplicationAsync(2);
        if (appResp?.Data?.IsLocked == true)
        {
            TempData["InfoMessage"] = "Aapka application lock aur submit ho chuka hai. Profile me badlav karne ki anumati nahi hai.";
            return RedirectToAction("Index");
        }

        var profileResp = await _apiClient.GetProfileAsync();
        var addrResp = await _apiClient.GetAddressAsync("CORRESPONDENCE");
        var states = await _apiClient.GetStatesAsync();
        var districts = await _apiClient.GetDistrictsAsync(1);
        var occupations = await _apiClient.GetOccupationsAsync();
        var householdCats = await _apiClient.GetHouseholdCategoriesAsync();
        var depCriteria = await _apiClient.GetDeprivationCriteriaAsync();
        var religions = await _apiClient.GetReligionsAsync();

        ulong selectedDistrictId = addrResp?.Data?.DistrictId ?? 1;
        var blocks = await _apiClient.GetBlocksAsync(selectedDistrictId);
        var vidhansabhas = await _apiClient.GetVidhansabhasAsync(selectedDistrictId);

        ViewBag.Profile = profileResp?.Data;
        ViewBag.Address = addrResp?.Data;
        ViewBag.States = states?.Data ?? new();
        ViewBag.Districts = districts?.Data ?? new();
        ViewBag.Blocks = blocks?.Data ?? new();
        ViewBag.Vidhansabhas = vidhansabhas?.Data ?? new();
        ViewBag.Occupations = occupations?.Data ?? new();
        ViewBag.HouseholdCategories = householdCats?.Data ?? new();
        ViewBag.DeprivationCriteria = depCriteria?.Data ?? new();
        ViewBag.Religions = religions?.Data ?? new();
        ViewBag.IsApplicationLocked = false;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(StudentProfileViewModel profileModel, AddressViewModel addressModel, IFormFile? photoFile)
    {
        var appResp = await _apiClient.GetCurrentApplicationAsync(2);
        if (appResp?.Data?.IsLocked == true)
        {
            TempData["ErrorMessage"] = "Aapka application pehle se lock aur submit hai. Profile update nahi kiya ja sakta.";
            return RedirectToAction("Index");
        }

        if (photoFile != null && photoFile.Length > 0)
        {
            var uploadResp = await _apiClient.UploadDocumentAsync(1, 2, photoFile);
            if (uploadResp?.Data != null)
            {
                TempData["InfoMessage"] = "Profile photo uploaded successfully.";
            }
        }

        // 1. Update Profile (family, household, etc.)
        await _apiClient.UpdateProfileAsync(profileModel);

        // 2. Save Address
        addressModel.AddressType = "CORRESPONDENCE";
        var saveAddrResp = await _apiClient.SaveAddressAsync(addressModel);
        if (saveAddrResp != null && saveAddrResp.Success)
        {
            TempData["SuccessMessage"] = "Profile details updated successfully.";
            return RedirectToAction("Index");
        }

        TempData["ErrorMessage"] = saveAddrResp?.Message ?? "Failed to save profile updates.";
        return RedirectToAction("Edit");
    }
}

[Authorize]
public class AcademicController : Controller
{
    private readonly IScholarshipApiClient _apiClient;

    public AcademicController(IScholarshipApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // Academic Details & 10th Class & Previous Completed Course (Matching PDF Pages 11-18)
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var appResp = await _apiClient.GetCurrentApplicationAsync(2);
        bool isLocked = appResp?.Data?.IsLocked == true;
        ViewBag.IsApplicationLocked = isLocked;

        var acadResp = await _apiClient.GetAcademicDetailsAsync();
        var acad = acadResp?.Data;
        ulong districtId = acad?.DistrictId > 0 ? acad.DistrictId.Value : 1;

        var districts = await _apiClient.GetDistrictsAsync(1);
        var courseTypes = await _apiClient.GetCourseTypesAsync();
        var admissionTypes = await _apiClient.GetAdmissionTypesAsync();
        var studyModes = await _apiClient.GetStudyModesAsync();
        var boards = await _apiClient.GetEducationBoardsAsync();
        var institutes = await _apiClient.GetInstitutesAsync(districtId);
        var courses = await _apiClient.GetCoursesAsync(null);

        List<LookupItemViewModel> branches = new();
        if (acad?.CourseId > 0)
        {
            var branchResp = await _apiClient.GetCourseBranchesAsync(acad.CourseId.Value);
            branches = branchResp?.Data ?? new();
        }

        ViewBag.Academic = acad;
        ViewBag.Districts = districts?.Data ?? new();
        ViewBag.CourseTypes = courseTypes?.Data ?? new();
        ViewBag.AdmissionTypes = admissionTypes?.Data ?? new();
        ViewBag.StudyModes = studyModes?.Data ?? new();
        ViewBag.Boards = boards?.Data ?? new();
        ViewBag.Institutes = institutes?.Data ?? new();
        ViewBag.Courses = courses?.Data ?? new();
        ViewBag.Branches = branches;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AcademicDetailsViewModel model, IFormFile? tenthMarksheetFile, IFormFile? marksheetFile)
    {
        var appResp = await _apiClient.GetCurrentApplicationAsync(2);
        if (appResp?.Data?.IsLocked == true)
        {
            TempData["ErrorMessage"] = "Aapka application pehle se submit aur lock ho chuka hai. Academic details me badlav nahi kiya ja sakta.";
            return RedirectToAction("Index");
        }

        if (tenthMarksheetFile != null && tenthMarksheetFile.Length > 0)
        {
            var uploadResp = await _apiClient.UploadDocumentAsync(2, 2, tenthMarksheetFile);
            if (uploadResp?.Data != null)
            {
                model.TenthMarksheetDocId = uploadResp.Data.DocumentId;
            }
        }

        if (marksheetFile != null && marksheetFile.Length > 0)
        {
            var uploadResp = await _apiClient.UploadDocumentAsync(3, 2, marksheetFile);
            if (uploadResp?.Data != null)
            {
                model.PreviousMarksheetDocId = uploadResp.Data.DocumentId;
            }
        }

        model.AcademicYearId = 2; // AY 2025-26
        var response = await _apiClient.SaveAcademicDetailsAsync(model);
        if (response != null && response.Success)
        {
            TempData["SuccessMessage"] = "Academic details and qualifications saved successfully.";
            return RedirectToAction("Index");
        }

        TempData["ErrorMessage"] = response?.Message ?? "Failed to save academic details.";
        return RedirectToAction("Index");
    }
}

[Authorize]
public class BankController : Controller
{
    private readonly IScholarshipApiClient _apiClient;

    public BankController(IScholarshipApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // Student Bank Details (Matching PDF Pages 19-20)
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var appResp = await _apiClient.GetCurrentApplicationAsync(2);
        bool isLocked = appResp?.Data?.IsLocked == true;
        ViewBag.IsApplicationLocked = isLocked;

        var bankResp = await _apiClient.GetBankAccountAsync();
        var banks = await _apiClient.GetBanksAsync();

        ViewBag.BankAccount = bankResp?.Data;
        ViewBag.Banks = banks?.Data ?? new();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(BankAccountViewModel model, IFormFile? passbookFile)
    {
        var appResp = await _apiClient.GetCurrentApplicationAsync(2);
        if (appResp?.Data?.IsLocked == true)
        {
            TempData["ErrorMessage"] = "Aapka application pehle se submit aur lock ho chuka hai. Bank details me badlav nahi kiya ja sakta.";
            return RedirectToAction("Index");
        }

        if (passbookFile != null && passbookFile.Length > 0)
        {
            var uploadResp = await _apiClient.UploadDocumentAsync(4, 2, passbookFile);
            if (uploadResp?.Data != null)
            {
                model.PassbookDocumentId = uploadResp.Data.DocumentId;
            }
        }

        var saveResp = await _apiClient.SaveBankAccountAsync(model);
        if (saveResp != null && saveResp.Success)
        {
            TempData["SuccessMessage"] = "Bank account details saved and verified for DBT disbursement.";
            return RedirectToAction("Index");
        }

        TempData["ErrorMessage"] = saveResp?.Message ?? "Failed to save bank account.";
        return RedirectToAction("Index");
    }
}

[Authorize]
public class CertificateController : Controller
{
    private readonly IScholarshipApiClient _apiClient;

    public CertificateController(IScholarshipApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // Caste, Domicile, Income Certificates (Matching PDF Pages 21-22)
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var appResp = await _apiClient.GetCurrentApplicationAsync(2);
        bool isLocked = appResp?.Data?.IsLocked == true;
        ViewBag.IsApplicationLocked = isLocked;

        var certsResp = await _apiClient.GetCertificatesAsync();
        ViewBag.Certificates = certsResp?.Data ?? new();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(CertificateViewModel model, IFormFile? certificateFile)
    {
        var appResp = await _apiClient.GetCurrentApplicationAsync(2);
        if (appResp?.Data?.IsLocked == true)
        {
            TempData["ErrorMessage"] = "Aapka application pehle se submit aur lock ho chuka hai. Certificates me badlav nahi kiya ja sakta.";
            return RedirectToAction("Index");
        }

        if (certificateFile != null && certificateFile.Length > 0)
        {
            // Map CertificateTypeId (1: Caste -> docType 5, 2: Domicile -> docType 6, 3: Income -> docType 7, 4: Differently-Abled -> docType 8)
            uint docTypeId = model.CertificateTypeId switch
            {
                1 => 5,
                2 => 6,
                3 => 7,
                4 => 8,
                _ => 10
            };

            var uploadResp = await _apiClient.UploadDocumentAsync(docTypeId, 2, certificateFile);
            if (uploadResp?.Data != null)
            {
                model.DocumentId = uploadResp.Data.DocumentId;
            }
        }

        var saveResp = await _apiClient.SaveCertificateAsync(model);
        if (saveResp != null && saveResp.Success)
        {
            TempData["SuccessMessage"] = "Certificate reference and document successfully saved.";
            return RedirectToAction("Index");
        }

        TempData["ErrorMessage"] = saveResp?.Message ?? "Certificate saving failed.";
        return RedirectToAction("Index");
    }
}

[Authorize]
public class DocumentController : Controller
{
    private readonly IScholarshipApiClient _apiClient;

    public DocumentController(IScholarshipApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var appResp = await _apiClient.GetCurrentApplicationAsync(2);
        bool isLocked = appResp?.Data?.IsLocked == true;
        ViewBag.IsApplicationLocked = isLocked;

        var docsResp = await _apiClient.GetDocumentsAsync();
        var certsResp = await _apiClient.GetCertificatesAsync();
        var bankResp = await _apiClient.GetBankAccountAsync();
        var acadResp = await _apiClient.GetAcademicDetailsAsync();
        var profileResp = await _apiClient.GetProfileAsync();
        var docTypes = await _apiClient.GetDocumentTypesAsync();

        ViewBag.Documents = docsResp?.Data ?? new();
        ViewBag.Certificates = certsResp?.Data ?? new();
        ViewBag.BankAccount = bankResp?.Data;
        ViewBag.Academic = acadResp?.Data;
        ViewBag.Profile = profileResp?.Data;
        ViewBag.DocumentTypes = docTypes?.Data ?? new();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(uint documentTypeId, IFormFile file)
    {
        var appResp = await _apiClient.GetCurrentApplicationAsync(2);
        if (appResp?.Data?.IsLocked == true)
        {
            TempData["ErrorMessage"] = "Aapka application pehle se submit aur lock ho chuka hai. Naye documents upload nahi kiye ja sakte.";
            return RedirectToAction("Index");
        }

        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a file to upload.";
            return RedirectToAction("Index");
        }

        var res = await _apiClient.UploadDocumentAsync(documentTypeId, 2, file);
        if (res != null && res.Success)
        {
            TempData["SuccessMessage"] = "Document uploaded successfully and linked to your records.";
        }
        else
        {
            TempData["ErrorMessage"] = res?.Message ?? "Document upload failed.";
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Download(ulong id)
    {
        var result = await _apiClient.DownloadDocumentAsync(id);
        if (result == null)
            return NotFound("Requested document not found.");

        return File(result.Value.Stream, result.Value.ContentType, result.Value.FileName);
    }
}

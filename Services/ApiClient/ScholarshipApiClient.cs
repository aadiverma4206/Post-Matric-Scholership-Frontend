using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Scholarship.Web.ViewModels;

namespace Scholarship.Web.Services.ApiClient;

public interface IScholarshipApiClient
{
    Task<ApiResponseModel<AuthResultViewModel>?> RegisterStudentAsync(RegisterViewModel model);
    Task<ApiResponseModel<AuthResultViewModel>?> LoginStudentAsync(LoginViewModel model);
    Task<ApiResponseModel<AuthResultViewModel>?> LoginOfficialAsync(OfficialLoginViewModel model);

    Task<ApiResponseModel<StudentProfileViewModel>?> GetProfileAsync();
    Task<ApiResponseModel<bool>?> UpdateProfileAsync(StudentProfileViewModel model);
    Task<ApiResponseModel<AddressViewModel>?> GetAddressAsync(string type);
    Task<ApiResponseModel<ulong>?> SaveAddressAsync(AddressViewModel model);

    Task<ApiResponseModel<AcademicDetailsViewModel>?> GetAcademicDetailsAsync();
    Task<ApiResponseModel<bool>?> SaveAcademicDetailsAsync(AcademicDetailsViewModel model);

    Task<ApiResponseModel<BankAccountViewModel>?> GetBankAccountAsync();
    Task<ApiResponseModel<ulong>?> SaveBankAccountAsync(BankAccountViewModel model);

    Task<ApiResponseModel<List<CertificateViewModel>>?> GetCertificatesAsync();
    Task<ApiResponseModel<ulong>?> SaveCertificateAsync(CertificateViewModel model);

    Task<ApiResponseModel<ApplicationSummaryViewModel>?> GetCurrentApplicationAsync(uint academicYearId);
    Task<ApiResponseModel<ApplicationSummaryViewModel>?> SaveDraftApplicationAsync(uint academicYearId, ulong schemeId, int currentStep);
    Task<ApiResponseModel<bool>?> LockApplicationAsync(ulong applicationId, string otpCode);
    Task<ApiResponseModel<string>?> RequestOtpAsync(ulong studentId, string purpose, ulong? applicationId = null);

    Task<ApiResponseModel<PagedResultViewModel<ApplicationSummaryViewModel>>?> GetInstituteQueueAsync(ulong instituteId, int? statusId, int page, int pageSize);
    Task<ApiResponseModel<PagedResultViewModel<ApplicationSummaryViewModel>>?> GetDistrictQueueAsync(ulong districtId, int? statusId, int page, int pageSize);
    Task<ApiResponseModel<bool>?> SubmitVerificationDecisionAsync(ulong applicationId, string level, string status, string? remarks);

    Task<ApiResponseModel<DashboardStatsViewModel>?> GetDashboardStatsAsync();
    Task<ApiResponseModel<StudentDashboardViewModel>?> GetStudentDashboardStatsAsync(uint academicYearId = 2);

    Task<ApiResponseModel<string>?> ForgotPasswordAsync(string userName, string mobileOrEmail);
    Task<ApiResponseModel<bool>?> ResetPasswordAsync(string userName, string otpCode, string newPassword);
    Task<ApiResponseModel<ForgotUserIdResultViewModel>?> ForgotUserIdAsync(string emailOrMobile);
    Task<ApiResponseModel<DocumentUploadResultViewModel>?> UploadDocumentAsync(uint documentTypeId, uint? academicYearId, IFormFile file);
    Task<ApiResponseModel<List<DocumentItemViewModel>>?> GetDocumentsAsync();
    Task<(Stream Stream, string ContentType, string FileName)?> DownloadDocumentAsync(ulong documentId);

    // Master lookups
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetAcademicYearsAsync();
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetSchemesAsync(uint? academicYearId = null);
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetCategoriesAsync();
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetGendersAsync();
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetReligionsAsync();
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetStatesAsync();
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetDistrictsAsync(ulong stateId);
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetBlocksAsync(ulong districtId);
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetVidhansabhasAsync(ulong districtId);
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetCitiesVillagesAsync(ulong districtId, ulong? blockId);
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetPostOfficesAsync(ulong districtId);
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetBanksAsync();
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetBankBranchesAsync(ulong bankId);
    Task<ApiResponseModel<System.Text.Json.JsonElement>?> GetBranchByIfscAsync(string ifsc);
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetCourseTypesAsync();
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetCoursesAsync(uint? courseTypeId);
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetCourseBranchesAsync(ulong courseId);
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetInstitutesAsync(ulong districtId);
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetInstituteCoursesAsync(ulong instituteId);

    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetOccupationsAsync();
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetHouseholdCategoriesAsync();
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetDeprivationCriteriaAsync();
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetAdmissionTypesAsync();
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetStudyModesAsync();
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetEducationBoardsAsync();
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetDocumentTypesAsync();
}

public class ScholarshipApiClient : IScholarshipApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ScholarshipApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    private void AttachBearerToken()
    {
        var token = _httpContextAccessor.HttpContext?.User.FindFirst("AccessToken")?.Value
            ?? _httpContextAccessor.HttpContext?.Session.GetString("JWToken");

        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private async Task<ApiResponseModel<T>?> SendPostAsync<T>(string endpoint, object payload)
    {
        try
        {
            AttachBearerToken();
            var content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ApiResponseModel<T>>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            return new ApiResponseModel<T>
            {
                Success = false,
                Message = $"Backend Service Connection Error: Unable to reach API at {endpoint}. ({ex.Message})"
            };
        }
    }

    private async Task<ApiResponseModel<T>?> SendGetAsync<T>(string endpoint)
    {
        try
        {
            AttachBearerToken();
            var response = await _httpClient.GetAsync(endpoint);
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ApiResponseModel<T>>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            return new ApiResponseModel<T>
            {
                Success = false,
                Message = $"Backend Service Connection Error: Unable to reach API at {endpoint}. ({ex.Message})"
            };
        }
    }

    public Task<ApiResponseModel<AuthResultViewModel>?> RegisterStudentAsync(RegisterViewModel model) =>
        SendPostAsync<AuthResultViewModel>("/api/auth/student/register", model);

    public Task<ApiResponseModel<AuthResultViewModel>?> LoginStudentAsync(LoginViewModel model) =>
        SendPostAsync<AuthResultViewModel>("/api/auth/student/login", model);

    public Task<ApiResponseModel<AuthResultViewModel>?> LoginOfficialAsync(OfficialLoginViewModel model) =>
        SendPostAsync<AuthResultViewModel>("/api/auth/official/login", model);

    public Task<ApiResponseModel<StudentProfileViewModel>?> GetProfileAsync() =>
        SendGetAsync<StudentProfileViewModel>("/api/profile");

    public Task<ApiResponseModel<bool>?> UpdateProfileAsync(StudentProfileViewModel model) =>
        SendPostAsync<bool>("/api/profile", model);

    public Task<ApiResponseModel<AddressViewModel>?> GetAddressAsync(string type) =>
        SendGetAsync<AddressViewModel>($"/api/addresses/{type}");

    public Task<ApiResponseModel<ulong>?> SaveAddressAsync(AddressViewModel model) =>
        SendPostAsync<ulong>("/api/addresses", model);

    public Task<ApiResponseModel<AcademicDetailsViewModel>?> GetAcademicDetailsAsync() =>
        SendGetAsync<AcademicDetailsViewModel>("/api/academic");

    public Task<ApiResponseModel<bool>?> SaveAcademicDetailsAsync(AcademicDetailsViewModel model) =>
        SendPostAsync<bool>("/api/academic", model);

    public Task<ApiResponseModel<BankAccountViewModel>?> GetBankAccountAsync() =>
        SendGetAsync<BankAccountViewModel>("/api/banks");

    public Task<ApiResponseModel<ulong>?> SaveBankAccountAsync(BankAccountViewModel model) =>
        SendPostAsync<ulong>("/api/banks", model);

    public Task<ApiResponseModel<List<CertificateViewModel>>?> GetCertificatesAsync() =>
        SendGetAsync<List<CertificateViewModel>>("/api/certificates");

    public Task<ApiResponseModel<ulong>?> SaveCertificateAsync(CertificateViewModel model) =>
        SendPostAsync<ulong>("/api/certificates", model);

    public Task<ApiResponseModel<ApplicationSummaryViewModel>?> GetCurrentApplicationAsync(uint academicYearId) =>
        SendGetAsync<ApplicationSummaryViewModel>($"/api/applications/current?academicYearId={academicYearId}");

    public Task<ApiResponseModel<ApplicationSummaryViewModel>?> SaveDraftApplicationAsync(uint academicYearId, ulong schemeId, int currentStep) =>
        SendPostAsync<ApplicationSummaryViewModel>($"/api/applications/draft?academicYearId={academicYearId}&schemeId={schemeId}&currentStep={currentStep}", new { });

    public Task<ApiResponseModel<bool>?> LockApplicationAsync(ulong applicationId, string otpCode) =>
        SendPostAsync<bool>("/api/applications/lock", new { applicationId, otpCode });

    public Task<ApiResponseModel<string>?> RequestOtpAsync(ulong studentId, string purpose, ulong? applicationId = null) =>
        SendPostAsync<string>("/api/auth/otp/generate", new { studentId, purpose, applicationId });

    public Task<ApiResponseModel<PagedResultViewModel<ApplicationSummaryViewModel>>?> GetInstituteQueueAsync(ulong instituteId, int? statusId, int page, int pageSize) =>
        SendGetAsync<PagedResultViewModel<ApplicationSummaryViewModel>>($"/api/verification/institute/queue?instituteId={instituteId}&statusId={statusId}&page={page}&pageSize={pageSize}");

    public Task<ApiResponseModel<PagedResultViewModel<ApplicationSummaryViewModel>>?> GetDistrictQueueAsync(ulong districtId, int? statusId, int page, int pageSize) =>
        SendGetAsync<PagedResultViewModel<ApplicationSummaryViewModel>>($"/api/verification/district/queue?districtId={districtId}&statusId={statusId}&page={page}&pageSize={pageSize}");

    public Task<ApiResponseModel<bool>?> SubmitVerificationDecisionAsync(ulong applicationId, string level, string status, string? remarks) =>
        SendPostAsync<bool>("/api/verification/decision", new { applicationId, verificationLevel = level, status, remarks });

    public Task<ApiResponseModel<DashboardStatsViewModel>?> GetDashboardStatsAsync() =>
        SendGetAsync<DashboardStatsViewModel>("/api/dashboard/stats");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetAcademicYearsAsync() =>
        SendGetAsync<List<LookupItemViewModel>>("/api/masters/academic-years");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetSchemesAsync(uint? academicYearId = null) =>
        SendGetAsync<List<LookupItemViewModel>>($"/api/masters/schemes?academicYearId={academicYearId}");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetCategoriesAsync() =>
        SendGetAsync<List<LookupItemViewModel>>("/api/masters/categories");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetGendersAsync() =>
        SendGetAsync<List<LookupItemViewModel>>("/api/masters/genders");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetReligionsAsync() =>
        SendGetAsync<List<LookupItemViewModel>>("/api/masters/religions");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetStatesAsync() =>
        SendGetAsync<List<LookupItemViewModel>>("/api/masters/states");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetDistrictsAsync(ulong stateId) =>
        SendGetAsync<List<LookupItemViewModel>>($"/api/masters/districts?stateId={stateId}");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetBlocksAsync(ulong districtId) =>
        SendGetAsync<List<LookupItemViewModel>>($"/api/masters/blocks?districtId={districtId}");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetVidhansabhasAsync(ulong districtId) =>
        SendGetAsync<List<LookupItemViewModel>>($"/api/masters/vidhansabhas?districtId={districtId}");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetCitiesVillagesAsync(ulong districtId, ulong? blockId) =>
        SendGetAsync<List<LookupItemViewModel>>($"/api/masters/cities-villages?districtId={districtId}&blockId={blockId}");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetPostOfficesAsync(ulong districtId) =>
        SendGetAsync<List<LookupItemViewModel>>($"/api/masters/post-offices?districtId={districtId}");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetBanksAsync() =>
        SendGetAsync<List<LookupItemViewModel>>("/api/masters/banks");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetBankBranchesAsync(ulong bankId) =>
        SendGetAsync<List<LookupItemViewModel>>($"/api/masters/bank-branches?bankId={bankId}");

    public Task<ApiResponseModel<System.Text.Json.JsonElement>?> GetBranchByIfscAsync(string ifsc) =>
        SendGetAsync<System.Text.Json.JsonElement>($"/api/masters/branch-by-ifsc?ifsc={Uri.EscapeDataString(ifsc)}");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetCourseTypesAsync() =>
        SendGetAsync<List<LookupItemViewModel>>("/api/masters/course-types");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetCoursesAsync(uint? courseTypeId) =>
        SendGetAsync<List<LookupItemViewModel>>($"/api/masters/courses?courseTypeId={courseTypeId}");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetCourseBranchesAsync(ulong courseId) =>
        SendGetAsync<List<LookupItemViewModel>>($"/api/masters/course-branches?courseId={courseId}");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetInstitutesAsync(ulong districtId) =>
        SendGetAsync<List<LookupItemViewModel>>($"/api/masters/institutes?districtId={districtId}");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetInstituteCoursesAsync(ulong instituteId) =>
        SendGetAsync<List<LookupItemViewModel>>($"/api/masters/institute-courses?instituteId={instituteId}");

    public Task<ApiResponseModel<StudentDashboardViewModel>?> GetStudentDashboardStatsAsync(uint academicYearId = 2) =>
        SendGetAsync<StudentDashboardViewModel>($"/api/dashboard/student?academicYearId={academicYearId}");

    public Task<ApiResponseModel<string>?> ForgotPasswordAsync(string userName, string mobileOrEmail) =>
        SendPostAsync<string>("/api/auth/forgot-password", new { userName, mobileOrEmail });

    public Task<ApiResponseModel<bool>?> ResetPasswordAsync(string userName, string otpCode, string newPassword) =>
        SendPostAsync<bool>("/api/auth/reset-password", new { userName, otpCode, newPassword });

    public Task<ApiResponseModel<ForgotUserIdResultViewModel>?> ForgotUserIdAsync(string emailOrMobile) =>
        SendPostAsync<ForgotUserIdResultViewModel>("/api/auth/forgot-userid", new { emailOrMobile });

    public async Task<ApiResponseModel<DocumentUploadResultViewModel>?> UploadDocumentAsync(uint documentTypeId, uint? academicYearId, IFormFile file)
    {
        try
        {
            AttachBearerToken();
            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(documentTypeId.ToString()), "documentTypeId");
            if (academicYearId.HasValue)
            {
                formData.Add(new StringContent(academicYearId.Value.ToString()), "academicYearId");
            }

            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
            formData.Add(fileContent, "file", file.FileName);

            var response = await _httpClient.PostAsync("/api/documents/upload", formData);
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ApiResponseModel<DocumentUploadResultViewModel>>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            return new ApiResponseModel<DocumentUploadResultViewModel>
            {
                Success = false,
                Message = $"Error uploading document: {ex.Message}"
            };
        }
    }

    public Task<ApiResponseModel<List<DocumentItemViewModel>>?> GetDocumentsAsync() =>
        SendGetAsync<List<DocumentItemViewModel>>("/api/documents/student");

    public async Task<(Stream Stream, string ContentType, string FileName)?> DownloadDocumentAsync(ulong documentId)
    {
        try
        {
            AttachBearerToken();
            var response = await _httpClient.GetAsync($"/api/documents/{documentId}/download");
            if (!response.IsSuccessStatusCode)
                return null;

            var stream = await response.Content.ReadAsStreamAsync();
            string contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            string fileName = response.Content.Headers.ContentDisposition?.FileNameStar 
                ?? response.Content.Headers.ContentDisposition?.FileName 
                ?? $"document_{documentId}";
            fileName = fileName.Trim('\"');
            return (stream, contentType, fileName);
        }
        catch
        {
            return null;
        }
    }

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetOccupationsAsync() =>
        SendGetAsync<List<LookupItemViewModel>>("/api/masters/occupations");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetHouseholdCategoriesAsync() =>
        SendGetAsync<List<LookupItemViewModel>>("/api/masters/household-categories");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetDeprivationCriteriaAsync() =>
        SendGetAsync<List<LookupItemViewModel>>("/api/masters/deprivation-criteria");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetAdmissionTypesAsync() =>
        SendGetAsync<List<LookupItemViewModel>>("/api/masters/admission-types");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetStudyModesAsync() =>
        SendGetAsync<List<LookupItemViewModel>>("/api/masters/study-modes");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetEducationBoardsAsync() =>
        SendGetAsync<List<LookupItemViewModel>>("/api/masters/education-boards");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetDocumentTypesAsync() =>
        SendGetAsync<List<LookupItemViewModel>>("/api/masters/document-types");
}

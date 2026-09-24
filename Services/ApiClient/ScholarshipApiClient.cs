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
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetStatusesAsync();
    Task<ApiResponseModel<List<LookupItemViewModel>>?> GetDocumentTypesAsync();

    // Admin Module APIs
    Task<ApiResponseModel<PagedResultViewModel<AdminStudentListItemViewModel>>?> GetAdminStudentsAsync(string? search, ulong? districtId, int? statusId, uint? categoryId, int page, int pageSize);
    Task<ApiResponseModel<AdminStudentDetailsViewModel>?> GetAdminStudentDetailsAsync(ulong studentId);
    Task<ApiResponseModel<AdminDashboardStatsSummaryViewModel>?> GetAdminStatsAsync();
    Task<(bool IsHealthy, string Message)> CheckApiHealthAsync();
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

    public async Task<(bool IsHealthy, string Message)> CheckApiHealthAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var response = await _httpClient.GetAsync("/health", cts.Token);
            if (response.IsSuccessStatusCode)
            {
                return (true, "Scholarship.Api backend is online and responding.");
            }
            return (false, $"Scholarship.Api responded with status {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return (false, GetFriendlyErrorMessage(ex, "/health"));
        }
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

    private static string GetFriendlyErrorMessage(Exception ex, string endpoint)
    {
        if (ex is HttpRequestException || ex.InnerException is System.Net.Sockets.SocketException || ex.Message.Contains("actively refused", StringComparison.OrdinalIgnoreCase))
        {
            return "Backend Service Offline: Unable to reach the Scholarship API service at http://localhost:5001. Please ensure that the backend service (Scholarship.Api) is running.";
        }
        if (ex is TaskCanceledException or TimeoutException)
        {
            return "Backend Service Timeout: The request to the Scholarship API service timed out. Please try again.";
        }
        return $"Communication Error: Unable to communicate with Scholarship API at {endpoint}. ({ex.Message})";
    }

    private static ApiResponseModel<T> CreateErrorResponse<T>(Exception ex, string endpoint)
    {
        var msg = GetFriendlyErrorMessage(ex, endpoint);
        return new ApiResponseModel<T>
        {
            Success = false,
            Message = msg,
            Errors = new List<string> { msg }
        };
    }

    private async Task<ApiResponseModel<T>> ParseResponseAsync<T>(HttpResponseMessage response, string endpoint)
    {
        string rawJson = string.Empty;
        try
        {
            rawJson = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            return new ApiResponseModel<T>
            {
                Success = false,
                Message = $"Failed to read response from API: {ex.Message}",
                Errors = new List<string> { ex.Message }
            };
        }

        if (string.IsNullOrWhiteSpace(rawJson))
        {
            if (response.IsSuccessStatusCode)
            {
                return new ApiResponseModel<T> { Success = true };
            }

            string statusMsg = GetStatusCodeMessage(response.StatusCode, endpoint);
            return new ApiResponseModel<T>
            {
                Success = false,
                Message = statusMsg,
                Errors = new List<string> { statusMsg }
            };
        }

        // 1. Try standard ApiResponseModel<T>
        try
        {
            var result = JsonSerializer.Deserialize<ApiResponseModel<T>>(rawJson, _jsonOptions);
            if (result != null)
            {
                if (!response.IsSuccessStatusCode && result.Success)
                {
                    result.Success = false;
                }
                if (!result.Success && string.IsNullOrWhiteSpace(result.Message))
                {
                    result.Message = GetStatusCodeMessage(response.StatusCode, endpoint);
                }
                return result;
            }
        }
        catch
        {
            // Fallthrough to try parsing ProblemDetails or standard errors
        }

        // 2. Try parsing ProblemDetails or ValidationProblemDetails
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;
            var errorsList = new List<string>();
            string message = string.Empty;

            if (root.TryGetProperty("message", out var msgProp) && msgProp.ValueKind == JsonValueKind.String)
                message = msgProp.GetString() ?? string.Empty;
            else if (root.TryGetProperty("title", out var titleProp) && titleProp.ValueKind == JsonValueKind.String)
                message = titleProp.GetString() ?? string.Empty;

            if (root.TryGetProperty("errors", out var errorsProp))
            {
                if (errorsProp.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in errorsProp.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in prop.Value.EnumerateArray())
                            {
                                var errStr = item.GetString();
                                if (!string.IsNullOrWhiteSpace(errStr))
                                    errorsList.Add(errStr);
                            }
                        }
                    }
                }
                else if (errorsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in errorsProp.EnumerateArray())
                    {
                        var errStr = item.GetString();
                        if (!string.IsNullOrWhiteSpace(errStr))
                            errorsList.Add(errStr);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(message) && errorsList.Count > 0)
                message = string.Join("; ", errorsList);

            if (string.IsNullOrWhiteSpace(message))
                message = GetStatusCodeMessage(response.StatusCode, endpoint);

            return new ApiResponseModel<T>
            {
                Success = response.IsSuccessStatusCode,
                Message = message,
                Errors = errorsList.Count > 0 ? errorsList : new List<string> { message }
            };
        }
        catch
        {
            // Not JSON
            string friendlyMsg = GetStatusCodeMessage(response.StatusCode, endpoint);
            return new ApiResponseModel<T>
            {
                Success = response.IsSuccessStatusCode,
                Message = friendlyMsg,
                Errors = new List<string> { friendlyMsg }
            };
        }
    }

    private static string GetStatusCodeMessage(System.Net.HttpStatusCode code, string endpoint) => code switch
    {
        System.Net.HttpStatusCode.Unauthorized => "Your session has expired or authentication is required. Please sign in again.",
        System.Net.HttpStatusCode.Forbidden => "Access Denied: You do not have sufficient permissions to perform this action.",
        System.Net.HttpStatusCode.NotFound => $"The requested information or service ({endpoint}) was not found.",
        System.Net.HttpStatusCode.BadRequest => "The submitted data was invalid. Please review your input and try again.",
        System.Net.HttpStatusCode.UnprocessableEntity => "Validation error: Some fields do not meet the required format.",
        System.Net.HttpStatusCode.InternalServerError => "The backend server encountered an error processing your request. Please try again shortly.",
        System.Net.HttpStatusCode.BadGateway => "The API gateway was unable to reach the upstream service. Please ensure the backend is active.",
        System.Net.HttpStatusCode.ServiceUnavailable => "The scholarship service is temporarily unavailable due to maintenance. Please try again later.",
        _ => $"Service responded with HTTP {(int)code}."
    };

    private async Task<ApiResponseModel<T>?> SendPostAsync<T>(string endpoint, object payload)
    {
        try
        {
            AttachBearerToken();
            var content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            return await ParseResponseAsync<T>(response, endpoint);
        }
        catch (Exception ex)
        {
            return CreateErrorResponse<T>(ex, endpoint);
        }
    }

    private async Task<ApiResponseModel<T>?> SendGetAsync<T>(string endpoint)
    {
        try
        {
            AttachBearerToken();
            var response = await _httpClient.GetAsync(endpoint);
            return await ParseResponseAsync<T>(response, endpoint);
        }
        catch (Exception ex)
        {
            return CreateErrorResponse<T>(ex, endpoint);
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
            return await ParseResponseAsync<DocumentUploadResultViewModel>(response, "/api/documents/upload");
        }
        catch (Exception ex)
        {
            return CreateErrorResponse<DocumentUploadResultViewModel>(ex, "/api/documents/upload");
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

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetStatusesAsync() =>
        SendGetAsync<List<LookupItemViewModel>>("/api/masters/statuses");

    public Task<ApiResponseModel<List<LookupItemViewModel>>?> GetDocumentTypesAsync() =>
        SendGetAsync<List<LookupItemViewModel>>("/api/masters/document-types");

    public Task<ApiResponseModel<PagedResultViewModel<AdminStudentListItemViewModel>>?> GetAdminStudentsAsync(string? search, ulong? districtId, int? statusId, uint? categoryId, int page, int pageSize)
    {
        var sb = new StringBuilder($"/api/admin/students?page={page}&pageSize={pageSize}");
        if (!string.IsNullOrWhiteSpace(search)) sb.Append($"&search={Uri.EscapeDataString(search.Trim())}");
        if (districtId.HasValue && districtId.Value > 0) sb.Append($"&districtId={districtId.Value}");
        if (statusId.HasValue && statusId.Value > 0) sb.Append($"&statusId={statusId.Value}");
        if (categoryId.HasValue && categoryId.Value > 0) sb.Append($"&categoryId={categoryId.Value}");
        return SendGetAsync<PagedResultViewModel<AdminStudentListItemViewModel>>(sb.ToString());
    }

    public Task<ApiResponseModel<AdminStudentDetailsViewModel>?> GetAdminStudentDetailsAsync(ulong studentId) =>
        SendGetAsync<AdminStudentDetailsViewModel>($"/api/admin/students/{studentId}");

    public Task<ApiResponseModel<AdminDashboardStatsSummaryViewModel>?> GetAdminStatsAsync() =>
        SendGetAsync<AdminDashboardStatsSummaryViewModel>("/api/admin/stats");
}

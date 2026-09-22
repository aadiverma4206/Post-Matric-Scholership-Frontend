using System.ComponentModel.DataAnnotations;

namespace Scholarship.Web.ViewModels;

public class ApiResponseModel<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class PagedResultViewModel<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int Page => PageNumber > 0 ? PageNumber : 1;
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 10));
}

public class LookupItemViewModel
{
    public ulong Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class AuthResultViewModel
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? StudentCode { get; set; }
    public ulong? StudentId { get; set; }
    public ulong? UserId { get; set; }
    public ulong? InstituteId { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class LoginViewModel
{
    [Required(ErrorMessage = "User ID / Registration Number is required")]
    [Display(Name = "User ID")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    public string CaptchaToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "Captcha code is required")]
    [Display(Name = "Enter Captcha")]
    public string CaptchaInput { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
}

public class OfficialLoginViewModel
{
    [Required(ErrorMessage = "Official User ID is required")]
    [Display(Name = "Official User ID")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    public string CaptchaToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "Captcha code is required")]
    public string CaptchaInput { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
}

public class RegisterViewModel
{
    // Section 1: Basic Details
    [Required(ErrorMessage = "First Name is required")]
    [Display(Name = "First Name *")]
    public string FirstName { get; set; } = string.Empty;

    [Display(Name = "Middle Name")]
    public string? MiddleName { get; set; }

    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    [Required]
    [Display(Name = "Are you Orphan? *")]
    public bool IsOrphan { get; set; }

    [Required(ErrorMessage = "Father/Husband/Guardian Name is required")]
    [Display(Name = "Father/Husband/Guardian Name *")]
    public string FatherGuardianName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mother Name is required")]
    [Display(Name = "Mother Name *")]
    public string MotherName { get; set; } = string.Empty;

    [Display(Name = "Is your Mother Single Woman?")]
    public bool IsMotherSingleWoman { get; set; }

    // Section 2: Profile Related Details
    [Required(ErrorMessage = "Date of Birth is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth *")]
    public DateTime DateOfBirth { get; set; } = new DateTime(2005, 1, 1);

    [Required(ErrorMessage = "Category is required")]
    [Display(Name = "Category *")]
    public uint CategoryId { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    [Display(Name = "Gender *")]
    public uint GenderId { get; set; }

    [Required(ErrorMessage = "Religion is required")]
    [Display(Name = "Religion *")]
    public uint ReligionId { get; set; }

    // Section 3: Personal Details
    [Required(ErrorMessage = "Aadhaar Card Number is required")]
    [RegularExpression(@"^[0-9]{12}$", ErrorMessage = "Aadhaar number must be 12 digits.")]
    [Display(Name = "Aadhaar Card Number *")]
    public string AadhaarNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mobile Number is required")]
    [RegularExpression(@"^[6-9][0-9]{9}$", ErrorMessage = "Enter a valid 10-digit Indian mobile number.")]
    [Display(Name = "Mobile Number *")]
    public string MobileNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email Address is required")]
    [EmailAddress(ErrorMessage = "Enter a valid Email address.")]
    [Display(Name = "Email *")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Alternate Mobile")]
    public string? AlternateMobile { get; set; }

    [Display(Name = "Alternate Email")]
    public string? AlternateEmail { get; set; }

    [Range(typeof(bool), "true", "true", ErrorMessage = "You must give Aadhaar verification consent to proceed.")]
    [Display(Name = "I give voluntary consent to authenticate my Aadhaar via UIDAI/e-KYC for scholarship benefits.")]
    public bool AadhaarConsent { get; set; }

    // Section 4: Permanent Address
    [Required(ErrorMessage = "Home Address is required")]
    [Display(Name = "Home Address *")]
    public string AddressLine { get; set; } = string.Empty;

    [Required(ErrorMessage = "PIN Code is required")]
    [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "PIN Code must be 6 digits.")]
    [Display(Name = "PIN Code *")]
    public string Pincode { get; set; } = string.Empty;

    [Required(ErrorMessage = "State is required")]
    [Display(Name = "State *")]
    public ulong StateId { get; set; }

    [Required(ErrorMessage = "District is required")]
    [Display(Name = "District *")]
    public ulong DistrictId { get; set; }

    [Display(Name = "Block *")]
    public ulong? BlockId { get; set; }

    [Display(Name = "Vidhan Sabha *")]
    public ulong? VidhansabhaId { get; set; }

    [Display(Name = "City/Village *")]
    public ulong? CityVillageId { get; set; }

    [Display(Name = "Post Office *")]
    public ulong? PostOfficeId { get; set; }

    // Section 5: Create Login Password (User ID is auto-generated 11-digit number)
    [Display(Name = "User ID")]
    public string? UserName { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    [DataType(DataType.Password)]
    [Display(Name = "Password *")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm Password is required")]
    [Compare("Password", ErrorMessage = "Password and Confirm Password must match")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password *")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class StudentProfileViewModel
{
    public ulong StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string FullName => $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ").Trim();
    public DateTime DateOfBirth { get; set; }
    public uint GenderId { get; set; }
    public string? GenderName { get; set; }
    public uint CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public uint ReligionId { get; set; }
    public string? ReligionName { get; set; }
    public string MaskedAadhaar { get; set; } = string.Empty;
    public string MaskedMobile { get; set; } = string.Empty;
    public string MaskedEmail { get; set; } = string.Empty;
    public string? AlternateMobile { get; set; }
    public string? AlternateEmail { get; set; }
    public ulong? PhotoDocumentId { get; set; }

    public string FatherGuardianName { get; set; } = string.Empty;
    public string MotherName { get; set; } = string.Empty;
    public bool IsOrphan { get; set; }
    public bool IsMotherSingleWoman { get; set; }
    public uint? FatherOccupationId { get; set; }
    public uint? MotherOccupationId { get; set; }
    public bool IsDifferentlyAbled { get; set; }
    public bool ParentsIlliterate { get; set; }

    public uint? HouseholdCategoryId { get; set; }
    public decimal AnnualIncome { get; set; }
    public string? MaskedBplNumber { get; set; }
    public string? BplNumber { get; set; }
    public string? FirstNameHindi { get; set; }
    public string? MiddleNameHindi { get; set; }
    public string? LastNameHindi { get; set; }
    public string? FatherNameHindi { get; set; }
    public string? MotherNameHindi { get; set; }
    public string? OtrNumber { get; set; }
    public List<uint> ApplicableDeprivationCriteria { get; set; } = new();
}

public class AddressViewModel
{
    public ulong AddressId { get; set; }
    public ulong StudentId { get; set; }
    [Required] public string AddressType { get; set; } = "PERMANENT";
    [Required] public string AddressLine { get; set; } = string.Empty;
    [Required] public string Pincode { get; set; } = string.Empty;
    [Required] public ulong StateId { get; set; }
    public string? StateName { get; set; }
    [Required] public ulong DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public ulong? BlockId { get; set; }
    public string? BlockName { get; set; }
    public ulong? VidhansabhaId { get; set; }
    public string? VidhansabhaName { get; set; }
    public ulong? CityVillageId { get; set; }
    public string? CityVillageName { get; set; }
    public ulong? PostOfficeId { get; set; }
    public string? PostOfficeName { get; set; }
}

public class AcademicDetailsViewModel
{
    // Class 10th
    public ulong? Class10Id { get; set; }
    [Required] public string TenthRollNumber { get; set; } = string.Empty;
    [Required] public string SchoolType { get; set; } = "GOVERNMENT";
    [Required] public ushort TenthPassingYear { get; set; }
    [Required] public uint TenthBoardId { get; set; }
    public string? TenthBoardName { get; set; }
    [Required] public decimal TenthPercentage { get; set; }
    public ulong? TenthMarksheetDocId { get; set; }

    // Previous Education
    public ulong? PreviousEducationId { get; set; }
    [Required] public uint PreviousCourseTypeId { get; set; }
    public string? PreviousCourseTypeName { get; set; }
    [Required] public ulong PreviousCourseId { get; set; }
    public string? PreviousCourseName { get; set; }
    public ulong? PreviousBranchId { get; set; }
    public string? PreviousBranchName { get; set; }
    [Required] public string PreviousInstituteName { get; set; } = string.Empty;
    [Required] public string PreviousRollNumber { get; set; } = string.Empty;
    [Required] public ushort PreviousPassingYear { get; set; }
    [Required] public decimal PreviousPercentage { get; set; }
    public ulong? PreviousMarksheetDocId { get; set; }

    // Current Academic Record
    public ulong? AcademicRecordId { get; set; }
    public ulong? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public ulong? InstituteId { get; set; }
    public uint? CourseTypeId { get; set; }
    public ulong? CourseId { get; set; }
    public ulong? BranchId { get; set; }
    [Required] public uint AcademicYearId { get; set; }
    [Required] public ulong SchemeId { get; set; }
    [Required] public ulong InstituteCourseId { get; set; }
    public string? InstituteCode { get; set; }
    public string? InstituteName { get; set; }
    public string? CourseCode { get; set; }
    public string? CourseName { get; set; }
    public string? BranchName { get; set; }
    [Required] public DateTime AdmissionDate { get; set; } = DateTime.Today;
    [Required] public string EnrollmentNumber { get; set; } = string.Empty;
    public DateTime? EnrollmentDate { get; set; }
    [Required] public uint AdmissionTypeId { get; set; }
    [Required] public uint StudyModeId { get; set; }
    [Required] public uint CourseYear { get; set; } = 1;
    public bool IsHosteller { get; set; }
    public bool IsLateralEntry { get; set; }
    public bool IsLocked { get; set; }
}

public class BankAccountViewModel
{
    public ulong StudentBankAccountId { get; set; }
    [Required] public ulong BankId { get; set; }
    public string? BankName { get; set; }
    [Required] public ulong BranchId { get; set; }
    public string? BranchName { get; set; }
    public string? BranchAddress { get; set; }
    public string? IfscCode { get; set; }
    [Required] public string AccountNumber { get; set; } = string.Empty;
    [Required] public string ConfirmAccountNumber { get; set; } = string.Empty;
    public string MaskedAccountNumber { get; set; } = string.Empty;
    public bool IsAadhaarSeeded { get; set; }
    public string VerificationStatus { get; set; } = "PENDING";
    public ulong? PassbookDocumentId { get; set; }
}

public class CertificateViewModel
{
    public ulong CertificateId { get; set; }
    [Required] public uint CertificateTypeId { get; set; }
    public bool IsOnlineGenerated { get; set; } = true;
    public string GeneratedFrom { get; set; } = "EDISTRICT_PORTAL";
    [Required] public string ReferenceNumber { get; set; } = string.Empty;
    public string MaskedReferenceNumber { get; set; } = string.Empty;
    public decimal AnnualIncome { get; set; }
    public DateTime? IssueDate { get; set; }
    public ulong? DocumentId { get; set; }
    public string VerificationStatus { get; set; } = "PENDING";
}

public class ApplicationSummaryViewModel
{
    public ulong ApplicationId { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public ulong StudentId { get; set; }
    public string? StudentCode { get; set; }
    public string? StudentName { get; set; }
    public string? FullName => StudentName;
    public uint AcademicYearId { get; set; }
    public string? AcademicYearCode { get; set; }
    public ulong SchemeId { get; set; }
    public string? SchemeName { get; set; }
    public uint ApplicationStatusId { get; set; }
    public string? StatusCode { get; set; }
    public string? StatusName { get; set; }
    public string? ApplicationStatus => StatusName ?? StatusCode;
    public string? InstituteName { get; set; }
    public string? CourseName { get; set; }
    public string? CurrentCourse => CourseName;
    public string? Category { get; set; }
    public int CurrentStep { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DashboardStatsViewModel
{
    public int TotalApplications { get; set; }
    public int Draft { get; set; }
    public int Submitted { get; set; }
    public int Locked { get; set; }
    public int PendingInstitute { get; set; }
    public int InstituteApproved { get; set; }
    public int InstituteRejected { get; set; }
    public int PendingDistrict { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Reverted { get; set; }
    public int Disbursed { get; set; }
}

public class StudentDashboardViewModel
{
    public ulong ApplicationId { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public ulong StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = "2025-26";
    public string Scheme { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CurrentCourse { get; set; } = string.Empty;
    public string Institute { get; set; } = string.Empty;
    public uint ApplicationStatusId { get; set; }
    public string ApplicationStatus { get; set; } = "DRAFT";
    public int ProfileCompletion { get; set; }
    public int DocumentCompletion { get; set; }
    public string VerificationStatus { get; set; } = "PENDING";
    public string BankStatus { get; set; } = "PENDING";
    public string OtrStatus { get; set; } = "PENDING";
    public bool IsLocked { get; set; }
    public DateTime? LockedAt { get; set; }
}

public class ForgotUserIdViewModel
{
    [Required(ErrorMessage = "Please enter your registered Email Address or Mobile Number")]
    [Display(Name = "Registered Email Address or Mobile Number *")]
    public string EmailOrMobile { get; set; } = string.Empty;
}

public class ForgotUserIdResultViewModel
{
    public string UserName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? MaskedEmail { get; set; }
    public string? MaskedMobile { get; set; }
}

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "User ID / Registration ID is required")]
    [Display(Name = "User ID *")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Registered Mobile Number or Email is required")]
    [Display(Name = "Registered Mobile or Email *")]
    public string MobileOrEmail { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "6-digit OTP code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits.")]
    [Display(Name = "Verification OTP *")]
    public string OtpCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "Password must include uppercase, lowercase, digit, and special character.")]
    [DataType(DataType.Password)]
    [Display(Name = "New Password *")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm new password is required")]
    [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password *")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Current password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password *")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "Password must include uppercase, lowercase, digit, and special character.")]
    [DataType(DataType.Password)]
    [Display(Name = "New Password *")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm new password is required")]
    [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password *")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class ScrutinyDecisionViewModel
{
    [Required] public ulong ApplicationId { get; set; }
    [Required] public string VerificationLevel { get; set; } = "INSTITUTE";
    [Required] public string Status { get; set; } = "APPROVED"; // APPROVED, REJECTED, TEMPORARY_REJECTED
    [Required(ErrorMessage = "Remarks are required for decisions")]
    public string Remarks { get; set; } = string.Empty;
}

public class DocumentUploadResultViewModel
{
    public ulong DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
}

public class DocumentItemViewModel
{
    public ulong DocumentId { get; set; }
    public string DocumentUUID { get; set; } = string.Empty;
    public ulong? StudentId { get; set; }
    public uint DocumentTypeId { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public uint? AcademicYearId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public ulong FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    public bool IsVerified { get; set; }
}

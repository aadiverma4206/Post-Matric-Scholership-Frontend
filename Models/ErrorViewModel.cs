namespace Scholarship.Web.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public int StatusCode { get; set; } = 500;
    public string Title { get; set; } = "An unexpected error occurred";
    public string Description { get; set; } = "The portal experienced an issue while processing your request.";
    public string? SolutionHint { get; set; }
    public bool IsApiDown { get; set; }
    public string? OriginalPath { get; set; }
}

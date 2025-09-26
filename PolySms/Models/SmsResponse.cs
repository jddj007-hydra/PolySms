namespace PolySms.Models;

public class SmsResponse
{
    public bool IsSuccess { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public string BizId { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
}
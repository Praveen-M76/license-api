namespace LicenseApi.Requests
{
    public class ValidateTrialResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int DaysLeft { get; set; }
    }
}
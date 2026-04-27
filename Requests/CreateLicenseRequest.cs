namespace LicenseApi.Requests
{
    public class CreateLicenseRequest
    {
        public string LicenseKey { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
    }
}
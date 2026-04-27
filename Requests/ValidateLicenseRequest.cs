namespace LicenseApi.Requests
{
    public class ValidateLicenseRequest
    {
        public string LicenseKey { get; set; } = string.Empty;
        public string MachineId { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
    }
}
namespace LicenseApi.Requests
{
    public class ActivateLicenseRequest
    {
        public string LicenseKey { get; set; } = string.Empty;
        public string MachineId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
    }
}
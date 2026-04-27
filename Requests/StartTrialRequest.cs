namespace LicenseApi.Requests
{
    public class StartTrialRequest
    {
        public string Email { get; set; }
        public string ProjectName { get; set; }
        public string MachineId { get; set; }
        public string Code { get; set; }
        public string VerificationCode { get; set; }
        public string Otp { get; set; }
    }
}
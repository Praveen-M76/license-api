using System;

namespace LicenseApi.Requests
{
    public class StartTrialResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public DateTime EndDate { get; set; }
    }
}
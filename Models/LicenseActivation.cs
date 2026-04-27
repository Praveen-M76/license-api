using System;

namespace LicenseApi.Models
{
    public class LicenseActivation
    {
        public int Id { get; set; }
        public int LicenseId { get; set; }
        public string MachineId { get; set; } = string.Empty;
        public DateTime ActivatedAt { get; set; }
        public string Result { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
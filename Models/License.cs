using System;

namespace LicenseApi.Models
{
    public class License
    {
        public int Id { get; set; }
        public string LicenseKey { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public string? ActivatedMachineId { get; set; }
        public DateTime? ActivatedAt { get; set; }

        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
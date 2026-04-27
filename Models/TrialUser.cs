using System;

namespace LicenseApi.Models
{
    public class TrialUser
    {
        public int Id { get; set; }
        public string? Gmail { get; set; }
        public string? MachineId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsExpired { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? GoogleSub { get; set; }
        public string? ProjectName { get; set; }
    }
}
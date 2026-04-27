using System;

namespace LicenseApi.Models
{
    public class EmailVerificationCode
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? ProjectName { get; set; }
        public string? VerificationCode { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
using LicenseApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LicenseApi
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<License> Licenses { get; set; }
        public DbSet<LicenseActivation> LicenseActivations { get; set; }
        public DbSet<TrialUser> TrialUsers { get; set; }
        public DbSet<EmailVerificationCode> EmailVerificationCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<License>().ToTable("Licenses");
            modelBuilder.Entity<LicenseActivation>().ToTable("LicenseActivations");
            modelBuilder.Entity<TrialUser>().ToTable("TrialUsers");

            base.OnModelCreating(modelBuilder);
        }
    }
}
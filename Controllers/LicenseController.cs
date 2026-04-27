using System;
using System.Linq;
using LicenseApi.Models;
using LicenseApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LicenseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LicenseController : ControllerBase
    {
        private readonly AppDbContext _db;

        public LicenseController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("activate")]
        public IActionResult Activate([FromBody] ActivateLicenseRequest request)
        {
            if (request == null ||
             string.IsNullOrWhiteSpace(request.LicenseKey) ||
             string.IsNullOrWhiteSpace(request.MachineId) ||
             string.IsNullOrWhiteSpace(request.ProjectName))
            {
                return BadRequest(new ActivateLicenseResponse
                {
                    Success = false,
                    Message = "License key, machine ID and project name are required."
                });
            }

            string inputKey = request.LicenseKey.Trim().ToUpper();
            string machineId = request.MachineId.Trim().ToUpper();
            string projectName = request.ProjectName.Trim();

            var license = _db.Licenses.FirstOrDefault(x =>
                x.LicenseKey == inputKey &&
                x.ProductName == projectName);

            if (license == null)
            {
                return Ok(new ActivateLicenseResponse
                {
                    Success = false,
                    Message = "Invalid license key."
                });
            }

            if (!license.IsActive)
            {
                return Ok(new ActivateLicenseResponse
                {
                    Success = false,
                    Message = "This license is blocked."
                });
            }

            if (license.Status == "Unused")
            {
                license.Status = "Activated";
                license.ActivatedMachineId = machineId;
                license.ActivatedAt = DateTime.Now;
                license.CustomerName = request.CustomerName;
                license.CustomerEmail = request.CustomerEmail;

                _db.LicenseActivations.Add(new LicenseActivation
                {
                    LicenseId = license.Id,
                    MachineId = machineId,
                    ActivatedAt = DateTime.Now,
                    Result = "Success",
                    Notes = "First activation"
                });

                _db.SaveChanges();

                return Ok(new ActivateLicenseResponse
                {
                    Success = true,
                    Message = "License activated successfully."
                });
            }

            if (license.Status == "Activated" && license.ActivatedMachineId == machineId)
            {
                _db.LicenseActivations.Add(new LicenseActivation
                {
                    LicenseId = license.Id,
                    MachineId = machineId,
                    ActivatedAt = DateTime.Now,
                    Result = "Success",
                    Notes = "Already activated on same machine"
                });

                _db.SaveChanges();

                return Ok(new ActivateLicenseResponse
                {
                    Success = true,
                    Message = "License already activated on this PC."
                });
            }

            _db.LicenseActivations.Add(new LicenseActivation
            {
                LicenseId = license.Id,
                MachineId = machineId,
                ActivatedAt = DateTime.Now,
                Result = "Failed",
                Notes = "Key already used on another machine"
            });

            _db.SaveChanges();

            return Ok(new ActivateLicenseResponse
            {
                Success = false,
                Message = "This license key is already used on another PC."
            });
        }

        [HttpPost("validate")]
        public IActionResult Validate([FromBody] ValidateLicenseRequest request)
        {
            if (request == null ||
     string.IsNullOrWhiteSpace(request.LicenseKey) ||
     string.IsNullOrWhiteSpace(request.MachineId) ||
     string.IsNullOrWhiteSpace(request.ProjectName))
            {
                return BadRequest(new ActivateLicenseResponse
                {
                    Success = false,
                    Message = "License key, machine ID and project name are required."
                });
            }

            string inputKey = request.LicenseKey.Trim().ToUpper();
            string machineId = request.MachineId.Trim().ToUpper();
            string projectName = request.ProjectName.Trim();

            var license = _db.Licenses.FirstOrDefault(x =>
    x.LicenseKey == inputKey &&
    x.ProductName == projectName);

            if (license == null)
            {
                return Ok(new ValidateLicenseResponse
                {
                    Success = false,
                    Message = "License key not found."
                });
            }

            if (!license.IsActive)
            {
                return Ok(new ValidateLicenseResponse
                {
                    Success = false,
                    Message = "License is blocked."
                });
            }

            if (license.Status != "Activated")
            {
                return Ok(new ValidateLicenseResponse
                {
                    Success = false,
                    Message = "License is not activated."
                });
            }

            if (!string.Equals(license.ActivatedMachineId, machineId, StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new ValidateLicenseResponse
                {
                    Success = false,
                    Message = "License does not belong to this PC."
                });
            }

            return Ok(new ValidateLicenseResponse
            {
                Success = true,
                Message = "License is valid."
            });
        }

        [HttpPost("create")]
        public IActionResult CreateLicense([FromBody] CreateLicenseRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.LicenseKey) ||
                string.IsNullOrWhiteSpace(request.ProductName))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "License key and product name are required."
                });
            }

            string inputKey = request.LicenseKey.Trim().ToUpper();

            bool exists = _db.Licenses.Any(x => x.LicenseKey == inputKey);

            if (exists)
            {
                return Ok(new
                {
                    success = false,
                    message = "License key already exists."
                });
            }

            var license = new License
            {
                LicenseKey = inputKey,
                ProductName = request.ProductName.Trim(),
                Status = "Unused",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _db.Licenses.Add(license);
            _db.SaveChanges();

            return Ok(new
            {
                success = true,
                message = "License created successfully."
            });
        }

        [HttpGet("all")]
        public IActionResult GetAllLicenses()
        {
            var data = _db.Licenses.OrderBy(x => x.Id).ToList();
            return Ok(data);
        }

        [HttpGet("by-key/{key}")]
        public IActionResult GetLicenseByKey(string key)
        {
            string inputKey = key.Trim().ToUpper();

            var license = _db.Licenses.FirstOrDefault(x => x.LicenseKey == inputKey);

            if (license == null)
                return NotFound(new { message = "License not found." });

            return Ok(license);
        }

        [HttpGet("used")]
        public IActionResult GetUsedLicenses()
        {
            var data = _db.Licenses
                .Where(x => x.Status == "Activated")
                .OrderByDescending(x => x.ActivatedAt)
                .ToList();

            return Ok(data);
        }

        [HttpGet("unused")]
        public IActionResult GetUnusedLicenses()
        {
            var data = _db.Licenses
                .Where(x => x.Status == "Unused")
                .OrderBy(x => x.Id)
                .ToList();

            return Ok(data);
        }

        [HttpGet("blocked")]
        public IActionResult GetBlockedLicenses()
        {
            var data = _db.Licenses
                .Where(x => !x.IsActive)
                .OrderBy(x => x.Id)
                .ToList();

            return Ok(data);
        }

        [HttpPost("block/{key}")]
        public IActionResult BlockLicense(string key)
        {
            string inputKey = key.Trim().ToUpper();

            var license = _db.Licenses.FirstOrDefault(x => x.LicenseKey == inputKey);

            if (license == null)
                return NotFound(new { message = "License not found." });

            license.IsActive = false;
            _db.SaveChanges();

            return Ok(new
            {
                success = true,
                message = "License blocked successfully."
            });
        }

        [HttpPost("unblock/{key}")]
        public IActionResult UnblockLicense(string key)
        {
            string inputKey = key.Trim().ToUpper();

            var license = _db.Licenses.FirstOrDefault(x => x.LicenseKey == inputKey);

            if (license == null)
                return NotFound(new { message = "License not found." });

            license.IsActive = true;
            _db.SaveChanges();

            return Ok(new
            {
                success = true,
                message = "License unblocked successfully."
            });
        }

        [HttpGet("activations")]
        public IActionResult GetActivations()
        {
            var data = _db.LicenseActivations
                .OrderByDescending(x => x.ActivatedAt)
                .ToList();

            return Ok(data);
        }

        [HttpGet("activations/{key}")]
        public IActionResult GetActivationsByKey(string key)
        {
            string inputKey = key.Trim().ToUpper();

            var license = _db.Licenses.FirstOrDefault(x => x.LicenseKey == inputKey);

            if (license == null)
                return NotFound(new { message = "License not found." });

            var logs = _db.LicenseActivations
                .Where(x => x.LicenseId == license.Id)
                .OrderByDescending(x => x.ActivatedAt)
                .ToList();

            return Ok(logs);
        }

        [HttpPost("reset/{key}")]
        public IActionResult ResetLicense(string key)
        {
            string inputKey = key.Trim().ToUpper();

            var license = _db.Licenses.FirstOrDefault(x => x.LicenseKey == inputKey);

            if (license == null)
                return NotFound(new { message = "License not found." });

            license.Status = "Unused";
            license.ActivatedMachineId = null;
            license.ActivatedAt = null;
            license.CustomerName = null;
            license.CustomerEmail = null;
            license.IsActive = true;

            _db.SaveChanges();

            return Ok(new
            {
                success = true,
                message = "License reset successfully."
            });
        }
    }
}
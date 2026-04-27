using LicenseApi.Models;
using LicenseApi.Requests;
using LicenseApi.Responses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace LicenseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrialController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TrialController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("start")]
        public IActionResult StartTrial([FromBody] StartTrialRequest request)
        {
            try
            {
                if (request == null ||
                    string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.ProjectName) ||
                    string.IsNullOrWhiteSpace(request.MachineId))
                {
                    return BadRequest(new StartTrialResponse
                    {
                        Success = false,
                        Message = "Email, project name and machine id are required."
                    });
                }

                string email = request.Email.Trim();
                string projectName = request.ProjectName.Trim();
                string machineId = request.MachineId.Trim();

                string enteredCode = "";

                if (!string.IsNullOrWhiteSpace(request.VerificationCode))
                    enteredCode = request.VerificationCode.Trim();
                else if (!string.IsNullOrWhiteSpace(request.Code))
                    enteredCode = request.Code.Trim();
                else if (!string.IsNullOrWhiteSpace(request.Otp))
                    enteredCode = request.Otp.Trim();

                if (string.IsNullOrWhiteSpace(enteredCode))
                {
                    return BadRequest(new StartTrialResponse
                    {
                        Success = false,
                        Message = "Verification code is required."
                    });
                }

                var existing = _context.TrialUsers
                    .FirstOrDefault(x => x.Gmail == email &&
                                         x.ProjectName == projectName &&
                                         x.IsExpired == false);

                if (existing != null)
                {
                    return BadRequest(new StartTrialResponse
                    {
                        Success = false,
                        Message = "Trial already used for this email and project."
                    });
                }

                var verification = _context.EmailVerificationCodes
                    .Where(x => x.Email == email &&
                                x.ProjectName == projectName &&
                                x.IsUsed == false)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefault();

                if (verification == null)
                {
                    return BadRequest(new StartTrialResponse
                    {
                        Success = false,
                        Message = "No verification code found. Please click Send Code again."
                    });
                }

                if (DateTime.Now > verification.ExpiresAt)
                {
                    return BadRequest(new StartTrialResponse
                    {
                        Success = false,
                        Message = "Verification code expired. Please click Send Code again."
                    });
                }

                if (!string.Equals(verification.VerificationCode, enteredCode, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new StartTrialResponse
                    {
                        Success = false,
                        Message = "Invalid verification code."
                    });
                }

                verification.IsUsed = true;

                DateTime startDate = DateTime.Now;
                DateTime endDate = startDate.AddDays(7);

                var trial = new TrialUser
                {
                    Gmail = email,
                    MachineId = machineId,
                    StartedAt = startDate,
                    ExpiryDate = endDate,
                    IsExpired = false,
                    CreatedAt = DateTime.Now,
                    GoogleSub = Guid.NewGuid().ToString(),
                    ProjectName = projectName
                };

                _context.TrialUsers.Add(trial);
                _context.SaveChanges();

                return Ok(new StartTrialResponse
                {
                    Success = true,
                    Message = "Trial started successfully.",
                    EndDate = endDate
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new StartTrialResponse
                {
                    Success = false,
                    Message = ex.ToString()
                });
            }
        }

        [HttpPost("validate")]
        public IActionResult ValidateTrial([FromBody] ValidateTrialRequest request)
        {
            try
            {
                if (request == null ||
                    string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.ProjectName))
                {
                    return BadRequest(new ValidateTrialResponse
                    {
                        Success = false,
                        Message = "Email and project name are required.",
                        DaysLeft = 0
                    });
                }

                string email = request.Email.Trim();
                string projectName = request.ProjectName.Trim();

                var trial = _context.TrialUsers
                    .FirstOrDefault(x => x.Gmail == email && x.ProjectName == projectName);

                if (trial == null)
                {
                    return Ok(new ValidateTrialResponse
                    {
                        Success = false,
                        Message = "No trial found for this email and project.",
                        DaysLeft = 0
                    });
                }

                if (trial.IsExpired)
                {
                    return Ok(new ValidateTrialResponse
                    {
                        Success = false,
                        Message = "Trial is expired.",
                        DaysLeft = 0
                    });
                }

                if (DateTime.Now > trial.ExpiryDate)
                {
                    trial.IsExpired = true;
                    _context.SaveChanges();

                    return Ok(new ValidateTrialResponse
                    {
                        Success = false,
                        Message = "Trial expired.",
                        DaysLeft = 0
                    });
                }

                int daysLeft = (int)Math.Ceiling((trial.ExpiryDate - DateTime.Now).TotalDays);
                if (daysLeft < 0) daysLeft = 0;

                return Ok(new ValidateTrialResponse
                {
                    Success = true,
                    Message = "Trial valid.",
                    DaysLeft = daysLeft
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ValidateTrialResponse
                {
                    Success = false,
                    Message = ex.ToString(),
                    DaysLeft = 0
                });
            }
        }

        [HttpPost("send-code")]
        public IActionResult SendCode([FromBody] SendTrialCodeRequest request)
        {
            try
            {
                if (request == null ||
                    string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.ProjectName))
                {
                    return BadRequest(new TrialCodeResponse
                    {
                        Success = false,
                        Message = "Email and project name are required."
                    });
                }

                string email = request.Email.Trim();
                string projectName = request.ProjectName.Trim();

                var existingTrial = _context.TrialUsers
                    .FirstOrDefault(x => x.Gmail == email &&
                                         x.ProjectName == projectName &&
                                         x.IsExpired == false);

                if (existingTrial != null)
                {
                    return BadRequest(new TrialCodeResponse
                    {
                        Success = false,
                        Message = "This email has already used the free trial for this project."
                    });
                }

                var oldCodes = _context.EmailVerificationCodes
                    .Where(x => x.Email == email &&
                                x.ProjectName == projectName &&
                                x.IsUsed == false)
                    .ToList();

                foreach (var oldCode in oldCodes)
                {
                    oldCode.IsUsed = true;
                }

                var random = new Random();
                string code = random.Next(100000, 999999).ToString();

                var verification = new EmailVerificationCode
                {
                    Email = email,
                    ProjectName = projectName,
                    VerificationCode = code,
                    ExpiresAt = DateTime.Now.AddMinutes(10),
                    IsUsed = false,
                    CreatedAt = DateTime.Now
                };

                _context.EmailVerificationCodes.Add(verification);
                _context.SaveChanges();

                var emailService = new EmailService();
                string errorMessage;
                bool sent = emailService.SendVerificationCode(email, code, out errorMessage);

                if (!sent)
                {
                    return BadRequest(new TrialCodeResponse
                    {
                        Success = false,
                        Message = "Email sending failed: " + errorMessage
                    });
                }

                return Ok(new TrialCodeResponse
                {
                    Success = true,
                    Message = "Verification code sent successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new TrialCodeResponse
                {
                    Success = false,
                    Message = ex.ToString()
                });
            }
        }
    }
}
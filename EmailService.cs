using System;
using System.Net.Http;
using System.Text;

namespace LicenseApi
{
    public class EmailService
    {
        public bool SendVerificationCode(string toEmail, string code, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var apiKey = Environment.GetEnvironmentVariable("BREVO_API_KEY");

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    errorMessage = "BREVO_API_KEY missing.";
                    return false;
                }

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", apiKey);

                var json = $@"
{{
  ""sender"": {{
    ""name"": ""IGCompareTool"",
    ""email"": ""praveenmathu20@gmail.com""
  }},
  ""to"": [
    {{
      ""email"": ""{toEmail}""
    }}
  ],
  ""subject"": ""Verification Code"",
  ""htmlContent"": ""<h3>Your code: {code}</h3>""
}}";

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = client.PostAsync("https://api.brevo.com/v3/smtp/email", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    errorMessage = response.Content.ReadAsStringAsync().Result;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}

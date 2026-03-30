using SendGrid;
using SendGrid.Helpers.Mail;

namespace FlightBookingAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

       
        /// Sends an OTP email using SendGrid.
       
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="otp">OTP code</param>
        /// <returns>True if email sent successfully, false otherwise</returns>
        public async Task<bool> SendOtpEmailAsync(string toEmail, string otp)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Recipient email cannot be empty", nameof(toEmail));

            if (string.IsNullOrWhiteSpace(otp))
                throw new ArgumentException("OTP cannot be empty", nameof(otp));

            // Get the API key from configuration
            var apiKey = _config["SendGrid:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("SendGrid API key not found in configuration.");

            // Initialize SendGrid client
            var client = new SendGridClient(apiKey);

            // Create email details
            var from = new EmailAddress("srijaselvakumar05@gmail.com", "Srija"); // Must be verified in SendGrid
            var subject = "Your OTP Code";
            var to = new EmailAddress(toEmail);
            var plainTextContent = $"Your OTP is: {otp}";
            var htmlContent = $"<strong>Your OTP is: {otp}</strong>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            // Send the email
            var response = await client.SendEmailAsync(msg);

            // Return true if email accepted by SendGrid
            return response.StatusCode == System.Net.HttpStatusCode.Accepted;
        }
    }
}

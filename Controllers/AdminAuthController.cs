using BCrypt.Net;
using FlightBookingAPI.Models;
using FlightBookingAPI.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FlightBookingAPI.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminAuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;

        public AdminAuthController(IConfiguration config, EmailService emailService)
        {
            _config = config;
            _emailService = emailService;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] loginRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request");

            using var con = GetConnection();
            con.Open();

            // Select only PasswordHash
            var cmd = new SqlCommand("SELECT PasswordHash FROM Users WHERE Username=@u", con);
            cmd.Parameters.AddWithValue("@u", request.Username);

            var hash = cmd.ExecuteScalar()?.ToString(); // will correctly get the hash

            if (hash == null)
                return Unauthorized("Invalid username or password");

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, hash))
                return Unauthorized("Invalid username or password");

            return Ok(new { message = "Login successful" });
        }


        // FORGOT PASSWORD – SEND OTP
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest("Email is required");

            using var con = GetConnection();
            con.Open();

            //  Find user by email
            var getUserCmd = new SqlCommand(
                "SELECT Id FROM Users WHERE Email=@e", con);
            getUserCmd.Parameters.AddWithValue("@e", request.Email);

            var userIdObj = getUserCmd.ExecuteScalar();

            if (userIdObj == null)
                return NotFound("Email not registered");

            int userId = Convert.ToInt32(userIdObj);

            //  Delete old OTPs for this user (optional)
            var deleteOld = new SqlCommand(
                "DELETE FROM EmailOTP WHERE UserId=@uid", con);
            deleteOld.Parameters.AddWithValue("@uid", userId);
            deleteOld.ExecuteNonQuery();

            //  Generate new OTP
            string otp = new Random().Next(100000, 999999).ToString(); // 6-digit OTP

            //  Insert new OTP with UserId
            var insertCmd = new SqlCommand(
                @"INSERT INTO EmailOTP (Email, OTP, ExpiryTime, IsUsed, UserId)
          VALUES (@e, @o, DATEADD(MINUTE,5,GETDATE()), 0, @uid)",
                con);

            insertCmd.Parameters.AddWithValue("@e", request.Email);
            insertCmd.Parameters.AddWithValue("@o", otp);
            insertCmd.Parameters.AddWithValue("@uid", userId);
            insertCmd.ExecuteNonQuery();

            //  Send OTP via email
            bool sent = await _emailService.SendOtpEmailAsync(request.Email, otp);
            if (!sent)
                return StatusCode(500, "Failed to send OTP");

            return Ok(new { message = "OTP sent successfully" });
        }
        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            using var con = GetConnection();
            con.Open();

            var cmd = new SqlCommand(
                @"SELECT UserId
          FROM EmailOTP
          WHERE OTP = @o
          AND ExpiryTime > GETDATE()
          AND IsUsed = 0",
                con);

            cmd.Parameters.AddWithValue("@o", request.Otp);

            var userId = cmd.ExecuteScalar();

            if (userId == null)
                return BadRequest("Invalid or expired OTP");

            // ✅ Mark ONLY THIS OTP as verified
            var updateCmd = new SqlCommand(
                @"UPDATE EmailOTP
          SET IsVerified = 1
          WHERE OTP = @o",
                con);

            updateCmd.Parameters.AddWithValue("@o", request.Otp);
            updateCmd.ExecuteNonQuery();

            return Ok(new { message = "OTP verified successfully" });
        }


        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
        {
            using var con = GetConnection();
            con.Open();

            // 1️⃣ Find user using the verified OTP
            var getUserCmd = new SqlCommand(
                @"SELECT UserId 
          FROM EmailOTP 
          WHERE OTP = @o 
          AND IsUsed = 0 
          AND ExpiryTime > GETDATE()",
                con);

            getUserCmd.Parameters.AddWithValue("@o", request.OTP);

            var userId = getUserCmd.ExecuteScalar();

            if (userId == null)
                return BadRequest("OTP not verified or expired");

            // 2️⃣ Hash the new password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // 3️⃣ Update the password for that user
            var updateUserCmd = new SqlCommand(
                @"UPDATE Users 
          SET PasswordHash = @p 
          WHERE Id = @id",
                con);

            updateUserCmd.Parameters.AddWithValue("@p", hashedPassword);
            updateUserCmd.Parameters.AddWithValue("@id", (int)userId);
            updateUserCmd.ExecuteNonQuery();

            // 4️⃣ Mark OTP as used
            var markUsedCmd = new SqlCommand(
                @"UPDATE EmailOTP 
          SET IsUsed = 1 
          WHERE OTP = @o",
                con);
            markUsedCmd.Parameters.AddWithValue("@o", request.OTP);
            markUsedCmd.ExecuteNonQuery();

            return Ok(new { message = "Password reset successful" });
        }


    }
}

using FlightBookingAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FlightBookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AgentsController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] Agent model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string connectionString = _config.GetConnectionString("DefaultConnection");

            using SqlConnection conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            string panPath = "";
            string aadhaarPath = "";
            string gstPath = "";

            // Save PAN
            if (model.PanFile != null)
            {
                panPath = Path.Combine(uploadPath, Guid.NewGuid() + "_" + model.PanFile.FileName);
                using var stream = new FileStream(panPath, FileMode.Create);
                await model.PanFile.CopyToAsync(stream);
            }

            // Save Aadhaar
            if (model.AadhaarFile != null)
            {
                aadhaarPath = Path.Combine(uploadPath, Guid.NewGuid() + "_" + model.AadhaarFile.FileName);
                using var stream = new FileStream(aadhaarPath, FileMode.Create);
                await model.AadhaarFile.CopyToAsync(stream);
            }

            // Save GST
            if (model.GstFile != null)
            {
                gstPath = Path.Combine(uploadPath, Guid.NewGuid() + "_" + model.GstFile.FileName);
                using var stream = new FileStream(gstPath, FileMode.Create);
                await model.GstFile.CopyToAsync(stream);
            }

            string query = @"INSERT INTO Agents
                (AgencyName, FirstName, LastName, Address, Country, State, City,
                 Email, Mobile, Pan, Gst,
                 PanFilePath, AadhaarFilePath, GstFilePath)
                VALUES
                (@AgencyName, @FirstName, @LastName, @Address, @Country, @State, @City,
                 @Email, @Mobile, @Pan, @Gst,
                 @PanFilePath, @AadhaarFilePath, @GstFilePath)";

            using SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@AgencyName", model.AgencyName);
            cmd.Parameters.AddWithValue("@FirstName", model.FirstName);
            cmd.Parameters.AddWithValue("@LastName", model.LastName);
            cmd.Parameters.AddWithValue("@Address", model.Address ?? "");
            cmd.Parameters.AddWithValue("@Country", model.Country ?? "");
            cmd.Parameters.AddWithValue("@State", model.State ?? "");
            cmd.Parameters.AddWithValue("@City", model.City ?? "");
            cmd.Parameters.AddWithValue("@Email", model.Email);
            cmd.Parameters.AddWithValue("@Mobile", model.Mobile);
            cmd.Parameters.AddWithValue("@Pan", model.Pan ?? "");
            cmd.Parameters.AddWithValue("@Gst", model.Gst ?? "");
            cmd.Parameters.AddWithValue("@PanFilePath", panPath);
            cmd.Parameters.AddWithValue("@AadhaarFilePath", aadhaarPath);
            cmd.Parameters.AddWithValue("@GstFilePath", gstPath);

            await cmd.ExecuteNonQueryAsync();

            return Ok("Agent Registered Successfully");
        }
    }
}

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightBookingAPI.Models
{
    public class Agent
    {
        public int Id { get; set; }

        [Required]
        public string AgencyName { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string? Address { get; set; }
        public string? Country { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string? Pan { get; set; }
        public string? Gst { get; set; }


        [Required]
        public string Email { get; set; }

        [Required]
        public string Mobile { get; set; }

       
        // FILES
       
        [NotMapped]
        public IFormFile? PanFile { get; set; }
        [NotMapped]
        public IFormFile? AadhaarFile { get; set; }
        [NotMapped]
        public IFormFile? GstFile { get; set; }
    }
}

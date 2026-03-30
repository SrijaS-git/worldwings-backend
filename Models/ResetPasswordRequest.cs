namespace FlightBookingAPI.Models
{
    public class ResetPasswordRequest
    {


        public string OTP { get; set; }

        public string NewPassword { get; set; }
    }
}

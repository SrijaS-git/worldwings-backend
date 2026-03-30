using FlightBookingAPI.Data;
using FlightBookingAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;
namespace FlightBookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

     

        public BookingsController(IConfiguration configuration)
        {
            _configuration = configuration;
            
        }
        // ✅ PASTE SAVEBOOKING HERE
        [HttpPost]
        public IActionResult SaveBooking([FromBody] Booking booking)
        {
            if (booking == null)
            {
                return BadRequest("Booking object is null");
            }

            string query = @"
  INSERT INTO Bookings
(BookingId, PNR, TicketNumber,
PassengerTitle, PassengerFirstName, PassengerLastName,
PassengerDOB, PassengerPhone, PassengerEmail,
FlightNumber, Airline, Sector,
DepartureTime, ArrivalTime, Duration,

OriginAirportName, OriginAirportCode,
DestinationAirportName, DestinationAirportCode,

TerminalOrigin, TerminalDestination,

BaseFare, BaggageAddon, MealAddon,

Baggage, Meals, PaymentMode, TotalAmount, BookingDate, Status)

VALUES
(@BookingId, @PNR, @TicketNumber,
@PassengerTitle, @PassengerFirstName, @PassengerLastName,
@PassengerDOB, @PassengerPhone, @PassengerEmail,
@FlightNumber, @Airline, @Sector,
@DepartureTime, @ArrivalTime, @Duration,

@OriginAirportName, @OriginAirportCode,
@DestinationAirportName, @DestinationAirportCode,

@TerminalOrigin, @TerminalDestination,

@BaseFare, @BaggageAddon, @MealAddon,

@Baggage, @Meals, @PaymentMode, @TotalAmount, GETDATE(),'Confirmed')
";

            using (SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@BookingId", booking.BookingId);
                cmd.Parameters.AddWithValue("@PNR", booking.PNR);
                cmd.Parameters.AddWithValue("@TicketNumber", booking.TicketNumber);
                cmd.Parameters.AddWithValue("@PassengerTitle", booking.PassengerTitle);
                cmd.Parameters.AddWithValue("@PassengerFirstName", booking.PassengerFirstName);
                cmd.Parameters.AddWithValue("@PassengerLastName", booking.PassengerLastName);

                cmd.Parameters.AddWithValue("@PassengerDOB", booking.PassengerDOB);
                cmd.Parameters.AddWithValue("@PassengerPhone", booking.PassengerPhone);
                cmd.Parameters.AddWithValue("@PassengerEmail", booking.PassengerEmail);
                cmd.Parameters.AddWithValue("@FlightNumber", booking.FlightNumber);
                cmd.Parameters.AddWithValue("@Airline", booking.Airline);
                cmd.Parameters.AddWithValue("@Sector", booking.Sector);
                cmd.Parameters.AddWithValue("@DepartureTime", booking.DepartureTime);
                cmd.Parameters.AddWithValue("@ArrivalTime", booking.ArrivalTime);
                cmd.Parameters.AddWithValue("@Baggage", booking.Baggage ?? "Included");
                cmd.Parameters.AddWithValue("@Meals", booking.Meals ?? "Not Selected");
                cmd.Parameters.AddWithValue("@PaymentMode", booking.PaymentMode);
                Console.WriteLine("Payment Mode Received: " + booking.PaymentMode);
                cmd.Parameters.AddWithValue("@TotalAmount", booking.TotalAmount);
                cmd.Parameters.AddWithValue("@Duration", booking.Duration);

                cmd.Parameters.AddWithValue("@OriginAirportName", booking.OriginAirportName);
                cmd.Parameters.AddWithValue("@OriginAirportCode", booking.OriginAirportCode);

                cmd.Parameters.AddWithValue("@DestinationAirportName", booking.DestinationAirportName);
                cmd.Parameters.AddWithValue("@DestinationAirportCode", booking.DestinationAirportCode);

                cmd.Parameters.AddWithValue("@TerminalOrigin", booking.TerminalOrigin);
                cmd.Parameters.AddWithValue("@TerminalDestination", booking.TerminalDestination);

                cmd.Parameters.AddWithValue("@BaseFare", booking.BaseFare);
                cmd.Parameters.AddWithValue("@BaggageAddon", booking.BaggageAddon);
                cmd.Parameters.AddWithValue("@MealAddon", booking.MealAddon);
                cmd.Parameters.AddWithValue("@FlightNumbers", booking.FlightNumbers ?? "");
                cmd.Parameters.AddWithValue("@DepartureTimes", booking.DepartureTimes ?? "");
                cmd.Parameters.AddWithValue("@ArrivalTimes", booking.ArrivalTimes ?? "");
                cmd.ExecuteNonQuery();
            }

            return Ok("Booking Saved Successfully");

        }

        [HttpGet("search")]
        public IActionResult SearchBooking(DateTime? fromDate, DateTime? toDate, string? pnr)
        {
            List<Booking> bookings = new List<Booking>();

            string query = @"
    SELECT * FROM Bookings
    WHERE
    (@FromDate IS NULL OR BookingDate >= @FromDate)
    AND
    (@ToDate IS NULL OR BookingDate < DATEADD(DAY,1,@ToDate))
    AND
    (@PNR IS NULL OR @PNR = '' OR PNR = @PNR)
    ";

            using (SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@FromDate", (object?)fromDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ToDate", (object?)toDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PNR", (object?)pnr ?? DBNull.Value);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    bookings.Add(new Booking
                    {
                        BookingId = reader["BookingId"].ToString(),
                        PNR = reader["PNR"].ToString(),
                        TicketNumber = reader["TicketNumber"].ToString(),

                        PassengerTitle = reader["PassengerTitle"].ToString(),
                        PassengerFirstName = reader["PassengerFirstName"].ToString(),
                        PassengerLastName = reader["PassengerLastName"].ToString(),
                        PassengerDOB = reader["PassengerDOB"] as DateTime?,
                        PassengerPhone = reader["PassengerPhone"].ToString(),
                        PassengerEmail = reader["PassengerEmail"].ToString(),

                        FlightNumber = reader["FlightNumber"].ToString(),
                        Airline = reader["Airline"].ToString(),
                        Sector = reader["Sector"].ToString(),
                        DepartureTime = reader["DepartureTime"].ToString(),
                        ArrivalTime = reader["ArrivalTime"].ToString(),

                        Baggage = reader["Baggage"].ToString(),
                        Meals = reader["Meals"].ToString(),
                        PaymentMode = reader["PaymentMode"].ToString(),
                        TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                        Status = reader["Status"].ToString()
                    });
                }
            }

            return Ok(bookings);
        }
        [HttpPut("cancel")]
        public IActionResult CancelTicket(string? bookingId, string? pnr)
        {
            if (string.IsNullOrEmpty(bookingId) && string.IsNullOrEmpty(pnr))
            {
                return BadRequest("Please enter BookingId or PNR");
            }

            string query = @"
        UPDATE Bookings
        SET Status = 'Cancelled'
        WHERE 
        (@BookingId IS NOT NULL AND BookingId = @BookingId)
        OR
        (@PNR IS NOT NULL AND PNR = @PNR)
    ";

            using (SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@BookingId", (object?)bookingId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PNR", (object?)pnr ?? DBNull.Value);

                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                {
                    return Ok("Ticket Cancelled Successfully");
                }
                else
                {
                    return NotFound("Booking not found");
                }
            }
        }
        [HttpGet("pnr/{pnr}")]
        public IActionResult GetBookingByPnr(string pnr)
        {
            Booking booking = null;

            string query = "SELECT * FROM Bookings WHERE PNR=@PNR";

            using (SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PNR", pnr);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    booking = new Booking
                    {

                        BookingId = reader["BookingId"].ToString(),
                        PNR = reader["PNR"].ToString(),
                        TicketNumber = reader["TicketNumber"].ToString(),

                        PassengerTitle = reader["PassengerTitle"].ToString(),
                        PassengerFirstName = reader["PassengerFirstName"].ToString(),
                        PassengerLastName = reader["PassengerLastName"].ToString(),

                        PassengerPhone = reader["PassengerPhone"].ToString(),
                        PassengerEmail = reader["PassengerEmail"].ToString(),

                        FlightNumber = reader["FlightNumber"].ToString(),
                        Airline = reader["Airline"].ToString(),

                        FromCity = reader["FromCity"].ToString(),
                        ToCity = reader["ToCity"].ToString(),

                        DepartureTime = reader["DepartureTime"].ToString(),
                        ArrivalTime = reader["ArrivalTime"].ToString(),

                        Baggage = reader["Baggage"].ToString(),
                        Meals = reader["Meals"].ToString(),

                        PaymentMode = reader["PaymentMode"].ToString(),

                        TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),

                        BaseFare = Convert.ToDecimal(reader["BaseFare"]),
                        BaggageAddon = Convert.ToDecimal(reader["BaggageAddon"]),
                        MealAddon = Convert.ToDecimal(reader["MealAddon"]),

                        Duration = reader["Duration"].ToString(),

                        OriginAirportName = reader["OriginAirportName"].ToString(),
                        OriginAirportCode = reader["OriginAirportCode"].ToString(),

                        DestinationAirportName = reader["DestinationAirportName"].ToString(),
                        DestinationAirportCode = reader["DestinationAirportCode"].ToString(),

                        TerminalOrigin = reader["TerminalOrigin"].ToString(),
                        TerminalDestination = reader["TerminalDestination"].ToString(),

                        BookingDate = Convert.ToDateTime(reader["BookingDate"]),

                        Status = reader["Status"].ToString()
                    };
                }
            }

            if (booking == null)
            {
                return NotFound("Ticket not found");
            }

            return Ok(booking);
        }
    }

}

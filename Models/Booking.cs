namespace FlightBookingAPI.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public string BookingId { get; set; }
        public string PNR { get; set; }
        public string TicketNumber { get; set; }

        public string? PassengerTitle { get; set; }
        public string? PassengerFirstName { get; set; }
        public string? PassengerLastName { get; set; }

        public DateTime? PassengerDOB { get; set; }
        public string? PassengerPhone { get; set; }
        public string? PassengerEmail { get; set; }

        public string FlightNumber { get; set; }
        public string? Airline { get; set; }
        public string? Sector { get; set; }
        public string? FromCity { get; set; }
        public string? ToCity { get; set; }
        public string? DepartureTime { get; set; }
        public string? ArrivalTime { get; set; }
   
        public string? FlightNumbers { get; set; } // "BK123 BK124"
        public string? DepartureTimes { get; set; } // "2026-05-20T18:10 2026-05-30T09:10"
        public string? ArrivalTimes { get; set; }   // "2026-05-21T08:35 2026-05-30T17:30"
        public string? Baggage { get; set; }
        public string? Meals { get; set; }

        public string? PaymentMode { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal BaseFare { get; set; }

        public decimal BaggageAddon { get; set; }

        public decimal MealAddon { get; set; }

        public string Duration { get; set; }

        public string OriginAirportName { get; set; }

        public string OriginAirportCode { get; set; }

        public string DestinationAirportName { get; set; }

        public string DestinationAirportCode { get; set; }

        public string TerminalOrigin { get; set; }

        public string TerminalDestination { get; set; }
        public DateTime BookingDate { get; set; }
        public string? Status { get; set; }
        
    }
}
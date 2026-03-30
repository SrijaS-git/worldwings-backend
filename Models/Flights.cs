using System.Text.Json;

namespace FlightBookingAPI.Models
{
    
    
        public class Root
        {
            public Response? Response { get; set; }
        


    }

    public class Response
        {
            public List<ResultContainer>? Results { get; set; }
        }

        public class ResultContainer
        {
            public List<Result>? Results { get; set; }
        }

        public class Result
        {
            public bool IsRefundable { get; set; }
            public bool IsLCC { get; set; }

            public Fare? Fare { get; set; }
            public List<FareBreakdown>? FareBreakdown { get; set; }
            public List<SegmentGroup>? Segments { get; set; }
            public List<PassengerFare>? PassengerFares { get; set; }
            public JsonElement? FareRules { get; set; }
        public List<MealInfo> MealDynamic { get; set; }


    }
    public class Fare
        {
            public decimal BaseFare { get; set; }
            public decimal Tax { get; set; }
            public decimal PublishedFare { get; set; }
            public string? Currency { get; set; }
        }
        public class FareBreakdown
        {
            public string? Currency { get; set; }
            public string? PassengerType { get; set; }
            public string? PassengerCount { get; set; }
            public decimal BaseFare { get; set; }
            public decimal Tax { get; set; }
        }
        public class SegmentGroup
        {
            public List<Segment>? Segments { get; set; }
            public List<LayoverInfo>? Layovers { get; set; }
        }
        public class Segment
        {
            public Airline? Airline { get; set; }
            public Origin? Origin { get; set; }
            public Destination? Destination { get; set; }
            public int Duration { get; set; }
            public string? CabinBaggage { get; set; }
            public string? CheckinBaggage { get; set; }
        public int? TripIndicator { get; set; }
        }
        public class Airline
        {
            public string? AirlineCode { get; set; }
            public string? AirlineName { get; set; }
            public string? FlightNumber { get; set; }
        }
        public class Origin
        {
            public Airport? Airport { get; set; }
            public string? DepTime { get; set; }
        }

        public class Destination
        {
            public Airport? Airport { get; set; }
            public string? ArrTime { get; set; }
        }
        public class Airport
        {
            public string? AirportCode { get; set; }
            public string? AirportName { get; set; }
            public string? CityName { get; set; }
            public string? CountryName { get; set; }
            public string? Terminal { get; set; }
        }
        public class PassengerFare
        {
            public string? PaxType { get; set; }          // Adult, Child, etc.
            public decimal? BaseFare { get; set; }
            public decimal? Tax { get; set; }
            public decimal? TotalPerPax { get; set; }
            public int? NoOfPax { get; set; }
            public decimal? Total { get; set; }
            public Fare? Fare { get; set; }
        }

        public class LayoverInfo
        {
            public string? Location { get; set; }
            public string? Duration { get; set; }         
        }

        public class ItineraryItem
        {
            public string? Airline { get; set; }
            public string? FlightNumber { get; set; }
            public string? From { get; set; }
            public string? To { get; set; }
            public DateTime? Departure { get; set; }
            public DateTime? Arrival { get; set; }
            public TerminalInfo? Terminal { get; set; }
            public string? Duration { get; set; }         
            public string? CabinBaggage { get; set; }
            public string? CheckinBaggage { get; set; }
            public List<LayoverInfo>? Layovers { get; set; }
            public List<PassengerFare>? FareDetails { get; set; }
            public FareRulesInfo? FareRules { get; set; }
        }

        public class TerminalInfo
        {
            public string? Origin { get; set; }
            public string? Destination { get; set; }
        }

        public class FareRulesInfo
        {
            public bool? Refundable { get; set; }
            public string? Rules { get; set; }
        }
        public class FareDetailInfo
        {
            public string PaxType { get; set; } = "ADT";
            public decimal BaseFare { get; set; }
            public decimal Tax { get; set; }
            public decimal TotalPerPax { get; set; }
            public int NoOfPax { get; set; }
            public decimal Total { get; set; }
        }

    public class MealInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }




}

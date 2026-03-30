using FlightBookingAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FlightBookingAPI.Controllers
{
    [ApiController]
    [Route("api/flights")]
    public class FlightsController : ControllerBase
    {
        private readonly ILogger<FlightsController> _logger;

        public FlightsController(ILogger<FlightsController> logger)
        {
            _logger = logger;
        }

        [HttpPost("availability")]
        public IActionResult GetAvailability([FromBody] FlightSearchInput input)
        {
            string basePath = Path.Combine(Directory.GetCurrentDirectory(), "Flights");

            if (!Directory.Exists(basePath))
                return NotFound("Flights base folder not found.");

            if (input == null || string.IsNullOrEmpty(input.Source) || string.IsNullOrEmpty(input.Destination) || string.IsNullOrEmpty(input.DepartureDate))
                return BadRequest("Invalid search input.");

            var sourceCode = ConvertCityToCode(input.Source);
            var destinationCode = ConvertCityToCode(input.Destination);

            var filteredFlights = new List<object>();
            var returnFlights = new List<object>();
            var mainFolders = Directory.GetDirectories(basePath);

            foreach (var mainFolder in mainFolders)
            {
                if (!mainFolder.Contains(sourceCode) || !mainFolder.Contains(destinationCode))
                    continue;

                var responseFolders = Directory.GetDirectories(mainFolder).Where(f => f.Contains("Response_TBO")).ToList();

                foreach (var folder in responseFolders)
                {
                    var jsonFiles = Directory.GetFiles(folder, "*.json");

                    foreach (var file in jsonFiles)
                    {
                        string jsonText = System.IO.File.ReadAllText(file);
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var data = JsonSerializer.Deserialize<Root>(jsonText, options);

                        if (data?.Response?.Results == null) continue;

                        foreach (var container in data.Response.Results)
                        {
                            foreach (var result in container.Results ?? new List<Result>())
                            {
                                foreach (var segmentGroup in result.Segments ?? new List<SegmentGroup>())
                                {
                                    if (segmentGroup?.Segments == null || !segmentGroup.Segments.Any()) continue;

                                    var firstSegment = segmentGroup.Segments.First();
                                    var lastSegment = segmentGroup.Segments.Last();
                                    int tripType = firstSegment?.TripIndicator ?? 1;

                                    _logger.LogInformation("Reading file: " + file);
                                    _logger.LogInformation("From JSON: " + firstSegment?.Origin?.Airport?.AirportCode);
                                    _logger.LogInformation("To JSON: " + lastSegment?.Destination?.Airport?.AirportCode);

                                    string from = firstSegment?.Origin?.Airport?.AirportCode ?? "";
                                    string to = lastSegment?.Destination?.Airport?.AirportCode ?? "";
                                    DateTime depDate = ConvertTboDate(firstSegment?.Origin?.DepTime ?? "").Date;

                                    _logger.LogInformation("FROM: " + from);
                                    _logger.LogInformation("TO: " + to);
                                    _logger.LogInformation("DEP DATE: " + depDate.ToString("yyyy-MM-dd"));

                                    // Departure flights
                                    if (tripType == 1)
                                    {
                                        if (from != sourceCode || to != destinationCode || depDate.ToString("yyyy-MM-dd") != input.DepartureDate)
                                            continue;
                                    }

                                    // Return flights
                                    if (tripType == 2)
                                    {
                                        if (from != destinationCode || to != sourceCode || depDate.ToString("yyyy-MM-dd") != input.ReturnDate)
                                            continue;
                                    }

                                    int stops = segmentGroup.Segments.Count - 1;

                                    var flightData = new
                                    {
                                        Airline = firstSegment?.Airline?.AirlineName ?? "",
                                        FlightNumber = firstSegment?.Airline?.FlightNumber ?? "",
                                        From = from,
                                        To = to,
                                        Stops = stops,
                                        Departure = ConvertTboDate(firstSegment?.Origin?.DepTime ?? ""),
                                        Arrival = ConvertTboDate(lastSegment?.Destination?.ArrTime ?? ""),
                                        Price = result?.Fare?.PublishedFare ?? 0,
                                        Refundable = result?.IsRefundable ?? false,
                                        Currency = result?.Fare?.Currency ?? "",
                                        SourceFile = file,

                                        segments = segmentGroup.Segments.Select(s => new
                                        {
                                            origin = s?.Origin?.Airport?.AirportCode ?? "",
                                            destination = s?.Destination?.Airport?.AirportCode ?? "",
                                            departure = ConvertTboDate(s?.Origin?.DepTime ?? ""),
                                            arrival = ConvertTboDate(s?.Destination?.ArrTime ?? ""),
                                            duration = s?.Duration ?? 0
                                        }).ToList(),

                                        fareOptions = new
                                        {
                                            baseFare = result?.Fare?.BaseFare ?? 0,
                                            tax = result?.Fare?.Tax ?? 0,
                                            publishedFare = result?.Fare?.PublishedFare ?? 0,
                                            refundable = result?.IsRefundable ?? false
                                        },

                                        Layovers = segmentGroup.Segments != null && segmentGroup.Segments.Count > 1
                                            ? segmentGroup.Segments
                                                .Zip(segmentGroup.Segments.Skip(1), (first, second) =>
                                                {
                                                    var diff = ConvertTboDate(second.Origin.DepTime) - ConvertTboDate(first.Destination.ArrTime);
                                                    var totalMinutes = (int)diff.TotalMinutes;

                                                    return new LayoverInfo
                                                    {
                                                        Location = first.Destination.Airport.AirportCode,
                                                        Duration = $"{totalMinutes / 60:D2}h{totalMinutes % 60:D2}m"
                                                    };
                                                })
                                                .ToList()
                                            : new List<LayoverInfo>(),

                                        FareDetails = result.PassengerFares != null && result.PassengerFares.Any()
                                            ? result.PassengerFares.Select(f => new FareDetailInfo
                                            {
                                                PaxType = f.PaxType ?? "ADT",
                                                BaseFare = f.BaseFare ?? 0,
                                                Tax = f.Tax ?? 0,
                                                TotalPerPax = f.TotalPerPax ?? 0,
                                                NoOfPax = f.NoOfPax ?? 1,
                                                Total = (f.TotalPerPax ?? 0) * (f.NoOfPax ?? 1)
                                            }).ToList()
                                            : new List<FareDetailInfo>
                                            {
                                                new FareDetailInfo
                                                {
                                                    PaxType = "ADT",
                                                    BaseFare = result.Fare?.BaseFare ?? 0,
                                                    Tax = result.Fare?.Tax ?? 0,
                                                    TotalPerPax = result.Fare?.PublishedFare ?? 0,
                                                    NoOfPax = 1,
                                                    Total = result.Fare?.PublishedFare ?? 0
                                                }
                                            },

                                        Meals = result.MealDynamic != null
                                            ? result.MealDynamic.Select(m => new MealInfo
                                            {
                                                Code = m.Code,
                                                Name = m.Name,
                                                Price = m.Price
                                            }).ToList()
                                            : new List<MealInfo>(),

                                        Itinerary = segmentGroup.Segments.Select(s => new
                                        {
                                            Airline = s?.Airline?.AirlineName ?? "",
                                            FlightNumber = s?.Airline?.FlightNumber ?? "",
                                            From = new
                                            {
                                                Code = s?.Origin?.Airport?.AirportCode ?? "",
                                                Name = s?.Origin?.Airport?.AirportName ?? ""
                                            },
                                            To = new
                                            {
                                                Code = s?.Destination?.Airport?.AirportCode ?? "",
                                                Name = s?.Destination?.Airport?.AirportName ?? ""
                                            },
                                            Departure = ConvertTboDate(s?.Origin?.DepTime ?? ""),
                                            Arrival = ConvertTboDate(s?.Destination?.ArrTime ?? ""),
                                            Terminal = new
                                            {
                                                Origin = s?.Origin?.Airport?.Terminal ?? "",
                                                Destination = s?.Destination?.Airport?.Terminal ?? ""
                                            },
                                            Duration = s?.Duration != null ? $"{s.Duration / 60:D2}h{s.Duration % 60:D2}m" : "",
                                            CabinBaggage = s?.CabinBaggage ?? "7 Kg",
                                            CheckinBaggage = s?.CheckinBaggage ?? "15 Kg",
                                            FareRules = new
                                            {
                                                Refundable = result.IsRefundable,
                                                Rules = result.FareRules.HasValue
                                                    ? result.FareRules.Value.ValueKind switch
                                                    {
                                                        JsonValueKind.String => result.FareRules.Value.GetString(),
                                                        JsonValueKind.Object => result.FareRules.Value.GetRawText(),
                                                        _ => "N/A"
                                                    }
                                                    : "N/A"
                                            }
                                        }).ToList()
                                    };

                                    if (tripType == 1)
                                    {
                                        filteredFlights.Add(flightData);
                                    }
                                    else if (tripType == 2)
                                    {
                                        returnFlights.Add(flightData);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(input.ReturnDate))
            {
                return Ok(filteredFlights);
            }

            return Ok(new
            {
                departureFlights = filteredFlights,
                returnFlights = returnFlights
            });
        }

        private DateTime ConvertTboDate(string tboDate)
        {
            if (string.IsNullOrEmpty(tboDate))
                return DateTime.MinValue;

            try
            {
                var milliseconds = long.Parse(tboDate.Replace("/Date(", "").Replace(")/", ""));
                return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).DateTime;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        private string ConvertCityToCode(string? city)
        {
            if (string.IsNullOrEmpty(city))
                return "";

            var cityMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Chennai", "MAA" },
                { "Delhi", "DEL" },
                { "Mumbai", "BOM" },
                { "Bangalore", "BLR" },
                { "Hyderabad", "HYD" },
                { "Kolkata", "CCU" },
                {"Dubai","DXB"},
                {"Singapore","SIN" }

            };

            return cityMap.ContainsKey(city) ? cityMap[city] : city.ToUpper();
        }

        public class FlightSearchInput
        {
            public string? Source { get; set; }
            public string? Destination { get; set; }
            public string? DepartureDate { get; set; }
            public string? ReturnDate { get; set; }
        }
    }
}
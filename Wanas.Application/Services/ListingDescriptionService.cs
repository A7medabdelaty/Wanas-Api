using Wanas.Application.DTOs.AI;
using Wanas.Application.Interfaces.AI;

namespace Wanas.Application.Services
{
    public class ListingDescriptionService:IListingDescriptionService
    {
        private readonly IAIProvider _ai;
        public ListingDescriptionService(IAIProvider ai )
        {
            _ai = ai;
        }

        public async Task<string> GenerateDescriptionAsync(GenerateDescriptionDto dto)
        {
            var prompt = BuildPrompt(dto);

            var result = await _ai.GenerateTextAsync(prompt);

            return result;
        }


        private string BuildPrompt(GenerateDescriptionDto d)
        {
            var roomsText = "";
            if (d.Rooms != null && d.Rooms.Count > 0)
            {
                roomsText = string.Join("\n", d.Rooms.Select(r =>
                    $"- Room {r.RoomNumber}: {r.BedsCount} beds, {r.AvailableBeds} available, Price per bed: {r.PricePerBed} EGP, AC: {(r.HasAirConditioner ? "Yes" : "No")}"
                ));
            }
            else
            {
                roomsText = "No specific room details provided.";
            }

            return $@"
                        Write an attractive, warm, and persuasive real-estate listing description.
                        It must be one flowing paragraph (3-5 sentences).
                        It should highlight comfort, convenience, lifestyle, and location.
                        it should be in arabic

                        Listing Information:
                        - Title: {d.Title}
                        - City: {d.City}
                        - Address: {d.Address}
                        - Monthly Rent: {d.MonthlyPrice} EGP
                        - Area: {d.AreaInSqMeters} m²
                        - Floor: {d.Floor} (Elevator: {(d.HasElevator ? "Yes" : "No")})
                        - Bathrooms: {d.TotalBathrooms}
                        - Kitchen: {(d.HasKitchen ? "Yes" : "No")}
                        - Internet: {(d.HasInternet ? "Yes" : "No")}
                        - Air Conditioning: {(d.HasAirConditioner ? "Yes" : "No")}
                        - Pet Friendly: {(d.IsPetFriendly ? "Yes" : "No")}
                        - Smoking Allowed: {(d.IsSmokingAllowed ? "Yes" : "No")}

                        Room Details:
                        {roomsText}

                        Do NOT list details as bullet points.
                        Write a natural, elegant real-estate description using full sentences.
                        Use friendly, appealing language that encourages renters.
                        ";
        }

    }
}

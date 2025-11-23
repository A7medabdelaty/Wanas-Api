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
                            return $@"
                Write an attractive, persuasive real-estate listing description in English. 
                Make it warm, engaging, and appealing for someone searching for a comfortable place to live.

                Listing details:
                - Title: {d.Title}
                - City: {d.City}
                - Address: {d.Address}
                - Monthly Rent: {d.MonthlyPrice} EGP
                - Area: {d.AreaInSqMeters} m²
                - Floor: {d.Floor} (Elevator available: {(d.HasElevator ? "Yes" : "No")})
                - Rooms: {d.AvailableRooms}/{d.TotalRooms} available
                - Beds: {d.AvailableBeds}/{d.TotalBeds} available
                - Bathrooms: {d.TotalBathrooms}
                - Kitchen: {(d.HasKitchen ? "Yes" : "No")}
                - Internet: {(d.HasInternet ? "Yes" : "No")}

                Write the description in 4–6 sentences. 
                Do NOT list the details as bullet points. 
                Write as a flowing paragraph.
                            ";
        }
    }
}

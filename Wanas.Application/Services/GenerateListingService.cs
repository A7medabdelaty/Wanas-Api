using AutoMapper;
using System.Text.Json;
using Wanas.Application.DTOs.AI;
using Wanas.Application.Interfaces.AI;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class GenerateListingService : IGenerateListingService
    {
        private readonly IAIProvider _aiProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GenerateListingService(IAIProvider aiProvider, IUnitOfWork unitOfWork , IMapper mapper)
        {
            _aiProvider = aiProvider;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        
        public async Task<GeneratedListingResponseDto> GenerateListingAsync(GenerateListingRequestDto request , string ownerId)
        {
            var prompt = BuildPrompt(request);

            var aiResult = await _aiProvider.GenerateTextAsync(prompt);

            var generatedDto = JsonSerializer.Deserialize<GeneratedListingResponseDto>(aiResult, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

            if(generatedDto == null)
            {
                throw new Exception("Ai returned invalid JSON");
            }

            var listing = _mapper.Map<Listing>(generatedDto);

            listing.UserId=ownerId;
            listing.IsActive= false;
            listing.CreatedAt= DateTime.UtcNow; 

            await _unitOfWork.Listings.AddAsync(listing);
            await _unitOfWork.CommitAsync();

            return generatedDto;

        }

        public string BuildPrompt(GenerateListingRequestDto request)
        {
            return $@"
                        Generate a real estate listing suggestion in STRICT JSON format. 
                        Do not include any text outside the JSON. 

                        Fields must match:
                        - title (string)
                        - description (string)
                        - tags (list of strings)
                        - suggestedMonthlyPrice (int)
                        - city (string)
                        - roomType (string)
                        - amenities (list of strings)
                        - propertyRules (list of strings)
                        - suggestedPhotoUrls (list of strings)
                        - readyToPublish (bool)

                        User input: 
                        City: {request.City ?? ""}
                        RoomType: {request.RoomType ?? ""}
                        Bedrooms: {request.Bedrooms}
                        MinPrice: {request.MinPrice}
                        MaxPrice: {request.MaxPrice}
                        Amenities: {string.Join(", ", request.Amenities ?? new List<string>())}
                        PropertyRules: {string.Join(", ", request.PropertyRules ?? new List<string>())}
                        ";
        }

    }
}

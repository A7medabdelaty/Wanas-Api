namespace Wanas.Application.DTOs.AI
{
    public class GenerateDescriptionDto
    {
        public string Title { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public int MonthlyPrice { get; set; }

        public bool HasElevator { get; set; }
        public string Floor { get; set; }
        public int AreaInSqMeters { get; set; }

        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int TotalBeds { get; set; }
        public int AvailableBeds { get; set; }
        public int TotalBathrooms { get; set; }

        public bool HasKitchen { get; set; }
        public bool HasInternet { get; set; }
    }
}


namespace Wanas.Domain.Entities
{
    public class ApartmentListing
    {
        public int Id { get; set; }
        public string Address { get; set; }

        //public int? MinimumPrice { get; set; }
        //public int? MaximumPrice { get; set; }
        public int MonthlyPrice { get; set; }
        public bool HasElevator { get; set; }
        public string Floor { get; set; }

        // public int NoOfApartmentsInAFloor { get; set; }
        public int AreaInSqMeters { get; set; }
        //public int TotalRooms { get; set; }
        //public int AvailableRooms { get; set; }
        //public int TotalBeds { get; set; }
        //public int AvailableBeds { get; set; }
        public int TotalBathrooms { get; set; }
        public bool HasKitchen { get; set; }
        public bool HasInternet { get; set; }
        public bool HasAirConditioner { get; set; }
        public bool HasFans { get; set; }
        public bool IsPetFriendly { get; set; }
        public bool IsSmokingAllowed { get; set; }

        // HasFridge, HasStove, NoOfAirConditioners, HasFans, NoOfFans, HasWashingMachine

        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        public ICollection<Bed> Beds { get; set; } = new List<Bed>();
        public int ListingId { get; set; }
        public virtual Listing Listing { get; set; }
    }
}

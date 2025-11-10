namespace Wanas.Domain.Entities
{
    public class ListingPhoto
    {
        public int Id { get; set; }
        public string URL { get; set; }
        public int ListingId { get; set; }
        public Listing Listing { get; set; }
    }
}

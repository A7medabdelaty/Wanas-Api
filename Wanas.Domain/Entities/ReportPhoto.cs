namespace Wanas.Domain.Entities;
public class ReportPhoto
{
    public int Id { get; set; }
    public string URL { get; set; }
    public int ReportId { get; set; }
    public virtual Report Report { get; set; }=null!;
}

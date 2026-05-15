namespace EventHub.Web.ViewModels;

public class EventFilterViewModel
{
    public string? Search { get; set; }
    public string? Location { get; set; }
    public int? CategoryId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public decimal? MaxPrice { get; set; }
}

namespace OrderApp.Shared.Dto;

public class FilterParameter
{
    public string SearchString { get; set; } = "";
    public string SortLabel { get; set; } = "";
    public SortDirection SortDirection { get; set; } = 0;
    public int PageIndex { get; set; }= 1;
    public int PageSize { get; set; } = 10;
    public List<int>? ProvidersIdFilter { get; set; }
    public List<string>? OrdersIdFilter { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set;}

}

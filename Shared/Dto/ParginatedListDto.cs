namespace OrderApp.Shared.Dto;

public class ParginatedListDto<T> where T : IDto
{
    public long TotalItems { get; set; }

    public List<T> Items { get; set; }
}

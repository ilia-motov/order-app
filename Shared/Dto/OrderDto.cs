using System.ComponentModel.DataAnnotations.Schema;

namespace OrderApp.Shared.Dto;

public class OrderDto : IDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTime Date { get; set; }
    public int ProviderId { get; set; }
    public ProviderDto? Provider { get; set; }
    public List<OrderItemDto>? Items { get; set; }

    public override string ToString()
    {
        return Name;
    }
}

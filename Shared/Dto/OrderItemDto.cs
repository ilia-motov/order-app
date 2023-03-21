namespace OrderApp.Shared.Dto;

public class OrderItemDto : IDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Name { get; set; }
    public double Quantity { get; set; }
    public string Unit { get; set; }

    //public OrderDto? Order { get; set; }
}

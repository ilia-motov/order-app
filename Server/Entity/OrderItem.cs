namespace OrderApp.Server.Entity;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Name { get; set; }
    public double Quantity { get; set; }
    public string Unit { get; set; }
    public Order? Order { get; set; }
}

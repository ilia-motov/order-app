namespace OrderApp.Server.Entity;

public class Provider
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Order>? Orders { get; set; }
}

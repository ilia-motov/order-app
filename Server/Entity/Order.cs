using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderApp.Server.Entity;

public class Order
{
    public int Id { get; set; }

    [Column("number")]
    public string Name { get; set; }

    [Column(TypeName = "DATE")]
    public DateTime Date { get; set; }
    public int ProviderId { get; set; }
    public Provider? Provider { get; set; }
    public List<OrderItem>? OrderItems { get; set; }
}

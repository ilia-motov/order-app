using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OrderApp.Server.Entity;
using System.Globalization;

namespace OrderApp.Server.DataAcsess;

public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Provider> Providers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connection = new SqliteConnection("Filename=OrderAppDb.db");

        connection.CreateCollation("NOCASE", (x, y) => string.Compare(x, y, true, new CultureInfo("ru-RU")));
        connection.CreateFunction("upper", (string value) => value.ToUpper(new CultureInfo("ru-RU")));
        connection.CreateFunction("lower", (string value) => value.ToLower(new CultureInfo("ru-RU")));

        optionsBuilder.UseSqlite(connection)
        .UseSnakeCaseNamingConvention();
    }
}

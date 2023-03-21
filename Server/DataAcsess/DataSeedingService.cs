using OrderApp.Server.Entity;

namespace OrderApp.Server.DataAcsess;

public interface IDataSeedingService
{
    void SeedDatabase();
}
public class DataSeedingService : IDataSeedingService
{
    private readonly OrderDbContext _context;

    public DataSeedingService(OrderDbContext context)
    {
        _context= context ?? throw new ArgumentNullException(nameof(context));
    }

    public void SeedDatabase()
    {
        if (!_context.Database.EnsureCreated())
            return;

        var providers = new List<Provider> 
        {
            new Provider() { Name = "Provider 1" },
            new Provider() { Name = "Provider 2" },
            new Provider() { Name = "Provider 3" }
        };

        _context.AddRange(providers);
        _context.SaveChanges();
    }
}

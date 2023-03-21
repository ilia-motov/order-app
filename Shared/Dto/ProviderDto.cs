namespace OrderApp.Shared.Dto;

public class ProviderDto : IDto
{
    public int Id { get; set; }
    public string Name { get; set; }

    public override string ToString()
    {
        return Name;
    }
}

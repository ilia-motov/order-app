using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderApp.Server.Entity;
using OrderApp.Shared;
using OrderApp.Shared.Dto;
using System.Linq.Expressions;
using TanvirArjel.EFCore.GenericRepository;

namespace OrderApp.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProviderController
{
    private readonly IRepository _repository;

    public ProviderController(IRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    [HttpGet("[action]")]
    public async Task<List<ProviderDto>> Autocomplete(string searchString)
    {
        var specification = new Specification<Provider>
        {
            Conditions = new List<Expression<Func<Provider, bool>>>
                {
                    x => string.IsNullOrWhiteSpace(searchString)
                    || x.Id.ToString().Contains(searchString.Trim())
                    || x.Name.Trim().ToLower().Contains(searchString.Trim().ToLower())
                },
            Take = 10,
            OrderByDynamic = (nameof(Provider.Name), SortDirection.Ascending.ToString())
        };

        return await _repository.GetListAsync(specification, x => new ProviderDto
        {
            Id = x.Id,
            Name = x.Name,
        });
    }

    [HttpGet("[action]")]
    public async Task<List<ProviderDto>> ReadProviderForFilter()
    {
        var providers = (await _repository.GetListAsync<Provider>())
            .Select(x => new ProviderDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToList();

        return providers;
    }

    [HttpGet("read-all")]
    public async Task<List<ProviderDto>> ReadAll(string searchString = "",string sortLabel = "", SortDirection sortDirection = SortDirection.Ascending)
    {
        var specification = new Specification<Provider>
        {
            Conditions = new List<Expression<Func<Provider, bool>>>
            {
                x => string.IsNullOrWhiteSpace(searchString)
                || x.Name.Trim().ToLower().Contains(searchString.Trim().ToLower())
            },
        };

        if (sortLabel != null)
            specification.OrderByDynamic = (sortLabel, sortDirection == SortDirection.Ascending ? "Asc" : "Desc");

        var providers = await _repository.GetListAsync(specification, x => new ProviderDto
        {
            Id = x.Id,
            Name = x.Name
        });

        return providers;
    }

    [HttpPut]
    public async Task<int> Create(ProviderDto providerDto)
    {
        var dbProviderSameNames = (await _repository.GetListAsync<Provider>())
            .Where(p => providerDto.Name.Trim().ToLower() == p.Name.Trim().ToLower())
            .Select(x => x)
            .ToList();

        if (dbProviderSameNames.Any())
            throw new ArgumentException("Поставщик с таким именем существует в системе!");

        var provider = new Provider
        {
            Name = providerDto.Name,
        };
        await _repository.AddAsync(provider);
        await _repository.SaveChangesAsync();

        return provider.Id;
    }

    [HttpPatch]
    public async Task Update(ProviderDto providerDto)
    {

        var providerItem = await _repository.GetByIdAsync<Provider>(providerDto.Id);

        if (providerItem == null) return;

        providerItem.Name = providerDto.Name;

        _repository.Update(providerItem);
        await _repository.SaveChangesAsync();
    }

    [HttpDelete]
    public async Task Delete(int id)
    {
        var providerItem = await _repository.GetByIdAsync<Provider>(id, c => c
                .Include(x => x.Orders));

        if (providerItem is null)
            throw new InvalidOperationException($"Поставщик с Id: {id} не найден");

        if (providerItem.Orders != null && providerItem.Orders.Any())
            throw new InvalidOperationException($"Невозможно удалить клиента: '{providerItem.Name}'!  Он используется в системе!");

        _repository.Remove(providerItem);
        await _repository.SaveChangesAsync();
    }

}

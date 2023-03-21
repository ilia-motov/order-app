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
public class OrderController : ControllerBase
{
    private readonly IRepository _repository;

    public OrderController(IRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    [HttpGet("[action]")]
    public async Task<List<OrderDto>> Autocomplete(string searchString)
    {
        var specification = new Specification<Order>
        {
            Conditions = new List<Expression<Func<Order, bool>>>
                {
                    x => string.IsNullOrWhiteSpace(searchString)
                    || x.Id.ToString().Contains(searchString.Trim())
                    || x.Name.Trim().ToLower().Contains(searchString.Trim().ToLower())
                },
            Take = 10,
            OrderByDynamic = (nameof(Order.Name), SortDirection.Ascending.ToString())
        };

        return await _repository.GetListAsync(specification, x => new OrderDto
        {
            Id = x.Id,
            Name = x.Name,
        });
    }

    [HttpGet("[action]")]
    public async Task<List<OrderItemDto>> ReadByIdOrder(int orderId)
    {


        var orderItems = (await _repository.GetListAsync<OrderItem>())
            .Where(x => x.OrderId == orderId)
            .Select(x => new OrderItemDto
            {
                Id = x.Id,
                Name = x.Name,
                Quantity = x.Quantity,
                Unit = x.Unit,
                OrderId=x.OrderId
            })
            .ToList();

        return orderItems;
    }

    [HttpGet("[action]")]
    public async Task<List<OrderDto>> ReadOrdersForFilter()
    {
        var orders = (await _repository.GetListAsync<Order>())
            .Select(x => new OrderDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToList();

        return orders;
    }

    [HttpPut]
    public async Task<int> Create(OrderDto orderDto)
    {
        var dbOrderSameNames = (await _repository.GetListAsync<Order>())
            .Where(p => orderDto.Name.Contains(p.Name))
            .Select(x => x)
            .ToList();

        if(dbOrderSameNames.Any(x => x.ProviderId == orderDto.ProviderId))
            throw new ArgumentException("Заказ с данным поставщиком существует в системе");

        var order = new Order()
        {
            Name = orderDto.Name,
            Date = DateTime.Now,
            ProviderId = orderDto.ProviderId,
            OrderItems = orderDto.Items?.Select(i => new OrderItem()
            {
                Name = i.Name,
                Quantity = i.Quantity,
                Unit = i.Unit
            })
            .ToList()
    };
        _repository.Add(order);
        await _repository.SaveChangesAsync();

        return order.Id;
    }

    [HttpGet("[action]")]
    public async Task<OrderDto> Read(int id)
    {
        var specification = new Specification<Order>
        {
            Conditions = new List<Expression<Func<Order, bool>>>
            {
                x => x.Id== id
            },
            Includes = x => x.Include(x => x.Provider).Include(x =>x.OrderItems)
        };

        var order = await _repository.GetAsync(specification);

        return new OrderDto
        {
            Id = order.Id,
            Name = order.Name,
            Date = order.Date,
            Provider = new ProviderDto
            {
                Id = order.Provider.Id,
                Name = order.Provider.Name
            },
            Items = order.OrderItems
                    .Select(i => new OrderItemDto
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Quantity = i.Quantity,
                        OrderId= i.OrderId,
                        Unit = i.Unit
                    })
                    .ToList()
        };
    }

    [HttpPost("[action]")]
    public async Task<ParginatedListDto<OrderDto>> ReadAll(FilterParameter parametr)
    {
        var specification = new PaginationSpecification<Order>
        {
            Includes = x => x.Include(x => x.Provider).Include(x => x.OrderItems),
            Conditions = new List<Expression<Func<Order, bool>>>
            {
                x => string.IsNullOrWhiteSpace(parametr.SearchString)
                || x.Name.Trim().ToLower().Contains(parametr.SearchString.Trim().ToLower())
                || x.Provider.Name.Trim().ToLower().Contains(parametr.SearchString.Trim().ToLower())
                || x.OrderItems
                .Where(i => i.Name.Trim().ToLower().Contains(parametr.SearchString.Trim().ToLower()))
                .Count() > 0
            },
            PageIndex = parametr.PageIndex,
            PageSize = parametr.PageSize
        };

        if (parametr.ProvidersIdFilter != null && parametr.ProvidersIdFilter.Any())
            specification.Conditions.Add(x => parametr.ProvidersIdFilter.Contains(x.ProviderId));

        if (parametr.OrdersIdFilter != null && parametr.OrdersIdFilter.Any())
            specification.Conditions.Add(x => parametr.OrdersIdFilter.Contains(x.Name));

        if (parametr.SortLabel != null)
            specification.OrderByDynamic = (parametr.SortLabel, parametr.SortDirection == SortDirection.Ascending ? "Asc" : "Desc");

        specification.Conditions.Add(x => parametr.StartTime <= x.Date);
        specification.Conditions.Add(x => parametr.EndTime >= x.Date.AddDays(-1));

        var paginatedList = await _repository.GetListAsync(specification, x => new OrderDto
        {
            Id = x.Id,
            Name = x.Name,
            Date = x.Date,
            Provider = new ProviderDto
            {
                Id = x.Provider.Id,
                Name = x.Provider.Name
            }
        });
        return new ParginatedListDto<OrderDto>
        {
            TotalItems = paginatedList.TotalItems,
            Items = paginatedList.Items,
        };
    }

    [HttpPatch]
    public async Task Update(OrderDto orderDto)
    {
        var specification = new Specification<Order>
        {
            Conditions = new List<Expression<Func<Order, bool>>>
            {
                x => x.Id== orderDto.Id
            },
            Includes = x => x.Include(x => x.OrderItems)
        };

        var order = await _repository.GetAsync<Order>(specification);

        if (order == null) return;

        order.Name = orderDto.Name;

        if(orderDto.Date > DateTime.Now)
            throw new InvalidDataException("Некорректная дата!");

        order.Date = orderDto.Date;
        order.ProviderId = orderDto.ProviderId;
        order.OrderItems = orderDto.Items
            .Select(x => new OrderItem
            {
                Id = x.Id,
                Name = x.Name,
                Quantity= x.Quantity,
                Unit = x.Unit,
                OrderId = x.OrderId,
            })
            .ToList();

        await _repository.UpdateAsync(order);
        await _repository.SaveChangesAsync();
    }

    [HttpDelete]
    public async Task Delete(int id)
    {
        var order = await _repository.GetByIdAsync<Order>(id);

        if (order is null)
            throw new InvalidOperationException($"Заказ {id} не найден");

        _repository.Remove(order);
        await _repository.SaveChangesAsync();
    }
}

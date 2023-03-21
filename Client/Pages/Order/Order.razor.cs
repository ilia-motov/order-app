using Microsoft.AspNetCore.Components;
using MudBlazor;
using Newtonsoft.Json;
using OrderApp.Client.Pages.Order.Components;
using OrderApp.Client.Pages.OrderItem.Components;
using OrderApp.Client.Shared;
using OrderApp.Shared.Dto;
using System.Net.Http.Json;

namespace OrderApp.Client.Pages.Order
{
    public partial class Order
    {
        private IEnumerable<ProviderDto> _providerFilter;

        private IEnumerable<OrderDto> _orderFilter;

        private MudTable<OrderDto> _table;

        private string _searchString = string.Empty;

        private OrderDto _itemBeforeEdit;

        private List<OrderItemDto> _orderItems = new List<OrderItemDto>();

        private DateTime? _endDate = DateTime.Now;

        private DateTime? _startDate = DateTime.Now.AddMonths(-1);

        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public ISnackbar Snackbar { get; set; }

        [Inject]
        public HttpClient Http { get; set; }

        public bool showOrderItems = false;

        private async Task<TableData<OrderDto>> ServerReload(TableState state)
        {
            var requestUri = "api/Order/ReadAll";

            var parameter = new FilterParameter
            {
                SearchString = _searchString,
                SortDirection = (OrderApp.Shared.SortDirection)state.SortDirection,
                SortLabel = state.SortLabel ?? "",
                PageIndex = state.Page + 1,
                PageSize = state.PageSize,
                ProvidersIdFilter = _providerFilter?.Select(p => p.Id).ToList(),
                OrdersIdFilter = _orderFilter?.Select(x => x.Name).ToList(),
                StartTime = _startDate,
                EndTime= _endDate
            };

            HttpResponseMessage response;

            try
            {
                response = await Http.PostAsJsonAsync(requestUri, parameter);
            }
            catch (Exception e)
            {
                Snackbar.Add(e.Message);
                return new TableData<OrderDto>();
            }

            var content = await response.Content.ReadAsStringAsync();
            ParginatedListDto<OrderDto> result = JsonConvert.DeserializeObject <ParginatedListDto<OrderDto>> (content);

            return new TableData<OrderDto>() { TotalItems = (int)result.TotalItems, Items = result.Items };
        }

        private async Task OnSearch(string text)
        {
            _searchString = text;
            await _table.ReloadServerData();
        }

        private async Task Delete()
        {
            if (_table.SelectedItems.Count == 0)
                return;

            var closeOnEscapeKey = new DialogOptions() { CloseOnEscapeKey = true };
            var dialogResult = await DialogService.Show<DeleteDialog>("Удалить", closeOnEscapeKey).Result;

            if (dialogResult.Cancelled)
                return;

            foreach (var item in _table.SelectedItems)
                await Http.DeleteAsync($"api/Order?id={item.Id}");

            await _table.ReloadServerData();
        }

        private async Task Create()
        {
            var parameters = new DialogParameters();

            parameters.Add("Order", new OrderDto
            {
                Name = "",
                Items = new List<OrderItemDto>()
            }); ;

            var closeOnEscapeKey = new DialogOptions() { CloseOnEscapeKey = true, FullScreen = true };

            var dialogResult = await DialogService.Show<CreateOrderDialog>("Создание нового заказа", parameters, closeOnEscapeKey).Result;

            if (!dialogResult.Cancelled)
                await _table.ReloadServerData();
        }

        private async Task ShowOrderItems(OrderDto order)
        {
            var parameters = new DialogParameters();

            order.Items = await Http.GetFromJsonAsync<List<OrderItemDto>>($"api/Order/ReadByIdOrder?orderId={order.Id}");

            parameters.Add("Order", order);

            var closeOnEscapeKey = new DialogOptions() { CloseOnEscapeKey = true, FullScreen = true };
            var dialogResult = await DialogService.Show<EditOrderItemsDialog>("Детали заказа", parameters, closeOnEscapeKey).Result;

            if (!dialogResult.Cancelled)
                await _table.ReloadServerData();
        }

        private async void UpdateProviderFilter(IEnumerable<ProviderDto> providers)
        {
            _providerFilter = providers;
            await _table.ReloadServerData();
        }

        private async void UpdateOrderFilter(IEnumerable<OrderDto> orders)
        {
            _orderFilter = orders;
            await _table.ReloadServerData();
        }

        public async Task SetStartDate(DateTime? date)
        {
            _startDate = date;
            await _table.ReloadServerData();
        }

        public async Task SetEndDate(DateTime? date)
        {
            _endDate = date;
            await _table.ReloadServerData();
        }

    }
}

using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderApp.Client.Pages.OrderItem.Components;
using OrderApp.Shared.Dto;

namespace OrderApp.Client.Shared
{
    public partial class OrderItemTable
    {
        private MudTable<OrderItemDto> _table;
        private bool blockSwitch = false;
        private string searchString = "";
        private OrderItemDto selectedItem1 = null;
        private OrderItemDto elementBeforeEdit;
        private TableApplyButtonPosition applyButtonPosition = TableApplyButtonPosition.End;
        private TableEditButtonPosition editButtonPosition = TableEditButtonPosition.End;
        private TableEditTrigger editTrigger = TableEditTrigger.RowClick;

        [Inject]
        public HttpClient Http { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        [Parameter]
        public int OrderId { get; set; }

        [Parameter]
        public List<OrderItemDto> Items { get; set; }

        [Parameter]
        public EventCallback<List<OrderItemDto>> ItemsChanged { get; set; }

        private void BackupItem(object element)
        {
            var orderItemElement = element as OrderItemDto;

            if (orderItemElement != null)
            {
                elementBeforeEdit = new()
                {
                    Id = orderItemElement.Id,
                    OrderId = orderItemElement.OrderId,
                    Name = orderItemElement.Name,
                    Quantity = orderItemElement.Quantity,
                    Unit = orderItemElement.Unit
                };
                StateHasChanged();
            }
        }

        private void ItemHasBeenCommitted(object element)
        {
            StateHasChanged();
        }

        private void ResetItemToOriginalValues(object element)
        {
            var orderItemElement = element as OrderItemDto;

            if (orderItemElement != null)
            {
                orderItemElement.Id = elementBeforeEdit.Id;
                orderItemElement.Name = elementBeforeEdit.Name;
                orderItemElement.OrderId = elementBeforeEdit.OrderId;
                orderItemElement.Quantity = elementBeforeEdit.Quantity;
                orderItemElement.Unit = elementBeforeEdit.Unit;
            }
        }

        private bool FilterFunc(OrderItemDto element)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;
            if (element.Unit.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (element.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if ($"{element.Id} {element.Quantity}".Contains(searchString))
                return true;
            return false;
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
                Items.Remove(item);
        }


        private async Task Create()
        {
            var closeOnEscapeKey = new DialogOptions() { CloseOnEscapeKey = true};
            var parameters = new DialogParameters();

            parameters.Add("OrderId", OrderId);

            var dialogResult = await DialogService.Show<CreateOrderItemDialog>("Создание нового элемента заказа", parameters, closeOnEscapeKey).Result;

            if (!dialogResult.Cancelled)
            {
                var data = (OrderItemDto)dialogResult.Data;

                if (data != null)
                    Items.Add(data);
            }
        }
    }
}

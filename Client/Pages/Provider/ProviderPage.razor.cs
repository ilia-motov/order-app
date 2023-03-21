using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderApp.Client.Pages.Provider.Components;
using OrderApp.Client.Shared;
using OrderApp.Shared.Dto;
using System.Net;
using System.Net.Http.Json;

namespace OrderApp.Client.Pages.Provider
{
    public partial class ProviderPage
    {
        private MudTable<ProviderDto> _table;

        private string _searchString = string.Empty;

        private ProviderDto _itemBeforeEdit;

        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public ISnackbar Snackbar { get; set; }

        [Inject]
        public HttpClient Http { get; set; }

        private async Task<TableData<ProviderDto>> ServerReload(TableState state)
        {
            var requestUri = $"api/Provider/read-all?searchString={_searchString}" +
                $"&sortLabel={state.SortLabel}&sortDirection={state.SortDirection}";
            var data = await Http.GetFromJsonAsync<List<ProviderDto>>(requestUri);
            return new TableData<ProviderDto>() {Items = data };
        }
        private async Task OnSearch(string text)
        {
            _searchString = text;
            await _table.ReloadServerData();
        }
        private void BackupItem(object element)
        {
            var providerElement = element as ProviderDto;

            if (providerElement != null)
            {
                _itemBeforeEdit = new()
                {
                    Id = providerElement.Id,
                    Name = providerElement.Name,
                };
                StateHasChanged();
            }
        }
        private void ItemHasBeenCommitted(object element)
        {
            Http.PatchAsJsonAsync<ProviderDto>("api/Provider", (ProviderDto)element);
            StateHasChanged();
        }

        private void ResetItemToOriginalValues(object element)
        {
            var providerElement = element as OrderDto;

            if (providerElement != null)
            {
                providerElement.Id = _itemBeforeEdit.Id;
                providerElement.Name = _itemBeforeEdit.Name;

                StateHasChanged();
            }
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

                try
                {
                    var response = await Http.DeleteAsync($"api/Provider?id={item.Id}");
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        var message = await response.Content.ReadAsStringAsync();
                        Snackbar.Add(message, Severity.Warning);
                    }
                }
                catch (Exception e)
                {
                    Snackbar.Add(e.Message);
                    return;
                }

            await _table.ReloadServerData();
        }
        private async Task Create()
        {
            var closeOnEscapeKey = new DialogOptions() { CloseOnEscapeKey = true };
            var dialogResult = await DialogService.Show<CreateProviderDialog>("Создание поставщика", closeOnEscapeKey).Result;

            if (!dialogResult.Cancelled)
                await _table.ReloadServerData();
        }
    }
}

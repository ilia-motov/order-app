using MudBlazor;
using OrderApp.Client.Pages.Order.Components;
using OrderApp.Client.Pages.Provider.Components;
using OrderApp.Shared.Dto;

namespace OrderApp.Client.Services
{
    public interface ICreateDialogService
    {
        Task<T?> Show<T>(string name) where T : class, IDto;
    }

    public class CreateDialogService : ICreateDialogService
    {
        private readonly IDialogService _dialogService;

        public CreateDialogService(IDialogService dialogService) =>
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        public async Task<T?> Show<T>(string name) where T : class, IDto
        {
            T? result = null;

            var parameters = new DialogParameters();
            var closeOnEscapeKey = new DialogOptions() { CloseOnEscapeKey = true };

            parameters.Add("Name", name);

            if (typeof(T) == typeof(ProviderDto))
            {
                var dialogResult = await _dialogService.Show<CreateProviderDialog>("Создание поставщика", parameters, closeOnEscapeKey).Result;
                if (!dialogResult.Cancelled)
                    result = dialogResult.Data as T;
            }

            return result;
        }
    }
}

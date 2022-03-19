using Blazored.Toast.Services;

namespace Sinance.BlazorApp.Services
{
    public class UserErrorService : IUserErrorService
    {
        private readonly IToastService toastService;

        public UserErrorService(IToastService toastService)
        {
            this.toastService = toastService;
        }

        public void ShowErrorToUser(string error)
        {
            toastService.ShowError(error);
        }
    }
}

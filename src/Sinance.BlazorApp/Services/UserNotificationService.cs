using Blazored.Toast.Services;

namespace Sinance.BlazorApp.Services;

public class UserNotificationService : IUserNotificationService
{
    private readonly IToastService toastService;

    public UserNotificationService(IToastService toastService)
    {
        this.toastService = toastService;
    }

    public void ShowSuccess(string message)
    {
        toastService.ShowSuccess(message);
    }

    public void ShowError(string message)
    {
        toastService.ShowError(message);
    }
}

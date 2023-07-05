namespace Sinance.BlazorApp.Services;

public interface IUserNotificationService
{
    void ShowError(string error);
    void ShowSuccess(string message);
}
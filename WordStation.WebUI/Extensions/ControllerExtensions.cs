using Microsoft.AspNetCore.Mvc;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace WordStation.WebUI.Extensions
{
    public static class ControllerExtensions
    {
        public static void NotifySuccess(this Controller controller, string title, string message)
            => Notify(controller, title, message, NotificationType.Success);

        public static void NotifyError(this Controller controller, string title, string message)
            => Notify(controller, title, message, NotificationType.Error);

        public static void NotifyInfo(this Controller controller, string title, string message)
            => Notify(controller, title, message, NotificationType.Info);

        private static void Notify(Controller controller, string title, string message, NotificationType type)
        {
            var notyf = controller.HttpContext.RequestServices.GetService<INotyfService>();
            if (notyf != null)
            {
                var fullMessage = string.IsNullOrEmpty(title) ? message : $"{title}: {message}";
                switch (type)
                {
                    case NotificationType.Success:
                        notyf.Success(fullMessage);
                        break;
                    case NotificationType.Error:
                        notyf.Error(fullMessage);
                        break;
                    case NotificationType.Info:
                        notyf.Information(fullMessage);
                        break;
                }
            }
        }

        private enum NotificationType
        {
            Success,
            Error,
            Info
        }
    }
}

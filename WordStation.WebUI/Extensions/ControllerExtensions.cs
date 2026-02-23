using Microsoft.AspNetCore.Mvc;
using WordStation.WebUI.Models;

namespace WordStation.WebUI.Extensions
{
    public static class ControllerExtensions
    {
        public static void NotifySuccess(this Controller controller, string title, string message)
            => Notify(controller, title, message, AlertType.Success);

        public static void NotifyError(this Controller controller, string title, string message)
            => Notify(controller, title, message, AlertType.Error);

        public static void NotifyInfo(this Controller controller, string title, string message)
            => Notify(controller, title, message, AlertType.Info);

        private static void Notify(Controller controller, string title, string message, AlertType type)
        {
            controller.TempData["AlertTitle"] = title;
            controller.TempData["AlertMessage"] = message;
            controller.TempData["AlertType"] = type.ToString();
        }
    }
}

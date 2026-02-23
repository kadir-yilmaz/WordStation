using Microsoft.AspNetCore.Mvc;
using WordStation.WebUI.Models;

namespace WordStation.WebUI.ViewComponents
{
    public class AlertViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            if (TempData["AlertMessage"] == null)
                return Content(string.Empty);

            var model = new AlertModel
            {
                Title = TempData["AlertTitle"]?.ToString() ?? "Notification",
                Message = TempData["AlertMessage"]?.ToString() ?? string.Empty,
                Type = Enum.TryParse<AlertType>(TempData["AlertType"]?.ToString(), out var type) ? type : AlertType.Info
            };

            return View(model);
        }
    }
}

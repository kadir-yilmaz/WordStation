namespace WordStation.WebUI.Models
{
    public class AlertModel
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public AlertType Type { get; set; }
    }

    public enum AlertType
    {
        Success,
        Error,
        Info
    }
}

namespace NotificationWebsite.Data
{
    public class NotificationSettings
    {
        public string RelativePath{ get; set; } = default!;
        public string GreetTemplate { get; set; } = default!;
        public string RemindTemplate { get; set; } = default!;
        public string Cron { get; set; } = default!;
    }
}

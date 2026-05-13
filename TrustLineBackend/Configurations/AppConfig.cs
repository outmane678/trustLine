namespace AnonymousComplaintsAPI.Configurations
{
    public class AppConfig
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Ips { get; set; } = new();
    }
}

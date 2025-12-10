namespace mcarthey.com.Models
{
    public class Project
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AsciiArt { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Technologies { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public string StatusColor { get; set; } = "green-400";
        public int Year { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? GitHubUrl { get; set; }
        public string? DemoUrl { get; set; }
    }
}

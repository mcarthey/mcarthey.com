namespace mcarthey.com.Models
{
    public class Hobby
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AsciiArt { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; }
        public string? ImageUrl { get; set; }
        public List<string> Highlights { get; set; } = new();
    }
}

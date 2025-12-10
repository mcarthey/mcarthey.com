namespace mcarthey.com.Models
{
    public class Skill
    {
        public string Name { get; set; } = string.Empty;
        public int Percentage { get; set; }
        public string Category { get; set; } = string.Empty; // "Primary" or "Secondary"
    }
}

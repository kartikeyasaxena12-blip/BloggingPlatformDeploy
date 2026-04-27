namespace PostService.Models
{
    public class SavedPost
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PostId { get; set; }
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Post Post { get; set; } = null!;
    }
}

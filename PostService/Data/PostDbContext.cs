using Microsoft.EntityFrameworkCore;
using PostService.Models;

namespace PostService.Data
{
    public class PostDbContext : DbContext
    {
        public PostDbContext(DbContextOptions<PostDbContext> options) : base(options)
        {
        }

        // Tables
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<SavedPost> SavedPosts { get; set; }
    }
}

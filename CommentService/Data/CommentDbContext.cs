using Microsoft.EntityFrameworkCore;
using CommentService.Models;

namespace CommentService.Data
{
    public class CommentDbContext : DbContext
    {
        public CommentDbContext(DbContextOptions<CommentDbContext> options) : base(options)
        {
        }

        // Tables
        public DbSet<Comment> Comments { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using NewsletterService.Models;

namespace NewsletterService.Data
{
    public class NewsletterDbContext : DbContext
    {
        public NewsletterDbContext(DbContextOptions<NewsletterDbContext> options) : base(options)
        {
        }

        public DbSet<Subscriber> Subscribers { get; set; }
    }
}

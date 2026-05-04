using ImageGallery.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace ImageGallery.Db
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Image> Images { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {

        }
    }
}

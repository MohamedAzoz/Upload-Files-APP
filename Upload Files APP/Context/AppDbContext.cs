using Microsoft.EntityFrameworkCore;
using Upload_Files_APP.Models;

namespace Upload_Files_APP.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options) { }
        
        public DbSet<UploadFile> uploadFiles { get; set; }

    }
}

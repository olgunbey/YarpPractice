using Microsoft.EntityFrameworkCore;
using YarpExample.Gateway.Entity;

namespace YarpExample.Gateway.Database
{
    public class YarpDbContext(DbContextOptions<YarpDbContext> dbContextOptions) : DbContext(dbContextOptions)
    {
        public DbSet<ClientCredentialTable> ClientCredentialTable { get; set; }
        public DbSet<Clients> Clients { get; set; }
    }
}

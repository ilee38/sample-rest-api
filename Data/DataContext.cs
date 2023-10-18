using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tweetbook.Domain;

namespace Tweetbook.Data
{
    public class DataContext : IdentityDbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// This represents a table in the database.
        /// </summary>
        public DbSet<Post> Posts { get; set; }



    }
}

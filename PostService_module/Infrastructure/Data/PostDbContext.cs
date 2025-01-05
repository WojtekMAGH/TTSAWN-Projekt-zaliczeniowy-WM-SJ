using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PostService_module.Core.Entities;


namespace PostService_module.Infrastructure.Data
{
    public class PostDbContext : DbContext
    {
        public PostDbContext(DbContextOptions<PostDbContext> options)
            : base(options) { }
        public DbSet<Post> Posts { get; set; }

    }
}

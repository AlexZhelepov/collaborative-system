using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace diploma.Data
{
    public class AppContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public static ApplicationDbContext DB
        {
            get
            {
                return new AppContextFactory().CreateDbContext(new string[] { });
            }
        }

        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=ddb;Username=postgres;Password=password");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}

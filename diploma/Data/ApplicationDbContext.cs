using System;
using System.Collections.Generic;
using System.Text;
using diploma.Data.Entities;
using diploma.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace diploma.Data
{
    // https://www.alfador.mx/software-development/configure-aspnet-core-2-mvc-identity-to-use-postgresql .
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .HasOne(n => n.UserInfo)
                .WithOne(n => n.User)
                .HasForeignKey<UserInfo>(n => n.UserId);
        }

        public DbSet<UserInfo> UserInfos { get; set; }
    }
}

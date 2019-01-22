

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Soyuan.Theme.Core.Helper;
using Soyuan.Theme.Domain.Entity;
using Soyuan.Theme.Domain.Extensions;
using System;
using System.IO;

namespace Soyuan.Theme.Domain
{
    public class ThemeDBContext : DbContext
    {
        public Soyuan.Theme.Core.Helper.JsonDatabase FileDB
        {
            get
            {
                var path = AppDomain.CurrentDomain.BaseDirectory + ConfigHelper.ReadConfigByName("FileDBPath");
                return new Core.Helper.JsonDatabase(path);
            }
        }

        public DbSet<ApplicationEntity> Application { get; set; }
        public DbSet<OrganizationEntity> Organization { get; set; }
        public DbSet<UserEntity> User { get; set; }
        public DbSet<TagEntity> Tag { get; set; }
        public DbSet<RoleEntity> Role { get; set; }
        public DbSet<UserRelateRoleEntity> UserRelateRole { get; set; }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();


            // define the database to use
            optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddEntityConfigurationsFromAssembly(GetType().Assembly);
        }
    }
}

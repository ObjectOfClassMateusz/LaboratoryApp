using LIMSApp.Models.Laboratory;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LIMSApp.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<LabSample> LaboratorySamples { get; set; }

        /*
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
        protected readonly IConfiguration _configuration;
        public AppDbContext(IConfiguration conf, DbContextOptions options) : base(options)
        {
            _configuration = conf;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("LIMS_Connection"));
        }*/
    }
}

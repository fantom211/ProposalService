using Microsoft.EntityFrameworkCore;
using ProposalService.Models.Entities;
using System.Reflection.Emit;

namespace ProposalService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Proposal> Proposals { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Proposal>()
                .HasIndex(p => new { p.TaskId, p.ExecutorId })
                .IsUnique();

            builder.Entity<Proposal>()
                .HasIndex(p => p.TaskId)
                .HasFilter("\"Status\" = 'accepted'")
                .IsUnique();
        }

    }
}

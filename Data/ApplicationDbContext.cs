using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShareCare.Models;

namespace ShareCare.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public DbSet<Group> Groups { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Group>()
                .HasOne(group => group.CreatorUser)
                .WithMany()
                .HasForeignKey(group => group.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Group>()
                .HasMany(group => group.Users)
                .WithMany(user => user.Groups);
        }

        public override int SaveChanges()
        {
            EnsureUniqueIds();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            EnsureUniqueIds();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void EnsureUniqueIds()
        {
            EnsureUniqueGroupIds();
        }

        private void EnsureUniqueGroupIds()
        {
            foreach (var entry in ChangeTracker.Entries<Group>().Where(e => e.State == EntityState.Added))
            {
                string newId;
                do
                {
                    newId = GenerateRandomId(6);
                }
                while (Groups.Any(g => g.Id == newId)); // Check DB for uniqueness

                entry.Entity.Id = newId;
            }
        }

        private static string GenerateRandomId(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

using Microsoft.EntityFrameworkCore;
using VitalEase.Server.Models;

namespace VitalEase.Server.Data
{
    public class VitalEaseServerContext : DbContext
    {
        public VitalEaseServerContext(DbContextOptions<VitalEaseServerContext> options)
         : base(options)
        {
        }

        // Add DbSets for your models
        public DbSet<User> Users { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<FavoriteLocation> FavoriteLocations { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<Routine> Routines { get; set; }
        public DbSet<ScheduledRoutine> ScheduledRoutines { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }



    }
}

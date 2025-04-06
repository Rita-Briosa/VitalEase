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
        public DbSet<Media> Media { get; set; }
        public DbSet<Routine> Routines { get; set; }
        public DbSet<ScheduledRoutine> ScheduledRoutines { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<ResetPasswordTokens> ResetPasswordTokens { get; set; }

        public DbSet<ResetEmailTokens> ResetEmailTokens { get; set; }

        public DbSet<DeleteAccountTokens> DeleteAccountTokens { get; set; }

        public DbSet<ExerciseRoutine> ExerciseRoutines { get; set; }

        public DbSet<ExerciseMedia> ExerciseMedia { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurando o relacionamento muitos-para-muitos entre Exercise e Routine usando a tabela de junção
            modelBuilder.Entity<ExerciseRoutine>()
                .HasKey(er => new { er.ExerciseId, er.RoutineId }); // Chave composta

            modelBuilder.Entity<ExerciseRoutine>()
                .HasOne(er => er.Exercise)
                .WithMany(e => e.ExerciseRoutine)
                .HasForeignKey(er => er.ExerciseId);

            modelBuilder.Entity<ExerciseRoutine>()
                .HasOne(er => er.Routine)
                .WithMany(r => r.ExerciseRoutine)
                .HasForeignKey(er => er.RoutineId);

            modelBuilder.Entity<ExerciseMedia>()
               .HasKey(er => new { er.ExerciseId, er.MediaId }); // Chave composta

            modelBuilder.Entity<ExerciseMedia>()
                .HasOne(er => er.Exercise)
                .WithMany(e => e.ExerciseMedia)
                .HasForeignKey(er => er.ExerciseId);

            modelBuilder.Entity<ExerciseMedia>()
                .HasOne(er => er.Media)
                .WithMany(r => r.ExerciseMedia)
                .HasForeignKey(er => er.MediaId);
        }

    }
}

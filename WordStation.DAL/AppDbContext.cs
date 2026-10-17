using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WordStation.EL.Models;

namespace WordStation.DAL
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {

        public AppDbContext(DbContextOptions options) : base(options)
        {

        }
        
        public DbSet<Word> Words { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<QuizHistory> QuizHistories { get; set; }

        // Study Plan
        public DbSet<StudyPlan> StudyPlans { get; set; }
        public DbSet<StudyPlanList> StudyPlanLists { get; set; }
        public DbSet<StudyPlanDay> StudyPlanDays { get; set; }
        public DbSet<StudyPlanDayWord> StudyPlanDayWords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<QuizHistory>(entity =>
            {
                entity.HasIndex(e => e.UserId);
            });

            modelBuilder.Entity<StudyPlan>(entity =>
            {
                entity.HasIndex(e => e.UserId);
            });

            modelBuilder.Entity<StudyPlanDayWord>(entity =>
            {
                entity.HasIndex(e => new { e.StudyPlanDayId, e.WordId }).IsUnique();
            });
        }
    }
}

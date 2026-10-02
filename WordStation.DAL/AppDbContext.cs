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
        public DbSet<DailyWordSession> DailyWordSessions { get; set; }
        public DbSet<DailyWordSessionItem> DailyWordSessionItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);



            modelBuilder.Entity<QuizHistory>(entity =>
            {
                entity.HasIndex(e => e.UserId);
            });

            modelBuilder.Entity<DailyWordSession>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.ListName }).IsUnique();
            });
        }
    }
}

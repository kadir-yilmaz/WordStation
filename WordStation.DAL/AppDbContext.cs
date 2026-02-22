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
        public DbSet<SynonymGroup> SynonymGroups { get; set; }
        public DbSet<SynonymWord> SynonymWords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // SynonymWord -> Word ilişkisi
            modelBuilder.Entity<SynonymWord>()
                .HasOne(sw => sw.Word)
                .WithMany(w => w.SynonymWords)
                .HasForeignKey(sw => sw.WordId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // SynonymWord -> SynonymGroup ilişkisi
            modelBuilder.Entity<SynonymWord>()
                .HasOne(sw => sw.SynonymGroup)
                .WithMany(sg => sg.SynonymWords)
                .HasForeignKey(sw => sw.SynonymGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}

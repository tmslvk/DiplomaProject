using System;
using System.Collections.Generic;
using DiplomaProject.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DiplomaProject
{
    public partial class DiplomaBDContext : DbContext
    {
        public DiplomaBDContext()
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Filling> Fillings { get; set; } = null!;
        public DbSet<Discipline> Disciplines { get; set; } = null!;
        public DbSet<Lesson> Lessons { get; set; } = null!;
        public DbSet<Group> Groups { get; set; } = null!;
        public DbSet<PlaceOfLesson> PlaceOfLessons { get; set; } = null!;
        public DbSet<BackupLessons> BackupLessons { get; set; } = null!;

        public DiplomaBDContext(DbContextOptions<DiplomaBDContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=DESKTOP-1D3P714\\SQLEXPRESS;Database=DiplomaBD;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //User
            modelBuilder.Entity<User>().HasKey(u => u.Id);

            //Fillings
            modelBuilder.Entity<Filling>().HasKey(u => u.Id);
            modelBuilder.Entity<Filling>().HasOne(f => f.User).WithMany(s => s.Filling).HasForeignKey(f => f.UserId);

            //Discipline
            modelBuilder.Entity<Discipline>().HasKey(u => u.Id);
            modelBuilder.Entity<Discipline>().HasOne(d => d.User).WithMany(u => u.Discipline).HasForeignKey(d => d.UserId);

            //Group
            modelBuilder.Entity<Group>().HasKey(u => u.Id);
            modelBuilder.Entity<Group>().HasOne(g => g.User).WithMany(u => u.Group).HasForeignKey(g => g.UserId);

            //PlaceOfLesson
            modelBuilder.Entity<PlaceOfLesson>().HasKey(p => p.Id);
            modelBuilder.Entity<PlaceOfLesson>().HasOne(p => p.User).WithMany(u => u.PlaceOfLesson).HasForeignKey(p => p.UserId);

            //Lesson
            modelBuilder.Entity<Lesson>().HasKey(l => l.Id);
            modelBuilder.Entity<Lesson>().HasOne(l => l.User).WithMany(u => u.Lesson).HasForeignKey(l => l.UserId);

            //BackupLessons
            modelBuilder.Entity<BackupLessons>().HasKey(u => u.Id);
            modelBuilder.Entity<BackupLessons>().HasOne(bl => bl.Lesson).WithMany(l => l.BackupLessons).HasForeignKey(bl => bl.LessonId);
        }
    }
}

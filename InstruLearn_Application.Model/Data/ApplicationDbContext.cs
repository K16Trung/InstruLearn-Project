using InstruLearn_Application.Model.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Learner> Learners { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Course_Content> Course_Contents { get; set; }
        public DbSet<Course_Content_Item> Course_Content_Items { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Learner>()
                .HasOne(a => a.Account)
                .WithOne()
                .HasForeignKey<Learner>(u => u.AccountId);

            modelBuilder.Entity<Admin>()
                .HasOne(a => a.Account)
                .WithOne()
                .HasForeignKey<Admin>(a => a.AccountId);
            
            modelBuilder.Entity<Staff>()
                .HasOne(s => s.Account)
                .WithOne()
                .HasForeignKey<Staff>(s => s.AccountId);
            
            modelBuilder.Entity<Teacher>()
                .HasOne(s => s.Account)
                .WithOne()
                .HasForeignKey<Teacher>(s => s.AccountId);
            
            modelBuilder.Entity<Manager>()
                .HasOne(s => s.Account)
                .WithOne()
                .HasForeignKey<Manager>(s => s.AccountId);

            modelBuilder.Entity<Course>()
                .Property(c => c.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Type)
                .WithMany(ct => ct.Courses)
                .HasForeignKey(c => c.TypeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Course>()
                .HasMany(c => c.CourseContents)
                .WithOne(cc => cc.Course)
                .HasForeignKey(cc => cc.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Course_Content>()
                .HasMany(cc => cc.CourseContentItems)
                .WithOne(ci => ci.CourseContent)
                .HasForeignKey(ci => ci.ContentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Course_Content_Item>()
                .HasOne(c => c.ItemType)
                .WithMany(ct => ct.CourseContentItems)
                .HasForeignKey(c => c.ItemTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

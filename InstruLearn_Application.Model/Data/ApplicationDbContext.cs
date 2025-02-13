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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Learner)
                .WithOne(u => u.Account)
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
        }
    }
}

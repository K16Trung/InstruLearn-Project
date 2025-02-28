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
        public DbSet<FeedBack> FeedBacks { get; set; }
        public DbSet<FeedbackReplies> FeedbackReplies { get; set; }
        public DbSet<QnA> QnA { get; set; }
        public DbSet<QnAReplies> QnAReplies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Admin)
                .WithOne(a => a.Account)
                .HasForeignKey<Admin>(a => a.AccountId);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Learner)
                .WithOne(a => a.Account)
                .HasForeignKey<Learner>(a => a.AccountId);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Staff)
                .WithOne(a => a.Account)
                .HasForeignKey<Staff>(a => a.AccountId);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Teacher)
                .WithOne(a => a.Account)
                .HasForeignKey<Teacher>(a => a.AccountId);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Manager)
                .WithOne(a => a.Account)
                .HasForeignKey<Manager>(a => a.AccountId);

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

            modelBuilder.Entity<FeedBack>()
                .HasOne(f => f.Course)
                .WithMany(c => c.FeedBacks)
                .HasForeignKey(f => f.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FeedBack>()
                .HasOne(f => f.Account)
                .WithMany(a => a.FeedBacks)
                .HasForeignKey(f => f.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FeedbackReplies>()
                .HasOne(fr => fr.FeedBack)
                .WithMany(f => f.FeedbackReplies)
                .HasForeignKey(fr => fr.FeedbackId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FeedbackReplies>()
                .HasOne(fr => fr.Account)
                .WithMany(a => a.FeedbackReplies)
                .HasForeignKey(fr => fr.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QnA>()
                .HasOne(q => q.Account)
                .WithMany(a => a.QnAs)
                .HasForeignKey(q => q.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QnA>()
                .HasOne(q => q.Course)
                .WithMany(c => c.QnAs)
                .HasForeignKey(q => q.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QnAReplies>()
                .HasOne(qr => qr.QnA)
                .WithMany(q => q.QnAReplies)
                .HasForeignKey(qr => qr.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QnAReplies>()
                .HasOne(qr => qr.Account)
                .WithMany(a => a.QnAReplies)
                .HasForeignKey(qr => qr.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

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
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<ClassDay> ClassDays { get; set; }
        public DbSet<Center_Course> Center_Courses { get; set; }
        public DbSet<Curriculum> Curriculums { get; set; }
        public DbSet<OneOnOneRequest> OneOnOneRequests { get; set; }
        public DbSet<OneOnOneRequestTests> OneOnOneRequestTests { get; set; }
        public DbSet<OneOnOneRequestDays> OneOnOneRequestDays { get; set; }
        public DbSet<OneOnOneSchedules> OneOnOneSchedules { get; set; }
        public DbSet<Major> Majors { get; set; }
        public DbSet<MajorTest> MajorTests { get; set; }

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

            modelBuilder.Entity<Wallet>()
                .HasOne(w => w.Learner)
                .WithOne(l => l.Wallet)
                .HasForeignKey<Wallet>(w => w.LearnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WalletTransaction>()
                .HasOne(wt => wt.Wallet)
                .WithMany(w => w.WalletTransactions)
                .HasForeignKey(wt => wt.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Wallet)
                .WithMany(w => w.Payments)
                .HasForeignKey(p => p.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.WalletTransaction)
                .WithMany(wt => wt.Payments)
                .HasForeignKey(p => p.WalletTransactionId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Class>()
                .HasOne(c => c.CenterCourse)    
                .WithMany(cc => cc.Classes)
                .HasForeignKey(c => c.CenterCourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Class>()
                .HasOne(c => c.Curriculum)
                .WithMany(cu => cu.Classes)
                .HasForeignKey(c => c.CuriculumId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Class>()
                .HasOne(c => c.Teacher)
                .WithMany(t => t.Classes)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Curriculum>()
                .HasOne(cu => cu.CenterCourse)
                .WithMany(cc => cc.Curriculums)
                .HasForeignKey(cu => cu.CenterCourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClassDay>()
                .HasOne(cd => cd.Class)
                .WithMany(c => c.ClassDays)
                .HasForeignKey(cd => cd.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Class>()
                .Property(c => c.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Major>()
                .HasMany(m => m.MajorTests)
                .WithOne(mt => mt.Major)
                .HasForeignKey(mt => mt.MajorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Major>()
                .HasMany(m => m.OneOnOneRequests)
                .WithOne(o => o.Major)
                .HasForeignKey(o => o.MajorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Teacher>()
                .HasMany(t => t.OneOnOneRequests)
                .WithOne(o => o.Teacher)
                .HasForeignKey(o => o.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Learner>()
                .HasMany(l => l.OneOnOneRequests)
                .WithOne(o => o.Learner)
                .HasForeignKey(o => o.LearnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OneOnOneRequest>()
                .HasMany(o => o.OneOnOneRequestDays)
                .WithOne(d => d.OneOnOneRequest)
                .HasForeignKey(d => d.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OneOnOneRequest>()
                .HasMany(o => o.OneOnOneSchedules)
                .WithOne(s => s.OneOnOneRequest)
                .HasForeignKey(s => s.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OneOnOneRequest>()
                .HasMany(o => o.OneOnOneRequestTests)
                .WithOne(rt => rt.OneOnOneRequest)
                .HasForeignKey(rt => rt.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MajorTest>()
                .HasMany(mt => mt.OneOnOneRequestTests)
                .WithOne(rt => rt.MajorTest)
                .HasForeignKey(rt => rt.TestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Teacher>()
                .HasMany(t => t.OneOnOneSchedules)
                .WithOne(s => s.Teacher)
                .HasForeignKey(s => s.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Learner>()
                .HasMany(l => l.OneOnOneSchedules)
                .WithOne(s => s.Learner)
                .HasForeignKey(s => s.LearnerId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}

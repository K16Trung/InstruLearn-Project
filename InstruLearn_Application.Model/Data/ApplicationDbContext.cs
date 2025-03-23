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
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Course_Package> CoursePackages { get; set; }
        public DbSet<Course_Content> Course_Contents { get; set; }
        public DbSet<CourseType> CourseTypes { get; set; }
        public DbSet<Course_Content_Item> Course_Content_Items { get; set; }
        public DbSet<ItemTypes> ItemTypes { get; set; }
        public DbSet<FeedBack> FeedBacks { get; set; }
        public DbSet<FeedbackReplies> FeedbackReplies { get; set; }
        public DbSet<QnA> QnA { get; set; }
        public DbSet<QnAReplies> QnAReplies { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<ClassDay> ClassDays { get; set; }
        public DbSet<Major> Majors { get; set; }
        public DbSet<MajorTest> MajorTests { get; set; }
        public DbSet<Learning_Registration> Learning_Registrations { get; set; }
        public DbSet<Learning_Registration_Type> Learning_Registration_Types { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<LearningRegistrationDay> LearningRegistrationDays { get; set; }
        public DbSet<Syllabus> Syllabus { get; set; }
        public DbSet<Purchase_Items> Purchase_Items { get; set; }
        public DbSet<Test_Result> Test_Results { get; set; }
        public DbSet<Schedules> Schedules { get; set; }
        public DbSet<ScheduleDays> ScheduleDays { get; set; }
        public DbSet<Certification> Certifications { get; set; }
        public DbSet<TeacherMajor> TeacherMajors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Admin)
                .WithOne(a => a.Account)
                .HasForeignKey<Admin>(a => a.AccountId);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Learner)
                .WithOne(a => a.Account)
                .HasForeignKey<Learner>(a => a.AccountId);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Teacher)
                .WithOne(a => a.Account)
                .HasForeignKey<Teacher>(a => a.AccountId);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Staff)
                .WithOne(a => a.Account)
                .HasForeignKey<Staff>(a => a.AccountId);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Manager)
                .WithOne(a => a.Account)
                .HasForeignKey<Manager>(a => a.AccountId);

            //

            modelBuilder.Entity<Course_Package>()
                .Property(c => c.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Course_Package>()
                .HasOne(c => c.Type)
                .WithMany(ct => ct.CoursePackages)
                .HasForeignKey(c => c.CourseTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Course_Package>()
                .HasMany(c => c.Classes)
                .WithOne(c => c.CoursePackage)
                .HasForeignKey(c => c.CoursePackageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Course_Package>()
                .HasMany(c => c.CourseContents)
                .WithOne(cc => cc.CoursePackage)
                .HasForeignKey(cc => cc.CoursePackageId)
                .OnDelete(DeleteBehavior.Cascade);

            //

            modelBuilder.Entity<Course_Content>()
                .HasMany(cc => cc.CourseContentItems)
                .WithOne(ci => ci.CourseContent)
                .HasForeignKey(ci => ci.ContentId)
                .OnDelete(DeleteBehavior.Cascade);

            //

            modelBuilder.Entity<Course_Content_Item>()
                .HasOne(c => c.ItemType)
                .WithMany(ct => ct.CourseContentItems)
                .HasForeignKey(c => c.ItemTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            //

            modelBuilder.Entity<FeedBack>()
                .HasOne(f => f.CoursePackage)
                .WithMany(c => c.FeedBacks)
                .HasForeignKey(f => f.CoursePackageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FeedBack>()
                .HasOne(f => f.Account)
                .WithMany(a => a.FeedBacks)
                .HasForeignKey(f => f.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            //

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

            //

            modelBuilder.Entity<QnA>()
                .HasOne(q => q.Account)
                .WithMany(a => a.QnAs)
                .HasForeignKey(q => q.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QnA>()
                .HasOne(q => q.CoursePackage)
                .WithMany(c => c.QnAs)
                .HasForeignKey(q => q.CoursePackageId)
                .OnDelete(DeleteBehavior.Cascade);

            //

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

            //

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

            //

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Wallet)
                .WithMany(w => w.Payments)
                .HasForeignKey(p => p.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.WalletTransaction)
                .WithMany(wt => wt.Payments)
                .HasForeignKey(p => p.TransactionId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            //

            modelBuilder.Entity<Class>()
                .HasOne(c => c.Teacher)
                .WithMany(t => t.Classes)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClassDay>()
                .HasOne(cd => cd.Class)
                .WithMany(c => c.ClassDays)
                .HasForeignKey(cd => cd.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Class>()
                .HasOne(c => c.Syllabus)
                .WithMany(s => s.Classes)
                .HasForeignKey(c => c.SyllabusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Class>()
                .Property(c => c.Price)
                .HasColumnType("decimal(18,2)");

            //

            modelBuilder.Entity<Major>()
                .HasMany(m => m.MajorTests)
                .WithOne(mt => mt.Major)
                .HasForeignKey(mt => mt.MajorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Learning_Registration Entity

            modelBuilder.Entity<Learning_Registration>()
                .HasOne(l => l.Learner)
                .WithMany(lr => lr.Learning_Registrations)
                .HasForeignKey(l => l.LearnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Learning_Registration>()
                .HasOne(lr => lr.Teacher)
                .WithMany(t => t.Learning_Registrations)
                .HasForeignKey(lr => lr.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Learning_Registration>()
                .HasOne(lr => lr.Classes)
                .WithMany(c => c.Learning_Registration)
                .HasForeignKey(lr => lr.ClassId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Learning_Registration>()
                .HasOne(l => l.Major)
                .WithMany(l => l.learning_Registrations)
                .HasForeignKey(l => l.MajorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Learning_Registration_Type Entity

            modelBuilder.Entity<Learning_Registration_Type>()
                .HasMany(l => l.Learning_Registrations)
                .WithOne(lrt => lrt.Learning_Registration_Type)
                .HasForeignKey(lrt => lrt.RegisTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Purchase Entity

            modelBuilder.Entity<Purchase>()
                .HasOne(l => l.Learner)
                .WithMany(lr => lr.Purchases)
                .HasForeignKey(l => l.LearnerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Purchase_Items Entity

            modelBuilder.Entity<Purchase_Items>()
                .HasOne(pi => pi.Purchase)
                .WithMany(p => p.PurchaseItems)
                .HasForeignKey(pi => pi.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Purchase_Items>()
                .HasOne(pi => pi.CoursePackage)
                .WithMany(cp => cp.PurchaseItems)
                .HasForeignKey(pi => pi.CoursePackageId)
                .OnDelete(DeleteBehavior.Cascade);

            //

            modelBuilder.Entity<TeacherMajor>()
                .Property(t => t.TeacherMajorId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<TeacherMajor>()
                .HasKey(tm => new { tm.TeacherId, tm.MajorId });

            modelBuilder.Entity<TeacherMajor>()
                .HasOne(tm => tm.Teacher)
                .WithMany(t => t.TeacherMajors)
                .HasForeignKey(tm => tm.TeacherId);

            modelBuilder.Entity<TeacherMajor>()
                .HasOne(tm => tm.Major)
                .WithMany(m => m.TeacherMajors)
                .HasForeignKey(tm => tm.MajorId);

            //

            modelBuilder.Entity<LearningRegistrationDay>()
                .HasOne(r => r.Learning_Registration)
                .WithMany(l => l.LearningRegistrationDay)
                .HasForeignKey(l => l.LearningRegisId)
                .OnDelete(DeleteBehavior.Restrict);

            //

            modelBuilder.Entity<Test_Result>()
                .HasOne(tr => tr.Major)
                .WithMany(mt => mt.TestResults)
                .HasForeignKey(tr => tr.MajorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Test_Result>()
                .HasOne(tr => tr.Learner)
                .WithMany(l => l.Test_Results)
                .HasForeignKey(tr => tr.LearnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Test_Result>()
                .HasOne(tr => tr.Teacher)
                .WithMany(t => t.TestResults)
                .HasForeignKey(tr => tr.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Test_Result>()
                .HasOne(tr => tr.LearningRegistration)
                .WithMany(lr => lr.Test_Results)
                .HasForeignKey(tr => tr.LearningRegisId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Schedules Entity
            modelBuilder.Entity<Schedules>()
                .HasKey(s => s.ScheduleId);

            modelBuilder.Entity<Schedules>()
                .HasOne(s => s.Teacher)
                .WithMany(t => t.Schedules)
                .HasForeignKey(s => s.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Schedules>()
                .HasOne(s => s.Learner)
                .WithMany(l => l.Schedules)
                .HasForeignKey(s => s.LearnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Schedules>()
                .HasOne(s => s.Registration)
                .WithMany(r => r.Schedules)
                .HasForeignKey(s => s.LearningRegisId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ScheduleDays Entity
            modelBuilder.Entity<ScheduleDays>()
                .HasKey(sd => sd.ScheduleDayId);

            modelBuilder.Entity<ScheduleDays>()
                .HasOne(sd => sd.Schedules)
                .WithMany(s => s.ScheduleDays)
                .HasForeignKey(sd => sd.ScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Enum Conversions
            modelBuilder.Entity<Schedules>()
                .Property(s => s.Mode)
                .HasConversion<string>();

            modelBuilder.Entity<ScheduleDays>()
                .Property(sd => sd.DayOfWeeks)
                .HasConversion<string>();
            //
            modelBuilder.Entity<Certification>()
                .HasOne(c => c.Learner)
                .WithMany(l => l.Certifications)
                .HasForeignKey(c => c.LearnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Certification>()
               .HasOne(c => c.CoursePackages)
               .WithOne(cp => cp.Certifications)
               .HasForeignKey<Certification>(c => c.CoursePackageId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

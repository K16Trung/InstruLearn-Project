﻿using InstruLearn_Application.Model.Models;
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
        public DbSet<Purchase_Items> Purchase_Items { get; set; }
        public DbSet<Schedules> Schedules { get; set; }
        public DbSet<ScheduleDays> ScheduleDays { get; set; }
        public DbSet<Certification> Certifications { get; set; }
        public DbSet<TeacherMajor> TeacherMajors { get; set; }
        public DbSet<LevelAssigned> LevelAssigneds { get; set; }
        public DbSet<Response> Responses { get; set; }
        public DbSet<ResponseType> ResponseTypes { get; set; }
        public DbSet<Learner_class> Learner_Classes { get; set; }
        public DbSet<LearningPathSession> LearningPathSessions { get; set; }
        public DbSet<Learner_Course> LearnerCourses { get; set; }
        public DbSet<Learner_Content_Progress> LearnerContentProgresses { get; set; }
        public DbSet<LearningRegisFeedbackQuestion> LearningRegisFeedbackQuestions { get; set; }
        public DbSet<LearningRegisFeedbackOption> LearningRegisFeedbackOptions { get; set; }
        public DbSet<LearningRegisFeedback> LearningRegisFeedbacks { get; set; }
        public DbSet<LearningRegisFeedbackAnswer> LearningRegisFeedbackAnswers { get; set; }
        public DbSet<StaffNotification> StaffNotifications { get; set; }
        public DbSet<TeacherEvaluationFeedback> TeacherEvaluationFeedbacks { get; set; }
        public DbSet<TeacherEvaluationQuestion> TeacherEvaluationQuestions { get; set; }
        public DbSet<TeacherEvaluationOption> TeacherEvaluationOptions { get; set; }
        public DbSet<TeacherEvaluationAnswer> TeacherEvaluationAnswers { get; set; }
        public DbSet<LevelFeedbackTemplate> LevelFeedbackTemplates { get; set; }
        public DbSet<LevelFeedbackCriterion> LevelFeedbackCriteria { get; set; }
        public DbSet<ClassFeedback> ClassFeedbacks { get; set; }
        public DbSet<ClassFeedbackEvaluation> ClassFeedbackEvaluations { get; set; }
        public DbSet<SelfAssessment> SelfAssessments { get; set; }
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }

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
                .Property(c => c.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Class>()
                .HasOne(c => c.Major)
                .WithMany(m => m.Classes)
                .HasForeignKey(c => c.MajorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Class>()
                .HasOne(c => c.Level)
                .WithMany()
                .HasForeignKey(c => c.LevelId)
                .OnDelete(DeleteBehavior.Restrict);

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

            modelBuilder.Entity<Learning_Registration>()
                .HasOne(l => l.Response)
                .WithMany(r => r.Learning_Registrations)
                .HasForeignKey(l => l.ResponseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Learning_Registration>()
                .HasOne(l => l.LevelAssigned)
                .WithMany(la => la.Learning_Registration)
                .HasForeignKey(l => l.LevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Learning_Registration_Type Entity

            modelBuilder.Entity<Learning_Registration_Type>()
                .HasMany(l => l.Learning_Registrations)
                .WithOne(lrt => lrt.Learning_Registration_Type)
                .HasForeignKey(lrt => lrt.RegisTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Learning_Registration_Type>().HasData(
                new Learning_Registration_Type
                {
                    RegisTypeId = Learning_Registration_Type.CustomLearningTypeId,
                    RegisTypeName = Learning_Registration_Type.CustomLearningTypeName,
                    RegisPrice = 0.00m
                },
                new Learning_Registration_Type
                {
                    RegisTypeId = Learning_Registration_Type.CenterLearningTypeId,
                    RegisTypeName = Learning_Registration_Type.CenterLearningTypeName,
                    RegisPrice = 0.00m
                }
            );

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

            modelBuilder.Entity<Schedules>()
                .HasOne(c => c.Class)
                .WithMany(s => s.Schedules)
                .HasForeignKey(s => s.ClassId)
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
               .HasOne(c => c.Learner)
               .WithMany(l => l.Certifications)
               .HasForeignKey(c => c.LearnerId)
               .OnDelete(DeleteBehavior.Cascade);

            //
            modelBuilder.Entity<LevelAssigned>()
               .HasOne(la => la.Major)
               .WithMany(m => m.LevelAssigneds)
               .HasForeignKey(la => la.MajorId)
               .OnDelete(DeleteBehavior.Restrict);

            // Response Entity
            modelBuilder.Entity<Response>()
                .HasOne(r => r.ResponseType)
                .WithMany(rt => rt.Responses)
                .HasForeignKey(r => r.ResponseTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Learner_class
            modelBuilder.Entity<Learner_class>()
                .HasOne(l => l.Learner)
                .WithMany(lc => lc.Learner_Classes)
                .HasForeignKey(l => l.LearnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Learner_class>()
                .HasOne(C => C.Classes)
                .WithMany(lc => lc.Learner_Classes)
                .HasForeignKey(C => C.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            // LearningPathSession
            modelBuilder.Entity<LearningPathSession>()
                .HasOne(lps => lps.LearningRegistration)
                .WithMany(lr => lr.LearningPathSessions)
                .HasForeignKey(lps => lps.LearningRegisId)
                .OnDelete(DeleteBehavior.Cascade);

            //
            modelBuilder.Entity<Learner_Course>()
                .HasOne(lc => lc.Learner)
                .WithMany(l => l.LearnerCourses)
                .HasForeignKey(lc => lc.LearnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Learner_Course>()
                .HasOne(lc => lc.CoursePackage)
                .WithMany(cp => cp.LearnerCourses)
                .HasForeignKey(lc => lc.CoursePackageId)
                .OnDelete(DeleteBehavior.Cascade);

            //
            modelBuilder.Entity<Learner_Content_Progress>()
                .HasOne(lcp => lcp.Learner)
                .WithMany(l => l.ContentProgresses)
                .HasForeignKey(lcp => lcp.LearnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Learner_Content_Progress>()
                .HasOne(lcp => lcp.ContentItem)
                .WithMany(ci => ci.LearnerProgresses)
                .HasForeignKey(lcp => lcp.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure LearningRegisFeedbackQuestion entity
            modelBuilder.Entity<LearningRegisFeedbackQuestion>()
                .HasKey(q => q.QuestionId);

            modelBuilder.Entity<LearningRegisFeedbackQuestion>()
                .HasMany(q => q.Options)
                .WithOne(o => o.Question)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure LearningRegisFeedbackOption entity
            modelBuilder.Entity<LearningRegisFeedbackOption>()
                .HasKey(o => o.OptionId);

            // Configure LearningRegisFeedback entity
            modelBuilder.Entity<LearningRegisFeedback>()
                .HasKey(f => f.FeedbackId);

            modelBuilder.Entity<LearningRegisFeedback>()
                .HasOne(f => f.Learner)
                .WithMany()
                .HasForeignKey(f => f.LearnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LearningRegisFeedback>()
                .HasOne(f => f.LearningRegistration)
                .WithMany()
                .HasForeignKey(f => f.LearningRegistrationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LearningRegisFeedback>()
                .HasMany(f => f.Answers)
                .WithOne(a => a.Feedback)
                .HasForeignKey(a => a.FeedbackId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LearningRegisFeedback>()
                .Property(f => f.Status)
                .HasConversion<string>();

            // Configure LearningRegisFeedbackAnswer entity
            modelBuilder.Entity<LearningRegisFeedbackAnswer>()
                .HasKey(a => a.AnswerId);

            modelBuilder.Entity<LearningRegisFeedbackAnswer>()
                .HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LearningRegisFeedbackAnswer>()
                .HasOne(a => a.SelectedOption)
                .WithMany()
                .HasForeignKey(a => a.SelectedOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure StaffNotification entity
            modelBuilder.Entity<StaffNotification>()
                .HasKey(n => n.NotificationId);

            modelBuilder.Entity<StaffNotification>()
                .HasOne(n => n.LearningRegistration)
                .WithMany()
                .HasForeignKey(n => n.LearningRegisId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StaffNotification>()
                .HasOne(n => n.Learner)
                .WithMany()
                .HasForeignKey(n => n.LearnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StaffNotification>()
                .Property(n => n.Status)
                .HasConversion<string>();

            modelBuilder.Entity<StaffNotification>()
                .Property(n => n.Type)
                .HasConversion<string>();

            // Configure TeacherEvaluationQuestion entity
            modelBuilder.Entity<TeacherEvaluationQuestion>()
                .HasKey(q => q.EvaluationQuestionId);

            modelBuilder.Entity<TeacherEvaluationQuestion>()
                .HasMany(q => q.Options)
                .WithOne(o => o.Question)
                .HasForeignKey(o => o.EvaluationQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure TeacherEvaluationOption entity
            modelBuilder.Entity<TeacherEvaluationOption>()
                .HasKey(o => o.EvaluationOptionId);

            // Configure TeacherEvaluationFeedback entity
            modelBuilder.Entity<TeacherEvaluationFeedback>()
                .HasKey(f => f.EvaluationFeedbackId);

            modelBuilder.Entity<TeacherEvaluationFeedback>()
                .HasOne(f => f.Learner)
                .WithMany()
                .HasForeignKey(f => f.LearnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherEvaluationFeedback>()
                .HasOne(f => f.Teacher)
                .WithMany()
                .HasForeignKey(f => f.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherEvaluationFeedback>()
                .HasOne(f => f.LearningRegistration)
                .WithMany()
                .HasForeignKey(f => f.LearningRegistrationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeacherEvaluationFeedback>()
                .HasMany(f => f.Answers)
                .WithOne(a => a.Feedback)
                .HasForeignKey(a => a.EvaluationFeedbackId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeacherEvaluationFeedback>()
                .Property(f => f.Status)
                .HasConversion<string>();

            // Configure TeacherEvaluationAnswer entity
            modelBuilder.Entity<TeacherEvaluationAnswer>()
                .HasKey(a => a.EvaluationAnswerId);

            modelBuilder.Entity<TeacherEvaluationAnswer>()
                .HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.EvaluationQuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherEvaluationAnswer>()
                .HasOne(a => a.SelectedOption)
                .WithMany()
                .HasForeignKey(a => a.SelectedOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure LevelFeedbackTemplate entity
            modelBuilder.Entity<LevelFeedbackTemplate>()
                .HasKey(t => t.TemplateId);

            modelBuilder.Entity<LevelFeedbackTemplate>()
                .HasOne(t => t.Level)
                .WithMany()
                .HasForeignKey(t => t.LevelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LevelFeedbackTemplate>()
                .HasMany(t => t.Criteria)
                .WithOne(c => c.Template)
                .HasForeignKey(c => c.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure LevelFeedbackCriterion entity
            modelBuilder.Entity<LevelFeedbackCriterion>()
                .HasKey(c => c.CriterionId);

            modelBuilder.Entity<LevelFeedbackCriterion>()
                .Property(c => c.Weight)
                .HasColumnType("decimal(5,2)");

            // Configure ClassFeedback entity
            modelBuilder.Entity<ClassFeedback>()
                .HasKey(f => f.FeedbackId);

            modelBuilder.Entity<ClassFeedback>()
                .HasOne(f => f.Class)
                .WithMany()
                .HasForeignKey(f => f.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClassFeedback>()
                .HasOne(f => f.Learner)
                .WithMany()
                .HasForeignKey(f => f.LearnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClassFeedback>()
                .HasOne(f => f.Template)
                .WithMany()
                .HasForeignKey(f => f.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClassFeedback>()
                .HasMany(f => f.Evaluations)
                .WithOne(e => e.Feedback)
                .HasForeignKey(e => e.FeedbackId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ClassFeedbackEvaluation entity
            modelBuilder.Entity<ClassFeedbackEvaluation>()
                .HasKey(e => e.EvaluationId);

            modelBuilder.Entity<ClassFeedbackEvaluation>()
                .HasOne(e => e.Criterion)
                .WithMany(c => c.Evaluations)
                .HasForeignKey(e => e.CriterionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClassFeedbackEvaluation>()
                .Property(e => e.AchievedPercentage)
                .HasColumnType("decimal(5,2)");

            // Configure SelfAssessment entity
            modelBuilder.Entity<SelfAssessment>()
                .HasMany(s => s.LearningRegistrations)
                .WithOne(lr => lr.SelfAssessment)
                .HasForeignKey(lr => lr.SelfAssessmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

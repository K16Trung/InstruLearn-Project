using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.UoW.IUoW
{
    public interface IUnitOfWork
    {
        ITeacherRepository TeacherRepository { get; }
        IAccountRepository AccountRepository { get; }
        ILearnerRepository LearnerRepository { get; }
        IAdminRepository AdminRepository { get; }
        IStaffRepository StaffRepository { get; }
        IManagerRepository ManagerRepository { get; }
        ICourseRepository CourseRepository { get; }
        ICourseTypeRepository CourseTypeRepository { get; }
        ICourseContentRepository CourseContentRepository { get; }
        IItemTypeRepository ItemTypeRepository { get; }
        ICourseContentItemRepository CourseContentItemRepository { get; }
        IWalletRepository WalletRepository { get; }
        IPaymentRepository PaymentsRepository { get; }
        IWalletTransactionRepository WalletTransactionRepository { get; }
        IFeedbackRepository FeedbackRepository { get; }
        IFeedbackRepliesRepository FeedbackRepliesRepository { get; }
        IQnARepository QnARepository { get; }
        IQnARepliesRepository QnARepliesRepository { get; }
        IClassRepository ClassRepository { get; }
        IClassDayRepository ClassDayRepository { get; }
        IMajorRepository MajorRepository { get; }
        IMajorTestRepository MajorTestRepository { get; }
        ILearningRegisRepository LearningRegisRepository { get; }
        ILearningRegisTypeRepository LearningRegisTypeRepository { get; }
        ILearningRegisDayRepository LearningRegisDayRepository { get; }
        ISyllabusRepository SyllabusRepository { get; }
        IPurchaseRepository PurchaseRepository { get; }
        IPurchaseItemRepository PurchaseItemRepository { get; }
        ICertificationRepository CertificationRepository { get; }
        IScheduleRepository ScheduleRepository { get; }
        ITeacherMajorRepository TeacherMajorRepository { get; }
        ILevelAssignedRepository LevelAssignedRepository { get; }
        IResponseRepository ResponseRepository { get; }
        IResponseTypeRepository ResponseTypeRepository { get; }
        ISyllabusContentRepository SyllabusContentRepository { get; }
        ILearningPathSessionRepository LearningPathSessionRepository { get; }
        ILearnerCourseRepository LearnerCourseRepository { get; }
        ILearnerContentProgressRepository LearnerContentProgressRepository { get; }
        ILearningRegisFeedbackAnswerRepository LearningRegisFeedbackAnswerRepository { get; }
        ILearningRegisFeedbackOptionRepository LearningRegisFeedbackOptionRepository { get; }
        ILearningRegisFeedbackQuestionRepository LearningRegisFeedbackQuestionRepository { get; }
        ILearningRegisFeedbackRepository LearningRegisFeedbackRepository { get; }
        IStaffNotificationRepository StaffNotificationRepository { get; }
        ITeacherEvaluationRepository TeacherEvaluationRepository { get; }
        ILevelFeedbackTemplateRepository LevelFeedbackTemplateRepository { get; }
        ILevelFeedbackCriterionRepository LevelFeedbackCriterionRepository { get; }
        IClassFeedbackRepository ClassFeedbackRepository { get; }
        IClassFeedbackEvaluationRepository ClassFeedbackEvaluationRepository { get; }
        ApplicationDbContext dbContext { get; }
        public Task<int> SaveChangeAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}

﻿using InstruLearn_Application.DAL.Repository;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.UoW
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly IManagerRepository _managerRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly ILearnerRepository _learnerRepository;
        private readonly ITeacherRepository _teacherRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseTypeRepository _courseTypeRepository;
        private readonly ICourseContentRepository _courseContentRepository;
        private readonly IItemTypeRepository _itemTypeRepository;
        private readonly ICourseContentItemRepository _courseContentItemRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IFeedbackRepliesRepository _feedbackRepliesRepository;
        private readonly IQnARepository _qnARepository;
        private readonly IQnARepliesRepository _qnARepliesRepository;
        private readonly IClassRepository _classRepository;
        private readonly IClassDayRepository _classDayRepository;
        private readonly IMajorRepository _majorRepository;
        private readonly IMajorTestRepository _majorTestRepository;
        private readonly ILearningRegisRepository _learningRegisRepository;
        private readonly ILearningRegisTypeRepository _learningRegisTypeRepository;
        private readonly ILearningRegisDayRepository _learningRegisDayRepository;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IPurchaseItemRepository _purchaseItemRepository;
        private readonly ICertificationRepository _certificationRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly ITeacherMajorRepository _teacherMajorRepository;
        private readonly ILevelAssignedRepository _levelAssignedRepository;
        private readonly IResponseRepository _responseRepository;
        private readonly IResponseTypeRepository _responseTypeRepository;
        private readonly ILearnerCourseRepository _learnerCourseRepository;
        private readonly ILearnerContentProgressRepository _learnerContentProgressRepository;
        private readonly ILearningPathSessionRepository _learningPathSessionRepository;
        private readonly ILearningRegisFeedbackAnswerRepository _learningRegisFeedbackAnswerRepository;
        private readonly ILearningRegisFeedbackOptionRepository _learningRegisFeedbackOptionRepository;
        private readonly ILearningRegisFeedbackQuestionRepository _learningRegisFeedbackQuestionRepository;
        private readonly ILearningRegisFeedbackRepository _learningRegisFeedbackRepository;
        private readonly IStaffNotificationRepository _staffNotificationRepository;
        private readonly ITeacherEvaluationRepository _teacherEvaluationRepository;
        private readonly ILevelFeedbackTemplateRepository _levelFeedbackTemplateRepository;
        private readonly ILevelFeedbackCriterionRepository _levelFeedbackCriterionRepository;
        private readonly IClassFeedbackRepository _classFeedbackRepository;
        private readonly IClassFeedbackEvaluationRepository _classFeedbackEvaluationRepository;
        private readonly ISelfAssessmentRepository _selfAssessmentRepository;
        private readonly ISystemConfigurationRepository _systemConfigurationRepository;
        private ILearnerClassRepository _learnerClassRepository;
        private readonly ApplicationDbContext _dbContext;
        private IDbContextTransaction? _transaction;
        private bool disposed = false;

        public IAccountRepository AccountRepository { get { return _accountRepository; } }
        public IAdminRepository AdminRepository { get { return _adminRepository; } }
        public IManagerRepository ManagerRepository { get { return _managerRepository; } }
        public IStaffRepository StaffRepository { get { return _staffRepository; } }
        public ILearnerRepository LearnerRepository { get { return _learnerRepository; } }
        public ITeacherRepository TeacherRepository { get { return _teacherRepository; } }
        public ICourseRepository CourseRepository { get { return _courseRepository; } }
        public ICourseTypeRepository CourseTypeRepository {  get { return _courseTypeRepository; } }
        public ICourseContentRepository CourseContentRepository { get { return _courseContentRepository; } }
        public IItemTypeRepository ItemTypeRepository { get { return _itemTypeRepository; } }
        public ICourseContentItemRepository CourseContentItemRepository { get { return _courseContentItemRepository; } }
        public IWalletRepository WalletRepository {  get { return _walletRepository; } }
        public IPaymentRepository PaymentsRepository { get { return _paymentRepository; } }
        public IWalletTransactionRepository WalletTransactionRepository {  get { return _walletTransactionRepository; } }
        public IFeedbackRepository FeedbackRepository { get { return _feedbackRepository; } }
        public IFeedbackRepliesRepository FeedbackRepliesRepository { get { return _feedbackRepliesRepository; } }
        public IQnARepository QnARepository { get { return _qnARepository; } }
        public IQnARepliesRepository QnARepliesRepository { get { return _qnARepliesRepository; } }
        public IClassRepository ClassRepository { get { return _classRepository; } }
        public IClassDayRepository ClassDayRepository { get { return _classDayRepository; } }
        public IMajorRepository MajorRepository { get { return _majorRepository; } }
        public IMajorTestRepository MajorTestRepository { get { return _majorTestRepository; } }
        public ILearningRegisRepository LearningRegisRepository { get { return _learningRegisRepository; } }
        public ILearningRegisTypeRepository LearningRegisTypeRepository { get { return _learningRegisTypeRepository; } }
        public ILearningRegisDayRepository LearningRegisDayRepository { get { return _learningRegisDayRepository; } }
        public IPurchaseRepository PurchaseRepository { get { return _purchaseRepository; } }
        public IPurchaseItemRepository PurchaseItemRepository { get { return _purchaseItemRepository; } }
        public ICertificationRepository CertificationRepository { get { return _certificationRepository; } }
        public IScheduleRepository ScheduleRepository { get { return _scheduleRepository; } }
        public ITeacherMajorRepository TeacherMajorRepository { get { return _teacherMajorRepository; } }
        public ILevelAssignedRepository LevelAssignedRepository { get { return _levelAssignedRepository; } }
        public IResponseRepository ResponseRepository { get { return _responseRepository; } }
        public IResponseTypeRepository ResponseTypeRepository { get { return _responseTypeRepository; } }
        public ILearningPathSessionRepository LearningPathSessionRepository { get { return _learningPathSessionRepository; } }
        public ILearnerCourseRepository LearnerCourseRepository { get { return _learnerCourseRepository; } }
        public ILearnerContentProgressRepository LearnerContentProgressRepository { get { return _learnerContentProgressRepository; } }
        public ILearningRegisFeedbackAnswerRepository LearningRegisFeedbackAnswerRepository { get { return _learningRegisFeedbackAnswerRepository; } }
        public ILearningRegisFeedbackOptionRepository LearningRegisFeedbackOptionRepository { get { return _learningRegisFeedbackOptionRepository; } }
        public ILearningRegisFeedbackQuestionRepository LearningRegisFeedbackQuestionRepository { get { return _learningRegisFeedbackQuestionRepository; } }
        public ILearningRegisFeedbackRepository LearningRegisFeedbackRepository { get { return _learningRegisFeedbackRepository; } }
        public IStaffNotificationRepository StaffNotificationRepository { get { return _staffNotificationRepository; } }
        public ITeacherEvaluationRepository TeacherEvaluationRepository { get { return _teacherEvaluationRepository; } }
        public ILevelFeedbackTemplateRepository LevelFeedbackTemplateRepository { get { return _levelFeedbackTemplateRepository; } }
        public ILevelFeedbackCriterionRepository LevelFeedbackCriterionRepository { get { return _levelFeedbackCriterionRepository; } }
        public IClassFeedbackRepository ClassFeedbackRepository { get { return _classFeedbackRepository; } }
        public IClassFeedbackEvaluationRepository ClassFeedbackEvaluationRepository { get { return _classFeedbackEvaluationRepository; } }
        public ILearnerClassRepository LearnerClassRepository { get { return _learnerClassRepository; } }
        public ISelfAssessmentRepository SelfAssessmentRepository { get { return _selfAssessmentRepository; } }
        public ISystemConfigurationRepository SystemConfigurationRepository { get { return _systemConfigurationRepository; } }

        public ApplicationDbContext dbContext { get { return _dbContext; } }



        public UnitOfWork(ApplicationDbContext dbContext, IAccountRepository accountRepository, IAdminRepository adminRepository, IManagerRepository managerRepository, IStaffRepository staffRepository, ILearnerRepository learnerRepository, ITeacherRepository teacherRepository, ICourseRepository courseRepository, ICourseTypeRepository courseTypeRepository, ICourseContentRepository courseContentRepository, IItemTypeRepository itemTypeRepository, ICourseContentItemRepository courseContentItemRepository, IFeedbackRepository feedbackRepository,
            IFeedbackRepliesRepository feedbackRepliesRepository, IQnARepository qnARepository, IQnARepliesRepository qnARepliesRepository, IWalletRepository walletRepository, IPaymentRepository paymentRepository, IWalletTransactionRepository walletTransactionRepository, IClassRepository classRepository, IClassDayRepository classDayRepository, IMajorRepository majorRepository, ILearningRegisRepository learningRegisRepository, ILearningRegisTypeRepository learningRegisTypeRepository,
            IMajorTestRepository majorTestRepository, IPurchaseRepository purchaseRepository, IPurchaseItemRepository purchaseItemRepository, ILearningRegisDayRepository learningRegisDayRepository, ICertificationRepository certificationRepository, IScheduleRepository scheduleRepository, ITeacherMajorRepository teacherMajorRepository, ILevelAssignedRepository levelAssignedRepository, IResponseRepository responseRepository, IResponseTypeRepository responseTypeRepository, ILearningPathSessionRepository learningPathSessionRepository
            , ILearnerCourseRepository learnerCourseRepository, ILearnerContentProgressRepository learnerContentProgressRepository, ILearningRegisFeedbackAnswerRepository learningRegisFeedbackAnswerRepository, ILearningRegisFeedbackOptionRepository learningRegisFeedbackOptionRepository, ILearningRegisFeedbackQuestionRepository learningRegisFeedbackQuestionRepository, ILearningRegisFeedbackRepository learningRegisFeedbackRepository, IStaffNotificationRepository staffNotificationRepository, ITeacherEvaluationRepository teacherEvaluationRepository, 
            ILevelFeedbackTemplateRepository levelFeedbackTemplateRepository, ILevelFeedbackCriterionRepository levelFeedbackCriterionRepository, IClassFeedbackRepository classFeedbackRepository, IClassFeedbackEvaluationRepository classFeedbackEvaluationRepository, ISelfAssessmentRepository selfAssessmentRepository, ILearnerClassRepository learnerClassRepository, ISystemConfigurationRepository systemConfigurationRepository)
        {
            _dbContext = dbContext;
            _adminRepository = adminRepository;
            _managerRepository = managerRepository;
            _staffRepository = staffRepository;
            _accountRepository = accountRepository;
            _learnerRepository = learnerRepository;
            _teacherRepository = teacherRepository;
            _courseRepository = courseRepository;
            _courseTypeRepository = courseTypeRepository;
            _courseContentRepository = courseContentRepository;
            _itemTypeRepository = itemTypeRepository;
            _courseContentItemRepository = courseContentItemRepository;
            _walletRepository = walletRepository;
            _paymentRepository = paymentRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _feedbackRepository = feedbackRepository;
            _feedbackRepliesRepository = feedbackRepliesRepository;
            _qnARepository = qnARepository;
            _qnARepliesRepository = qnARepliesRepository;
            _classRepository = classRepository;
            _classDayRepository = classDayRepository;
            _majorRepository = majorRepository;
            _majorTestRepository = majorTestRepository;
            _learningRegisRepository = learningRegisRepository;
            _learningRegisTypeRepository = learningRegisTypeRepository;
            _purchaseRepository = purchaseRepository;
            _purchaseItemRepository = purchaseItemRepository;
            _learningRegisDayRepository = learningRegisDayRepository;
            _certificationRepository = certificationRepository;
            _scheduleRepository = scheduleRepository;
            _teacherMajorRepository = teacherMajorRepository;
            _levelAssignedRepository = levelAssignedRepository;
            _responseRepository = responseRepository;
            _responseTypeRepository = responseTypeRepository;
            _learningPathSessionRepository = learningPathSessionRepository;
            _learnerCourseRepository = learnerCourseRepository;
            _learnerContentProgressRepository = learnerContentProgressRepository;
            _learningRegisFeedbackAnswerRepository = learningRegisFeedbackAnswerRepository;
            _learningRegisFeedbackOptionRepository = learningRegisFeedbackOptionRepository;
            _learningRegisFeedbackQuestionRepository = learningRegisFeedbackQuestionRepository;
            _learningRegisFeedbackRepository = learningRegisFeedbackRepository;
            _staffNotificationRepository = staffNotificationRepository;
            _teacherEvaluationRepository = teacherEvaluationRepository;
            _levelFeedbackTemplateRepository = levelFeedbackTemplateRepository;
            _levelFeedbackCriterionRepository = levelFeedbackCriterionRepository;
            _classFeedbackRepository = classFeedbackRepository;
            _classFeedbackEvaluationRepository = classFeedbackEvaluationRepository;
            _selfAssessmentRepository = selfAssessmentRepository;
            _learnerClassRepository = learnerClassRepository;
            _systemConfigurationRepository = systemConfigurationRepository;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                }
                disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<int> SaveChangeAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            _transaction = await _dbContext.Database.BeginTransactionAsync();
            return _transaction;
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
                }
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }
    }
}

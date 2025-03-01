using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Data;
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
        private readonly ApplicationDbContext _dbContext;
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
        public ApplicationDbContext dbContext { get { return _dbContext; } }

        

        public UnitOfWork(ApplicationDbContext dbContext, IAccountRepository accountRepository, IAdminRepository adminRepository, IManagerRepository managerRepository, IStaffRepository staffRepository, ILearnerRepository learnerRepository, ITeacherRepository teacherRepository, ICourseRepository courseRepository, ICourseTypeRepository courseTypeRepository, ICourseContentRepository courseContentRepository, IItemTypeRepository itemTypeRepository, ICourseContentItemRepository courseContentItemRepository, IWalletRepository walletRepository, IPaymentRepository paymentRepository, IWalletTransactionRepository walletTransactionRepository)
        public UnitOfWork(ApplicationDbContext dbContext, IAccountRepository accountRepository, IAdminRepository adminRepository, IManagerRepository managerRepository, IStaffRepository staffRepository, ILearnerRepository learnerRepository, ITeacherRepository teacherRepository, ICourseRepository courseRepository, ICourseTypeRepository courseTypeRepository, ICourseContentRepository courseContentRepository, IItemTypeRepository itemTypeRepository, ICourseContentItemRepository courseContentItemRepository, IFeedbackRepository feedbackRepository, IFeedbackRepliesRepository feedbackRepliesRepository, IQnARepository qnARepository, IQnARepliesRepository qnARepliesRepository)
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
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
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
    }
}

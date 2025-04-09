using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Purchase;
using InstruLearn_Application.Model.Models.DTO.PurchaseItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class PurchaseItemService : IPurchaseItemService
    {
        private readonly IPurchaseItemRepository _purchaseItemRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PurchaseItemService(IPurchaseItemRepository purchaseItemRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _purchaseItemRepository = purchaseItemRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<List<ResponseDTO>> GetAllPurchaseItemAsync()
        {
            var purchaseItemList = await _unitOfWork.PurchaseItemRepository.GetPurchaseItemWithFullCourseDetailsAsync();
            var purchaseItemDtos = _mapper.Map<IEnumerable<PurchaseItemDTO>>(purchaseItemList);
            var responseList = new List<ResponseDTO>();

            foreach (var purchaseItemDto in purchaseItemDtos)
            {
                responseList.Add(new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Thành công lấy ra mục mua hàng.",
                    Data = purchaseItemDto
                });
            }

            return responseList;
        }

        public async Task<ResponseDTO> GetPurchaseItemByIdAsync(int purchaseItemId)
        {
            var purchaseItem = await _unitOfWork.PurchaseItemRepository.GetPurchaseItemsWithFullCourseDetailsByPurchaseIdAsync(purchaseItemId);

            if (purchaseItem == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy mục mua hàng.",
                };
            }

            var purchaseItemDto = _mapper.Map<PurchaseItemDTO>(purchaseItem);

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Thành công lấy ra mục mua hàng.",
                Data = purchaseItemDto
            };
        }

        public async Task<ResponseDTO> GetPurchaseItemByLearnerIdAsync(int learnerId)
        {
            var purchaseItemList = await _unitOfWork.PurchaseItemRepository.GetPurchaseItemWithFullCourseDetailsAsync();
            if (purchaseItemList == null || !purchaseItemList.Any())
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy mục mua hàng.",
                    Data = null
                };
            }

            var purchaseItems = purchaseItemList.Where(pi => pi.Purchase != null && pi.Purchase.LearnerId == learnerId).ToList();
            if (!purchaseItems.Any())
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Không tìm thấy mục mua hàng nào cho học viên có ID {learnerId}.",
                    Data = null
                };
            }

            var purchaseItemDtos = _mapper.Map<IEnumerable<PurchaseItemDTO>>(purchaseItems);

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Thành công lấy ra mục mua hàng.",
                Data = purchaseItemDtos
            };
        }

        public async Task<ResponseDTO> CreatePurchaseItemAsync(CreatePurchaseItemDTO createPurchaseItemsDTO)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Validate learner exists
                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(createPurchaseItemsDTO.LearnerId);
                if (learner == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy học viên",
                    };
                }

                // Get the wallet
                var wallet = await _unitOfWork.WalletRepository.FirstOrDefaultAsync(w => w.LearnerId == createPurchaseItemsDTO.LearnerId);
                if (wallet == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy ví",
                    };
                }

                // Create new purchase
                var purchase = new Purchase
                {
                    LearnerId = createPurchaseItemsDTO.LearnerId,
                    PurchaseDate = DateTime.Now,
                    Learner = learner
                };

                // Add purchase to repository
                await _unitOfWork.PurchaseRepository.AddAsync(purchase);
                await _unitOfWork.SaveChangeAsync(); // Save to get generated PurchaseId

                decimal totalAmountToDeduct = 0;
                List<Purchase_Items> purchaseItems = new List<Purchase_Items>();
                List<string> courseNames = new List<string>();

                // Process each course package in the request
                foreach (var item in createPurchaseItemsDTO.CoursePackages)
                {
                    // Validate course package exists
                    var coursePackage = await _unitOfWork.CourseRepository.GetByIdAsync(item.CoursePackageId);
                    if (coursePackage == null)
                    {
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = $"Không tìm thấy gói học có ID {item.CoursePackageId}",
                        };
                    }

                    // Store course name for success message
                    courseNames.Add(coursePackage.CourseName);

                    // Add course price to total
                    decimal itemAmount = coursePackage.Price ?? 0m;
                    totalAmountToDeduct += itemAmount;

                    // Create purchase item
                    var purchaseItem = new Purchase_Items
                    {
                        PurchaseId = purchase.PurchaseId, // Use the new purchase ID
                        CoursePackageId = item.CoursePackageId,
                        TotalAmount = itemAmount,
                        Purchase = purchase,
                        CoursePackage = coursePackage
                    };
                    purchaseItems.Add(purchaseItem);
                }

                // Check if wallet has sufficient funds
                if (wallet.Balance < totalAmountToDeduct)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không đủ tiền trong ví",
                    };
                }

                // Add all purchase items
                foreach (var item in purchaseItems)
                {
                    await _unitOfWork.PurchaseItemRepository.AddAsync(item);
                }

                // Create a new wallet transaction record
                string transactionId = Guid.NewGuid().ToString();
                var walletTransaction = new WalletTransaction
                {
                    TransactionId = transactionId,
                    WalletId = wallet.WalletId,
                    Amount = totalAmountToDeduct,
                    TransactionType = TransactionType.Payment,
                    Status = TransactionStatus.Complete,
                    TransactionDate = DateTime.Now,
                    Wallet = wallet
                };

                await _unitOfWork.WalletTransactionRepository.AddAsync(walletTransaction);

                // Create a payment record linked to the transaction
                var payment = new Payment
                {
                    WalletId = wallet.WalletId,
                    TransactionId = transactionId,
                    WalletTransaction = walletTransaction,
                    AmountPaid = totalAmountToDeduct,
                    PaymentMethod = PaymentMethod.Wallet,
                    PaymentFor = PaymentFor.Online_Course,
                    Status = PaymentStatus.Completed,
                    Wallet = wallet
                };

                await _unitOfWork.PaymentsRepository.AddAsync(payment);

                // Update wallet balance
                wallet.Balance -= totalAmountToDeduct;
                wallet.UpdateAt = DateTime.Now;

                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Create detailed success message including learner information
                string courseListString = string.Join(", ", courseNames);
                string successMessage = $"Học viên {learner.FullName} đã mua thành công {purchaseItems.Count} gói học: {courseListString} với tổng số tiền là {totalAmountToDeduct:C}";

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = successMessage,
                    Data = new
                    {
                        Learner = new
                        {
                            LearnerId = learner.LearnerId,
                            FullName = learner.FullName
                        },
                        TotalAmount = totalAmountToDeduct,
                        PurchaseId = purchase.PurchaseId,
                        TransactionId = transactionId,
                        CoursePackages = purchaseItems.Select(pi => new {
                            CoursePackageId = pi.CoursePackageId,
                            CoursePackageName = pi.CoursePackage.CourseName,
                            Amount = pi.TotalAmount
                        }).ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                // Rollback in case of any error
                await _unitOfWork.RollbackTransactionAsync();
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Đã xảy ra lỗi: {ex.Message}",
                };
            }
        }

        public async Task<ResponseDTO> DeletePurchaseItemAsync(int purchaseItemId)
        {
            var deletePurchaseItem = await _unitOfWork.PurchaseItemRepository.GetByIdAsync(purchaseItemId);
            if (deletePurchaseItem != null)
            {
                await _unitOfWork.PurchaseItemRepository.DeleteAsync(purchaseItemId);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã xóa thành công mục mua hàng"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Không tìm thấy mục mua hàng có ID {purchaseItemId}"
                };
            }
        }
    }
}
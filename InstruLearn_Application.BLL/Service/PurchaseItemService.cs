using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
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
            var purchaseItemList = await _unitOfWork.PurchaseItemRepository.GetAllAsync();
            var puchaseItemDtos = _mapper.Map<IEnumerable<PurchaseItemDTO>>(purchaseItemList);

            var responseList = new List<ResponseDTO>();

            foreach (var purchaseItemDto in puchaseItemDtos)
            {
                responseList.Add(new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Purchase item retrieved successfully.",
                    Data = purchaseItemDto
                });
            }
            return responseList;
        }

        public async Task<ResponseDTO> GetPurchaseItemByIdAsync(int purchaseItemId)
        {
            var purchaseItem = await _unitOfWork.PurchaseItemRepository.GetByIdAsync(purchaseItemId);
            if (purchaseItem == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Purchase item not found.",
                };
            }
            var puchaseDto = _mapper.Map<PurchaseItemDTO>(purchaseItem);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Purchase item retrieved successfully.",
                Data = puchaseDto
            };
        }

        public async Task<ResponseDTO> CreatePurchaseItemAsync(CreatePurchaseItemDTO createPurchaseItemsDTO)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var purchase = await _unitOfWork.PurchaseRepository.GetByIdAsync(createPurchaseItemsDTO.PurchaseId);
                if (purchase == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Purchase not found",
                    };
                }
                // Get the learner ID directly from the purchase
                int learnerId = purchase.LearnerId;
                // Get the wallet directly from repository
                var wallet = await _unitOfWork.WalletRepository.FirstOrDefaultAsync(w => w.LearnerId == learnerId);
                if (wallet == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Wallet not found",
                    };
                }
                decimal totalAmountToDeduct = 0;
                List<Purchase_Items> purchaseItems = new List<Purchase_Items>();
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
                            Message = $"Course package with ID {item.CoursePackageId} not found",
                        };
                    }
                    // Add course price to total
                    decimal itemAmount = coursePackage.Price;
                    totalAmountToDeduct += itemAmount;
                    // Create purchase item
                    var purchaseItem = new Purchase_Items
                    {
                        PurchaseId = createPurchaseItemsDTO.PurchaseId,
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
                        Message = "Insufficient funds in wallet",
                    };
                }
                // Add all purchase items
                foreach (var item in purchaseItems)
                {
                    await _unitOfWork.PurchaseItemRepository.AddAsync(item);
                }
                // Update wallet balance
                wallet.Balance -= totalAmountToDeduct;
                wallet.UpdateAt = DateTime.Now;
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();
                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Successfully purchased {purchaseItems.Count} course packages for total amount {totalAmountToDeduct}",
                    Data = new
                    {
                        TotalAmount = totalAmountToDeduct,
                        PurchaseId = purchase.PurchaseId,
                        CoursePackages = purchaseItems.Select(pi => new {
                            CoursePackageId = pi.CoursePackageId,
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
                    Message = $"An error occurred: {ex.Message}",
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
                    Message = "Purchase item deleted successfully"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Purchase item with ID {purchaseItemId} not found"
                };
            }
        }
    }
}

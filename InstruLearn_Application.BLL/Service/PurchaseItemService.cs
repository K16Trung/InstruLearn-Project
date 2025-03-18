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

        public async Task<ResponseDTO> CreatePurchaseItemAsync(CreatePurchaseItemDTO createPurchaseItemDTO)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var purchase = await _unitOfWork.PurchaseRepository.GetByIdAsync(createPurchaseItemDTO.PurchaseId);
                if (purchase == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Purchase not found",
                    };
                }

                // Validate course package exists
                var coursePackage = await _unitOfWork.CourseRepository.GetByIdAsync(createPurchaseItemDTO.CoursePackageId);
                if (coursePackage == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Course package not found",
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

                // Calculate the amount to deduct
                decimal amountToDeduct = createPurchaseItemDTO.TotalAmount;

                // Check if wallet has sufficient funds
                if (wallet.Balance < amountToDeduct)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Insufficient funds in wallet",
                    };
                }

                // Create the purchase item
                var purchaseItemObj = _mapper.Map<Purchase_Items>(createPurchaseItemDTO);
                purchaseItemObj.Purchase = purchase;
                purchaseItemObj.CoursePackage = coursePackage;

                await _unitOfWork.PurchaseItemRepository.AddAsync(purchaseItemObj);

                wallet.Balance -= amountToDeduct;
                wallet.UpdateAt = DateTime.Now;

                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Purchase item added successfully and wallet updated",
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

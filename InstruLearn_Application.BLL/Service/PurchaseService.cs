﻿using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.Model.Models.DTO.Purchase;
using InstruLearn_Application.Model.Models;

namespace InstruLearn_Application.BLL.Service
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PurchaseService(IPurchaseRepository purchaseRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _purchaseRepository = purchaseRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<List<ResponseDTO>> GetAllPurchaseAsync()
        {
            var purchaseList = await _unitOfWork.PurchaseRepository.GetAllAsync();
            var puchaseDtos = _mapper.Map<IEnumerable<PurchaseDTO>>(purchaseList);

            var responseList = new List<ResponseDTO>();

            foreach (var purchaseDto in puchaseDtos)
            {
                responseList.Add(new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Lấy ra giao dịch mua thành công.",
                    Data = purchaseDto
                });
            }
            return responseList;
        }
        public async Task<ResponseDTO> GetPurchaseByIdAsync(int purchaseId)
        {
            var purchase = await _unitOfWork.PurchaseRepository.GetByIdAsync(purchaseId);
            if (purchase == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy giao dịch mua.",
                };
            }
            var puchaseDto = _mapper.Map<PurchaseDTO>(purchase);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Lấy ra giao dịch mua thành công.",
                Data = puchaseDto
            };
        }
        public async Task<ResponseDTO> GetPurchaseByLearnerIdAsync(int learnerId)
        {
            var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learnerId);
            if (learner == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy học viên.",
                };
            }

            var purchases = await _unitOfWork.PurchaseRepository.GetByLearnerIdAsync(learnerId);

            var purchaseDtos = _mapper.Map<IEnumerable<PurchaseDTO>>(purchases);

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Lấy ra giao dịch mua thành công.",
                Data = purchaseDtos
            };
        }
        public async Task<ResponseDTO> DeletePurchaseAsync(int purchaseId)
        {
            var deletePurchase = await _unitOfWork.PurchaseRepository.GetByIdAsync(purchaseId);
            if (deletePurchase != null)
            {
                await _unitOfWork.PurchaseRepository.DeleteAsync(purchaseId);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã xóa giao dịch mua thành công  "
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Không tìm thấy giao dịch mua với ID {purchaseId}"
                };
            }
        }
    }
}

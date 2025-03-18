using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models.DTO.Syllabus;
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
            var puchaseDtos = _mapper.Map<IEnumerable<SyllabusDTO>>(purchaseList);

            var responseList = new List<ResponseDTO>();

            foreach (var purchaseDto in puchaseDtos)
            {
                responseList.Add(new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Purchase retrieved successfully.",
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
                    Message = "Purchase not found.",
                };
            }
            var puchaseDto = _mapper.Map<PurchaseDTO>(purchase);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Purchase retrieved successfully.",
                Data = puchaseDto
            };
        }
        public Task<ResponseDTO> CreatePurchaseAsync(CreatePurchaseDTO purchaseDTO)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDTO> DeletePurchaseAsync(int purchaseId)
        {
            throw new NotImplementedException();
        }
    }
}

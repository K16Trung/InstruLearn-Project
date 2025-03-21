using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Certification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class CertificationService : ICertificationService
    {
        private readonly ICertificationRepository _certificationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CertificationService(ICertificationRepository certificationRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _certificationRepository = certificationRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ResponseDTO>> GetAllCertificationAsync()
        {
            var certificationList = await _unitOfWork.CertificationRepository.GetAllAsync();
            var certificationDtos = _mapper.Map<IEnumerable<CertificationDTO>>(certificationList);

            var responseList = new List<ResponseDTO>();

            foreach (var certificationDto in certificationDtos)
            {
                responseList.Add(new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Certification retrieved successfully.",
                    Data = certificationDto
                });
            }
            return responseList;
        }

        public async Task<ResponseDTO> GetCertificationByIdAsync(int certificationId)
        {
            var certification = await _unitOfWork.CertificationRepository.GetByIdAsync(certificationId);
            if (certification == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Certification not found.",
                };
            }
            var certificationDto = _mapper.Map<CertificationDTO>(certification);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Certification retrieved successfully.",
                Data = certificationDto
            };
        }

        public async Task<ResponseDTO> CreateCertificationAsync(CreateCertificationDTO createCertificationDTO)
        {
            var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(createCertificationDTO.LearnerId);
            if (learner == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Learner not found",
                };
            }
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(createCertificationDTO.CoursePackageId);
            if (course == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Course not found",
                };
            }
            var certificationObj = _mapper.Map<Certification>(createCertificationDTO);
            certificationObj.Learner = learner;
            certificationObj.CoursePackages = course;

            await _unitOfWork.CertificationRepository.AddAsync(certificationObj);
            await _unitOfWork.SaveChangeAsync();

            var response = new ResponseDTO
            {
                IsSucceed = true,
                Message = "Certification added successfully",
            };

            return response;
        }
        public async Task<ResponseDTO> UpdateCertificationAsync(int certificationId, UpdateCertificationDTO updateCertificationDTO)
        {
            var certificationUpdate = await _unitOfWork.CertificationRepository.GetByIdAsync(certificationId);
            if (certificationUpdate != null)
            {
                certificationUpdate = _mapper.Map(updateCertificationDTO, certificationUpdate);
                await _unitOfWork.CertificationRepository.UpdateAsync(certificationUpdate);
                var result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Certification update successfully!"
                    };
                }
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Certification update failed!"
                };
            }
            return new ResponseDTO
            {
                IsSucceed = false,
                Message = "Certification not found!"
            };
        }
        public async Task<ResponseDTO> DeleteCertificationAsync(int certificationId)
        {
            var deleteCertification = await _unitOfWork.CertificationRepository.GetByIdAsync(certificationId);
            if (deleteCertification != null)
            {
                await _unitOfWork.CertificationRepository.DeleteAsync(certificationId);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Certification deleted successfully"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Certification with ID {certificationId} not found"
                };
            }
        }
    }
}

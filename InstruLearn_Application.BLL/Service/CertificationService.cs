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
                    Message = "Lấy ra danh sách chứng nhận thành công.",
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
                    Message = "Không tìm thấy chứng nhận.",
                };
            }
            var certificationDto = _mapper.Map<CertificationDTO>(certification);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã lấy lại chứng nhận thành công.",
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
                    Message = "Không tìm thấy học viên",
                };
            }
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(createCertificationDTO.CoursePackageId);
            if (course == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy gói học",
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
                Message = "Thêm chứng nhận thành công",
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
                        Message = "Cập nhật chứng nhận thành công!"
                    };
                }
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Cập nhật chứng nhận thất bại!"
                };
            }
            return new ResponseDTO
            {
                IsSucceed = false,
                Message = "Không tìm thấy chứng nhận!"
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
                    Message = "Xóa chứng nhận thất bại"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Không tìm thấy chứng nhận có ID {certificationId}"
                };
            }
        }
    }
}

using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Certification;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ICertificationService
    {
        Task<List<ResponseDTO>> GetAllCertificationAsync();
        Task<ResponseDTO> GetCertificationByIdAsync(int certificationId);
        Task<ResponseDTO> CreateCertificationAsync(CreateCertificationDTO createCertificationDTO);
        Task<ResponseDTO> UpdateCertificationAsync(int certificationId, UpdateCertificationDTO updateCertificationDTO);
        Task<ResponseDTO> DeleteCertificationAsync(int certificationId);
        Task<ResponseDTO> GetLearnerCertificationsAsync(int learnerId);
        Task<ResponseDTO> IsEligibleForCertificateAsync(int learnerId, CertificationType certificationType, int? learningRegisId = null);
    }
}
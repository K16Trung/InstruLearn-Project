using InstruLearn_Application.Model.Models.DTO.Certification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IGoogleSheetsService
    {
        Task<Dictionary<string, object>> TestGoogleSheetsConnectionAsync();
        Task<bool> SaveCertificationDataAsync(CertificationDataDTO certificationData);
        Task<List<CertificationDataDTO>> GetAllCertificatesAsync();
    }
}

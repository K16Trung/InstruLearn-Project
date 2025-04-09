using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.SyllbusContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ISyllabusContentService
    {
        Task<List<ResponseDTO>> GetAllSyllabusContentsAsync();
        Task<ResponseDTO> GetSyllabusContentByIdAsync(int syallbusContentId);
        Task<ResponseDTO> AddSyllabusContentAsync(CreateSyllabusContentDTO createDto);
        Task<ResponseDTO> UpdateSyllabusContentAsync(int syallbusContentId, UpdateSyllabusContentDTO updateDto);
        Task<ResponseDTO> DeleteSyllabusContentAsync(int syallbusContentId);
    }
}

using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IResponseService
    {
        Task<List<ResponseDTO>> GetAllResponseAsync();
        Task<ResponseDTO> GetResponseByIdAsync(int responseId);
        Task<ResponseDTO> CreateResponseAsync(CreateResponseDTO createResponseDTO);
        Task<ResponseDTO> UpdateResponseAsync(int responseId, UpdateResponseDTO updateResponseDTO);
        Task<ResponseDTO> DeleteResponseAsync(int responseId);
    }
}

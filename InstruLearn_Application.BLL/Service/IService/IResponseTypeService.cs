using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.ResponseType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IResponseTypeService
    {
        Task<List<ResponseDTO>> GetAllResponseType();
        Task<ResponseDTO> GetResponseTypeById(int responseTypeId);
        Task<ResponseDTO> CreateResponseType(CreateResponseTypeDTO createResponseTypeDTO);
        Task<ResponseDTO> UpdateResponseType(int responseTypeId, UpdateResponseTypeDTO createResponseTypeDTO);
        Task<ResponseDTO> DeleteResponseType(int responseTypeId);
    }
}

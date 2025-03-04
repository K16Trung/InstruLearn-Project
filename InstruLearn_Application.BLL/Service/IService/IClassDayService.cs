using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.ClassDay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IClassDayService
    {
        Task<List<ClassDayDTO>> GetAllClassDayAsync();
        Task<ClassDayDTO> GetClassDayByIdAsync(int classDayId);
        Task<ResponseDTO> AddClassDayAsync(CreateClassDayDTO createClassDayDTO);
        Task<ResponseDTO> UpdateClassDayAsync(int classDayId, UpdateClassDayDTO updateClassDayDTO);
        Task<ResponseDTO> DeleteClassDayAsync(int classDayId);
    }
}

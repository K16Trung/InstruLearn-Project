using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.CenterCourse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ICenterCourseService
    {
        Task<List<CenterCourseDTO>> GetAllCenterCourseAsync();
        Task<CenterCourseDTO> GetCenterCourseByIdAsync(int centerCourseId);
        Task<ResponseDTO> AddCenterCourseAsync(CreateCenterCourseDTO createCenterCourseDTO);
        Task<ResponseDTO> UpdateCenterCourseAsync(int centerCourseId, UpdateCenterCourseDTO updateCenterCourseDTO);
        Task<ResponseDTO> DeleteCenterCourseAsync(int centerCourseId);
    }
}

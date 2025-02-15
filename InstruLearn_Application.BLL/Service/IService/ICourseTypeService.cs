using InstruLearn_Application.Model.Models.DTO.Course;
using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.Model.Models.DTO.CourseType;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ICourseTypeService
    {
        Task<ResponseDTO> GetAllCourseTypeAsync();
        Task<ResponseDTO> GetCourseTypeByIdAsync(int courseTypeId);
        Task<ResponseDTO> AddCourseTypeAsync(CreateCourseTypeDTO createDto);
        Task<ResponseDTO> UpdateCourseTypeAsync(int courseTypeId, UpdateCourseTypeDTO updateDto);
        Task<ResponseDTO> DeleteCourseTypeAsync(int courseTypeId);
    }
}

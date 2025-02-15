using InstruLearn_Application.Model.Models.DTO.Course;
using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ICourseService
    {
        Task<ResponseDTO> GetAllCoursesAsync();
        Task<ResponseDTO> GetCourseByIdAsync(int courseId);
        Task<ResponseDTO> AddCourseAsync(CreateCourseDTO createDto);
        Task<ResponseDTO> UpdateCourseAsync(int courseId, UpdateCourseDTO updateDto);
        Task<ResponseDTO> DeleteCourseAsync(int courseId);
    }
}

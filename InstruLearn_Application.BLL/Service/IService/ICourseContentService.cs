using InstruLearn_Application.Model.Models.DTO.CourseType;
using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.Model.Models.DTO.Course_Content;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ICourseContentService
    {
        Task<ResponseDTO> GetAllCourseContentAsync();
        Task<ResponseDTO> GetCourseContentByIdAsync(int courseContentId);
        Task<ResponseDTO> AddCourseContentAsync(CreateCourseContentDTO createDto);
        Task<ResponseDTO> UpdateCourseContentAsync(int courseContentId, UpdateCourseContentDTO updateDto);
        Task<ResponseDTO> DeleteCourseContentAsync(int courseContentId);
    }
}

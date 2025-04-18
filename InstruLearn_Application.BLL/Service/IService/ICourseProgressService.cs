using InstruLearn_Application.Model.Models.DTO.LearnerCourse;
using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ICourseProgressService
    {
        Task<ResponseDTO> UpdateCourseProgressAsync(UpdateLearnerCourseProgressDTO updateDto);
        Task<ResponseDTO> UpdateContentItemProgressAsync(int learnerId, int contentItemId);
        Task<ResponseDTO> GetCourseProgressAsync(int learnerId, int coursePackageId);
        Task<ResponseDTO> GetAllCourseProgressByLearnerAsync(int learnerId);
        Task<ResponseDTO> GetAllLearnersForCourseAsync(int coursePackageId);
    }
}

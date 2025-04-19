using InstruLearn_Application.Model.Models.DTO.LearnerCourse;
using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.Model.Models.DTO.LearnerVideoProgress;
using InstruLearn_Application.Model.Models;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ICourseProgressService
    {
        Task<ResponseDTO> UpdateCourseProgressAsync(UpdateLearnerCourseProgressDTO updateDto);
        Task<ResponseDTO> UpdateContentItemProgressAsync(int learnerId, int contentItemId);
        Task<ResponseDTO> GetCourseProgressAsync(int learnerId, int coursePackageId);
        Task<ResponseDTO> GetAllCourseProgressByLearnerAsync(int learnerId);
        Task<ResponseDTO> GetAllLearnersForCourseAsync(int coursePackageId);
        Task<ResponseDTO> UpdateVideoProgressAsync(UpdateVideoProgressDTO updateDto);
        Task<ResponseDTO> GetVideoProgressAsync(int learnerId, int contentItemId);
        Task<ResponseDTO> GetCourseVideoProgressAsync(int learnerId, int coursePackageId);
        Task<Course_Content_Item> GetContentItemAsync(int contentItemId);
        Task<ItemTypes> GetItemTypeAsync(int itemTypeId);
        Task<bool> UpdateContentItemDurationAsync(int contentItemId, double duration);


    }
}

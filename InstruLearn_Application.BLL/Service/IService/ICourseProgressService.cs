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
        Task<ResponseDTO> UpdateContentItemProgressAsync(int learnerId, int itemId);
        Task<ResponseDTO> GetCourseProgressAsync(int learnerId, int coursePackageId);
        Task<ResponseDTO> GetAllCourseProgressByLearnerAsync(int learnerId);
        Task<ResponseDTO> GetAllLearnersForCourseAsync(int coursePackageId);
        Task<ResponseDTO> UpdateVideoWatchTimeAsync(UpdateVideoWatchTimeDTO updateDto);
        Task<ResponseDTO> UpdateVideoDurationAsync(UpdateVideoDurationDTO updateDto);
        Task<ResponseDTO> GetVideoProgressAsync(int learnerId, int itemId);
        Task<ResponseDTO> GetCourseVideoProgressAsync(int learnerId, int coursePackageId);
        Task<Course_Content_Item> GetContentItemAsync(int itemId);
        Task<ItemTypes> GetItemTypeAsync(int itemTypeId);
        Task<bool> UpdateContentItemDurationAsync(int itemId, double duration);
        Task<ResponseDTO> GetAllCoursePackagesWithDetailsAsync(int learnerId, int coursePackageId);
        Task<bool> RecalculateAllLearnersProgressForCourse(int coursePackageId);
        Task<Learner_Course> GetLearnerCourseAsync(int learnerId, int coursePackageId);
        Task<Course_Content> GetContentByIdAsync(int contentId);
        Task<int> GetCoursePackageIdForContentItemAsync(int itemId);
        Task<bool> HasLearnerPurchasedCourseAsync(int learnerId, int coursePackageId);
        Task<ResponseDTO> GetCompletedCoursesForLearnerAsync(int learnerId);
    }
}
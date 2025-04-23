using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.LearnerCourse;
using InstruLearn_Application.Model.Models.DTO.LearnerVideoProgress;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseProgressController : ControllerBase
    {
        private readonly ICourseProgressService _courseProgressService;

        public CourseProgressController(ICourseProgressService courseProgressService)
        {
            _courseProgressService = courseProgressService;
        }

        private async Task<(bool isValid, ResponseDTO errorResponse)> ValidateContentAccessAsync(int learnerId, int itemId)
        {
            var coursePackageId = await _courseProgressService.GetCoursePackageIdForContentItemAsync(itemId);
            if (coursePackageId == 0)
            {
                return (false, new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy nội dung khóa học."
                });
            }

            bool hasPurchased = await _courseProgressService.HasLearnerPurchasedCourseAsync(learnerId, coursePackageId);
            if (!hasPurchased)
            {
                return (false, new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Học viên chưa mua khóa học này. Vui lòng mua khóa học trước khi cập nhật tiến độ."
                });
            }

            return (true, null);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateCourseProgress(UpdateLearnerCourseProgressDTO updateDto)
        {
            bool hasPurchased = await _courseProgressService.HasLearnerPurchasedCourseAsync(
                updateDto.LearnerId, updateDto.CoursePackageId);

            if (!hasPurchased)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Học viên chưa mua khóa học này. Vui lòng mua khóa học trước khi cập nhật tiến độ."
                });
            }

            var response = await _courseProgressService.UpdateCourseProgressAsync(updateDto);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }

        [HttpPost("update-content-item")]
        public async Task<IActionResult> UpdateContentItemProgress(int learnerId, int itemId)
        {
            var (isValid, errorResponse) = await ValidateContentAccessAsync(learnerId, itemId);
            if (!isValid)
            {
                return BadRequest(errorResponse);
            }

            var contentItem = await _courseProgressService.GetContentItemAsync(itemId);
            if (contentItem != null && contentItem.ItemTypeId > 0)
            {
                var itemType = await _courseProgressService.GetItemTypeAsync(contentItem.ItemTypeId);
                if (itemType != null && itemType.ItemTypeName.ToLower().Contains("video"))
                {
                    return BadRequest(new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không thể đánh dấu video đã hoàn thành thủ công. Vui lòng xem video để tự động theo dõi tiến độ.",
                        Data = new
                        {
                            IsVideo = true,
                            RecommendedApi = "Hãy sử dụng API update-video-watchtime để cập nhật tiến độ xem video."
                        }
                    });
                }
            }

            var response = await _courseProgressService.UpdateContentItemProgressAsync(learnerId, itemId);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }

        [HttpPost("update-video-watchtime")]
        public async Task<IActionResult> UpdateVideoWatchTime(UpdateVideoWatchTimeDTO updateDto)
        {
            try
            {
                var response = await _courseProgressService.UpdateVideoWatchTimeAsync(updateDto);
                return response.IsSucceed ? Ok(response) : BadRequest(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi cập nhật thời gian xem video: {ex.Message}"
                });
            }
        }

        [HttpPost("update-video-duration")]
        public async Task<IActionResult> UpdateVideoDuration(UpdateVideoDurationDTO updateDto)
        {
            try
            {
                var response = await _courseProgressService.UpdateVideoDurationAsync(updateDto);
                return response.IsSucceed ? Ok(response) : BadRequest(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi cập nhật thời lượng video: {ex.Message}"
                });
            }
        }

        [HttpGet("completed-courses/{learnerId}")]
        public async Task<IActionResult> GetCompletedCoursesForLearner(int learnerId)
        {
            var response = await _courseProgressService.GetCompletedCoursesForLearnerAsync(learnerId);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }

        [HttpGet("{learnerId}/{coursePackageId}")]
        public async Task<IActionResult> GetCourseProgress(int learnerId, int coursePackageId)
        {
            bool hasPurchased = await _courseProgressService.HasLearnerPurchasedCourseAsync(
                learnerId, coursePackageId);

            if (!hasPurchased)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Học viên chưa mua khóa học này."
                });
            }

            var response = await _courseProgressService.GetCourseProgressAsync(learnerId, coursePackageId);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }

        [HttpGet("learner/{learnerId}")]
        public async Task<IActionResult> GetAllCourseProgressByLearner(int learnerId)
        {
            var response = await _courseProgressService.GetAllCourseProgressByLearnerAsync(learnerId);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }

        [HttpGet("course/{coursePackageId}")]
        public async Task<IActionResult> GetAllLearnersForCourse(int coursePackageId)
        {
            var response = await _courseProgressService.GetAllLearnersForCourseAsync(coursePackageId);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }

        [HttpGet("video-progress/{learnerId}/{itemId}")]
        public async Task<IActionResult> GetVideoProgress(int learnerId, int itemId)
        {
            var (isValid, errorResponse) = await ValidateContentAccessAsync(learnerId, itemId);
            if (!isValid)
            {
                return BadRequest(errorResponse);
            }

            var response = await _courseProgressService.GetVideoProgressAsync(learnerId, itemId);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }

        [HttpGet("course-video-progress/{learnerId}/{coursePackageId}")]
        public async Task<IActionResult> GetCourseVideoProgress(int learnerId, int coursePackageId)
        {
            bool hasPurchased = await _courseProgressService.HasLearnerPurchasedCourseAsync(
                learnerId, coursePackageId);

            if (!hasPurchased)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Học viên chưa mua khóa học này."
                });
            }

            var response = await _courseProgressService.GetCourseVideoProgressAsync(learnerId, coursePackageId);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }

        [HttpGet("all-course-packages/{learnerId}/{coursePackageId}")]
        public async Task<IActionResult> GetAllCoursePackagesWithDetails(int learnerId, int coursePackageId)
        {
            bool hasPurchased = await _courseProgressService.HasLearnerPurchasedCourseAsync(
                learnerId, coursePackageId);

            if (!hasPurchased)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Học viên chưa mua khóa học này."
                });
            }

            var response = await _courseProgressService.GetAllCoursePackagesWithDetailsAsync(learnerId, coursePackageId);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }
    }
}
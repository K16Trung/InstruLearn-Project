using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.LearnerCourse;
using InstruLearn_Application.Model.Models.DTO.LearnerVideoProgress;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("update")]
        public async Task<IActionResult> UpdateCourseProgress(UpdateLearnerCourseProgressDTO updateDto)
        {
            var response = await _courseProgressService.UpdateCourseProgressAsync(updateDto);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }

        [HttpPost("update-content-item")]
        public async Task<IActionResult> UpdateContentItemProgress(int learnerId, int itemId)
        {
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
                            RecommendedApi = "Hãy sử dụng API update-video-progress để cập nhật tiến độ xem video."
                        }
                    });
                }
            }

            var response = await _courseProgressService.UpdateContentItemProgressAsync(learnerId, itemId);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }

        [HttpPost("update-video-progress")]
        public async Task<IActionResult> UpdateVideoProgress(UpdateVideoProgressDTO updateDto)
        {
            try
            {
                if (!updateDto.TotalDuration.HasValue)
                {
                    var contentItem = await _courseProgressService.GetContentItemAsync(updateDto.ItemId);
                    if (contentItem != null && contentItem.DurationInSeconds.HasValue)
                    {
                        updateDto.TotalDuration = contentItem.DurationInSeconds.Value;
                    }
                }

                if (updateDto.TotalDuration.HasValue && updateDto.TotalDuration.Value > 0)
                {
                    await _courseProgressService.UpdateContentItemDurationAsync(
                        updateDto.ItemId, updateDto.TotalDuration.Value);
                }

                var response = await _courseProgressService.UpdateVideoProgressAsync(updateDto);
                return response.IsSucceed ? Ok(response) : BadRequest(response);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi cập nhật tiến độ video: {ex.Message}"
                });
            }
        }

        [HttpGet("{learnerId}/{coursePackageId}")]
        public async Task<IActionResult> GetCourseProgress(int learnerId, int coursePackageId)
        {
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
            var response = await _courseProgressService.GetVideoProgressAsync(learnerId, itemId);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }

        [HttpGet("course-video-progress/{learnerId}/{coursePackageId}")]
        public async Task<IActionResult> GetCourseVideoProgress(int learnerId, int coursePackageId)
        {
            var response = await _courseProgressService.GetCourseVideoProgressAsync(learnerId, coursePackageId);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }

        [HttpGet("all-course-packages/{learnerId}/{coursePackageId}")]
        public async Task<IActionResult> GetAllCoursePackagesWithDetails(int learnerId, int coursePackageId)
        {
            var response = await _courseProgressService.GetAllCoursePackagesWithDetailsAsync(learnerId, coursePackageId);
            return response.IsSucceed ? Ok(response) : BadRequest(response);
        }

    }
}
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

        /// <summary>
        /// Validates if the learner has access to a course content item
        /// </summary>
        private async Task<(bool isValid, ResponseDTO errorResponse)> ValidateContentAccessAsync(int learnerId, int itemId)
        {
            // Get the course package ID for this content item
            var coursePackageId = await _courseProgressService.GetCoursePackageIdForContentItemAsync(itemId);
            if (coursePackageId == 0)
            {
                return (false, new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy nội dung khóa học."
                });
            }

            // Check if the learner has purchased this course
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
            // Validate if learner has purchased this course
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
            // Validate learner's access to this content
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

        [HttpPost("update-video-progress")]
        [Obsolete("This endpoint is deprecated. Use update-video-watchtime and update-video-duration instead.")]
        public async Task<IActionResult> UpdateVideoProgress(UpdateVideoProgressDTO updateDto)
        {
            try
            {
                // Validate learner's access to this content
                var (isValid, errorResponse) = await ValidateContentAccessAsync(updateDto.LearnerId, updateDto.ItemId);
                if (!isValid)
                {
                    return BadRequest(errorResponse);
                }

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
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi cập nhật tiến độ video: {ex.Message}"
                });
            }
        }

        [HttpPost("update-video-watchtime")]
        public async Task<IActionResult> UpdateVideoWatchTime(UpdateVideoWatchTimeDTO updateDto)
        {
            try
            {
                // Validate learner's access to this content
                var (isValid, errorResponse) = await ValidateContentAccessAsync(updateDto.LearnerId, updateDto.ItemId);
                if (!isValid)
                {
                    return BadRequest(errorResponse);
                }

                // Map to the original DTO for backward compatibility
                var videoProgressDto = new UpdateVideoProgressDTO
                {
                    LearnerId = updateDto.LearnerId,
                    ItemId = updateDto.ItemId,
                    WatchTimeInSeconds = updateDto.WatchTimeInSeconds,
                    TotalDuration = null // Will get from existing content item data
                };

                var contentItem = await _courseProgressService.GetContentItemAsync(updateDto.ItemId);
                if (contentItem != null && contentItem.DurationInSeconds.HasValue)
                {
                    videoProgressDto.TotalDuration = contentItem.DurationInSeconds.Value;
                }

                var response = await _courseProgressService.UpdateVideoProgressAsync(videoProgressDto);
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
                // Validate learner's access to this content
                var (isValid, errorResponse) = await ValidateContentAccessAsync(updateDto.LearnerId, updateDto.ItemId);
                if (!isValid)
                {
                    return BadRequest(errorResponse);
                }

                if (updateDto.TotalDuration <= 0)
                {
                    return BadRequest(new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Thời lượng video phải lớn hơn 0."
                    });
                }

                bool updated = await _courseProgressService.UpdateContentItemDurationAsync(
                    updateDto.ItemId, updateDto.TotalDuration);

                if (updated)
                {
                    // After updating duration, we need to refresh the progress
                    var videoProgress = await _courseProgressService.GetVideoProgressAsync(
                        updateDto.LearnerId, updateDto.ItemId);

                    return Ok(new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Đã cập nhật thời lượng video thành công.",
                        Data = videoProgress.Data
                    });
                }
                else
                {
                    return BadRequest(new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không thể cập nhật thời lượng video."
                    });
                }
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

        [HttpGet("{learnerId}/{coursePackageId}")]
        public async Task<IActionResult> GetCourseProgress(int learnerId, int coursePackageId)
        {
            // Validate if learner has purchased this course
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
            // Validate learner's access to this content
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
            // Validate if learner has purchased this course
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
            // Validate if learner has purchased this course
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
using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.LearnerCourse;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class CourseProgressService : ICourseProgressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CourseProgressService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDTO> UpdateCourseProgressAsync(UpdateLearnerCourseProgressDTO updateDto)
        {
            try
            {
                // Validate learner
                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(updateDto.LearnerId);
                if (learner == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy học viên."
                    };
                }

                // Validate course
                var course = await _unitOfWork.CourseRepository.GetByIdAsync(updateDto.CoursePackageId);
                if (course == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy khóa học."
                    };
                }

                // Calculate course progress based on content items
                double calculatedPercentage = await CalculateCompletionPercentageFromContentItemsAsync(
                    updateDto.LearnerId,
                    updateDto.CoursePackageId,
                    updateDto.CompletionPercentage);

                // Update or create the progress entry
                var success = await _unitOfWork.LearnerCourseRepository.UpdateProgressAsync(
                    updateDto.LearnerId,
                    updateDto.CoursePackageId,
                    calculatedPercentage
                );

                if (!success)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không thể cập nhật tiến độ khóa học."
                    };
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã cập nhật tiến độ khóa học thành công.",
                    Data = new { CompletionPercentage = calculatedPercentage }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi cập nhật tiến độ khóa học: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> UpdateContentItemProgressAsync(int learnerId, int contentItemId, bool isCompleted)
        {
            try
            {
                // Validate learner
                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learnerId);
                if (learner == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy học viên."
                    };
                }

                // Validate content item
                var contentItem = await _unitOfWork.CourseContentItemRepository.GetByIdAsync(contentItemId);
                if (contentItem == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy nội dung khóa học."
                    };
                }

                // Get course content
                var courseContent = await _unitOfWork.CourseContentRepository.GetByIdAsync(contentItem.ContentId);
                if (courseContent == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy phần nội dung khóa học."
                    };
                }

                // Track content item completion
                // This could be in another repository/table, but for now we'll just update the course progress
                var courseProgress = await _unitOfWork.LearnerCourseRepository.GetByLearnerAndCourseAsync(
                    learnerId,
                    courseContent.CoursePackageId);

                if (courseProgress == null)
                {
                    // Need to count completed items vs total items
                    var allContentItems = await GetAllCourseContentItemsAsync(courseContent.CoursePackageId);
                    int totalItems = allContentItems.Count;
                    int completedItems = isCompleted ? 1 : 0;

                    double percentage = totalItems > 0 ? (double)completedItems / totalItems * 100 : 0;

                    // Create a new progress entry
                    await _unitOfWork.LearnerCourseRepository.UpdateProgressAsync(
                        learnerId,
                        courseContent.CoursePackageId,
                        percentage
                    );
                }
                else
                {
                    // Calculate new percentage based on all content items
                    double newPercentage = await CalculateCompletionPercentageFromContentItemsAsync(
                        learnerId,
                        courseContent.CoursePackageId,
                        courseProgress.CompletionPercentage,
                        contentItemId,
                        isCompleted);

                    // Update existing progress entry
                    await _unitOfWork.LearnerCourseRepository.UpdateProgressAsync(
                        learnerId,
                        courseContent.CoursePackageId,
                        newPercentage
                    );
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã cập nhật tiến độ nội dung thành công.",
                    Data = new
                    {
                        ContentItemId = contentItemId,
                        IsCompleted = isCompleted,
                        CoursePackageId = courseContent.CoursePackageId
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi cập nhật tiến độ nội dung: {ex.Message}"
                };
            }
        }

        private async Task<List<Course_Content_Item>> GetAllCourseContentItemsAsync(int coursePackageId)
        {
            var contentItems = new List<Course_Content_Item>();

            // Get all course contents for this course package
            var courseContents = await _unitOfWork.CourseContentRepository.GetQuery()
                .Where(cc => cc.CoursePackageId == coursePackageId)
                .ToListAsync();

            // Get all content items for these course contents
            foreach (var content in courseContents)
            {
                var items = await _unitOfWork.CourseContentItemRepository.GetQuery()
                    .Where(cci => cci.ContentId == content.ContentId)
                    .ToListAsync();

                contentItems.AddRange(items);
            }

            return contentItems;
        }

        private async Task<double> CalculateCompletionPercentageFromContentItemsAsync(
            int learnerId,
            int coursePackageId,
            double currentPercentage,
            int? updatedContentItemId = null,
            bool? isCompleted = null)
        {
            // Get all content items for the course
            var allContentItems = await GetAllCourseContentItemsAsync(coursePackageId);
            int totalItems = allContentItems.Count;

            if (totalItems == 0)
            {
                // If there are no content items, just use the provided percentage
                return currentPercentage;
            }

            // For a proper implementation, you would need a table to track which content items
            // have been completed by each learner. For now, we'll just use the passed-in percentage
            // and adjust it if a specific content item was updated

            if (updatedContentItemId.HasValue && isCompleted.HasValue)
            {
                // Calculate percentage value of a single item
                double itemPercentValue = 100.0 / totalItems;

                // If item is completed and wasn't before, add its percentage
                // If item is uncompleted and was before, subtract its percentage
                // Otherwise, no change

                // For simplicity, we're just assuming the item's status is changing from its opposite
                if (isCompleted.Value)
                {
                    currentPercentage += itemPercentValue;
                }
                else
                {
                    currentPercentage -= itemPercentValue;
                }

                // Ensure percentage is within valid range
                currentPercentage = Math.Max(0, Math.Min(100, currentPercentage));
            }

            return currentPercentage;
        }

        public async Task<ResponseDTO> GetCourseProgressAsync(int learnerId, int coursePackageId)
        {
            try
            {
                var learnerCourse = await _unitOfWork.LearnerCourseRepository
                    .GetByLearnerAndCourseAsync(learnerId, coursePackageId);

                if (learnerCourse == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Chưa có tiến độ cho khóa học này.",
                        Data = new { CompletionPercentage = 0.0 }
                    };
                }

                var course = await _unitOfWork.CourseRepository.GetByIdAsync(coursePackageId);
                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learnerId);

                // Count content items
                var allContentItems = await GetAllCourseContentItemsAsync(coursePackageId);
                int totalItems = allContentItems.Count;

                var learnerCourseDTO = new LearnerCourseDTO
                {
                    LearnerCourseId = learnerCourse.LearnerCourseId,
                    LearnerId = learnerCourse.LearnerId,
                    LearnerName = learner?.FullName ?? "Unknown",
                    CoursePackageId = learnerCourse.CoursePackageId,
                    CourseName = course?.CourseName ?? "Unknown",
                    CompletionPercentage = learnerCourse.CompletionPercentage,
                    EnrollmentDate = learnerCourse.EnrollmentDate,
                    LastAccessDate = learnerCourse.LastAccessDate,
                    TotalContentItems = totalItems
                };

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã lấy tiến độ khóa học thành công.",
                    Data = learnerCourseDTO
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lấy tiến độ khóa học: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetAllCourseProgressByLearnerAsync(int learnerId)
        {
            try
            {
                // Check if learner exists
                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learnerId);
                if (learner == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy học viên."
                    };
                }

                // Get all progress for learner
                var learnerCourses = await _unitOfWork.LearnerCourseRepository.GetByLearnerIdAsync(learnerId);
                var progressDTOs = new List<LearnerCourseDTO>();

                foreach (var lc in learnerCourses)
                {
                    var course = await _unitOfWork.CourseRepository.GetByIdAsync(lc.CoursePackageId);
                    if (course == null) continue;

                    // Count content items for this course
                    var allContentItems = await GetAllCourseContentItemsAsync(lc.CoursePackageId);
                    int totalItems = allContentItems.Count;

                    progressDTOs.Add(new LearnerCourseDTO
                    {
                        LearnerCourseId = lc.LearnerCourseId,
                        LearnerId = lc.LearnerId,
                        LearnerName = learner.FullName,
                        CoursePackageId = lc.CoursePackageId,
                        CourseName = course.CourseName,
                        CompletionPercentage = lc.CompletionPercentage,
                        EnrollmentDate = lc.EnrollmentDate,
                        LastAccessDate = lc.LastAccessDate,
                        TotalContentItems = totalItems
                    });
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã lấy tất cả tiến độ khóa học thành công.",
                    Data = progressDTOs
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lấy tiến độ khóa học: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetAllLearnersForCourseAsync(int coursePackageId)
        {
            try
            {
                // Check if course exists
                var course = await _unitOfWork.CourseRepository.GetByIdAsync(coursePackageId);
                if (course == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy khóa học."
                    };
                }

                // Count content items for this course
                var allContentItems = await GetAllCourseContentItemsAsync(coursePackageId);
                int totalItems = allContentItems.Count;

                // Get all learners and their progress for this course
                var learnerCourses = await _unitOfWork.LearnerCourseRepository.GetByCoursePackageIdAsync(coursePackageId);
                var progressDTOs = new List<LearnerCourseDTO>();

                foreach (var lc in learnerCourses)
                {
                    var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(lc.LearnerId);
                    if (learner == null) continue;

                    progressDTOs.Add(new LearnerCourseDTO
                    {
                        LearnerCourseId = lc.LearnerCourseId,
                        LearnerId = lc.LearnerId,
                        LearnerName = learner.FullName,
                        CoursePackageId = lc.CoursePackageId,
                        CourseName = course.CourseName,
                        CompletionPercentage = lc.CompletionPercentage,
                        EnrollmentDate = lc.EnrollmentDate,
                        LastAccessDate = lc.LastAccessDate,
                        TotalContentItems = totalItems
                    });
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã lấy tất cả học viên cho khóa học thành công.",
                    Data = progressDTOs
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lấy học viên cho khóa học: {ex.Message}"
                };
            }
        }
    }
}
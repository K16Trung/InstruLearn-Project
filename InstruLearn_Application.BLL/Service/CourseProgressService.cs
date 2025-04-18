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

        public async Task<ResponseDTO> UpdateContentItemProgressAsync(int learnerId, int contentItemId)
        {
            try
            {
                bool isCompleted = true;

                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learnerId);
                if (learner == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy học viên."
                    };
                }

                var contentItem = await _unitOfWork.CourseContentItemRepository.GetByIdAsync(contentItemId);
                if (contentItem == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy nội dung khóa học."
                    };
                }

                var courseContent = await _unitOfWork.CourseContentRepository.GetByIdAsync(contentItem.ContentId);
                if (courseContent == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy phần nội dung khóa học."
                    };
                }

                var courseProgress = await _unitOfWork.LearnerCourseRepository.GetByLearnerAndCourseAsync(
                    learnerId,
                    courseContent.CoursePackageId);

                if (courseProgress == null)
                {

                    var allContentItems = await GetAllCourseContentItemsAsync(courseContent.CoursePackageId);
                    int totalItems = allContentItems.Count;

                    int completedItems = 1;

                    double percentage = totalItems > 0 ? (double)completedItems / totalItems * 100 : 0;

                    await _unitOfWork.LearnerCourseRepository.UpdateProgressAsync(
                        learnerId,
                        courseContent.CoursePackageId,
                        percentage
                    );
                }
                else
                {
                    double newPercentage = await CalculateCompletionPercentageFromContentItemsAsync(
                        learnerId,
                        courseContent.CoursePackageId,
                        courseProgress.CompletionPercentage,
                        contentItemId,
                        isCompleted);

                    await _unitOfWork.LearnerCourseRepository.UpdateProgressAsync(
                        learnerId,
                        courseContent.CoursePackageId,
                        newPercentage
                    );
                }

                courseProgress = await _unitOfWork.LearnerCourseRepository.GetByLearnerAndCourseAsync(
                    learnerId,
                    courseContent.CoursePackageId);

                if (courseProgress != null)
                {
                    courseProgress.LastAccessDate = DateTime.Now;
                    await _unitOfWork.SaveChangeAsync();
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

            var courseContents = await _unitOfWork.CourseContentRepository.GetQuery()
                .Where(cc => cc.CoursePackageId == coursePackageId)
                .ToListAsync();

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
            var allContentItems = await GetAllCourseContentItemsAsync(coursePackageId);
            int totalItems = allContentItems.Count;

            if (totalItems == 0)
            {
                return currentPercentage;
            }

            if (updatedContentItemId.HasValue && isCompleted.HasValue)
            {
                double itemPercentValue = 100.0 / totalItems;

                if (isCompleted.Value)
                {
                    currentPercentage += itemPercentValue;
                }
                else
                {
                    currentPercentage -= itemPercentValue;
                }

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
                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learnerId);
                if (learner == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy học viên."
                    };
                }

                var learnerCourses = await _unitOfWork.LearnerCourseRepository.GetByLearnerIdAsync(learnerId);
                var progressDTOs = new List<LearnerCourseDTO>();

                foreach (var lc in learnerCourses)
                {
                    var course = await _unitOfWork.CourseRepository.GetByIdAsync(lc.CoursePackageId);
                    if (course == null) continue;

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
                var course = await _unitOfWork.CourseRepository.GetByIdAsync(coursePackageId);
                if (course == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy khóa học."
                    };
                }

                var allContentItems = await GetAllCourseContentItemsAsync(coursePackageId);
                int totalItems = allContentItems.Count;

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
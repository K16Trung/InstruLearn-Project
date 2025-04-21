using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.LearnerCourse;
using InstruLearn_Application.Model.Models.DTO.LearnerVideoProgress;
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
                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(updateDto.LearnerId);
                if (learner == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy học viên."
                    };
                }

                var course = await _unitOfWork.CourseRepository.GetByIdAsync(updateDto.CoursePackageId);
                if (course == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy khóa học."
                    };
                }

                double calculatedPercentage = await CalculateTypeBasedCompletionPercentageAsync(
                    updateDto.LearnerId,
                    updateDto.CoursePackageId);

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

        public async Task<ResponseDTO> UpdateContentItemProgressAsync(int learnerId, int itemId)
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

                var contentItem = await _unitOfWork.CourseContentItemRepository.GetByIdAsync(itemId);
                if (contentItem == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy nội dung khóa học."
                    };
                }

                var itemType = await _unitOfWork.ItemTypeRepository.GetByIdAsync(contentItem.ItemTypeId);
                if (itemType == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy loại nội dung."
                    };
                }

                bool isVideo = itemType.ItemTypeName.ToLower().Contains("video");

                bool isDocument = !isVideo && (
                    itemType.ItemTypeName.ToLower().Contains("document") ||
                    itemType.ItemTypeName.ToLower().Contains("pdf") ||
                    itemType.ItemTypeName.ToLower().Contains("doc") ||
                    !contentItem.DurationInSeconds.HasValue
                );

                if (isVideo && isDocument)
                {
                    isDocument = false;
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

                if (isDocument)
                {
                    var contentProgress = await _unitOfWork.LearnerContentProgressRepository
                        .GetByLearnerAndContentItemAsync(learnerId, itemId);

                    if (contentProgress == null)
                    {
                        contentProgress = new Learner_Content_Progress
                        {
                            LearnerId = learnerId,
                            ItemId = itemId,
                            IsCompleted = true,
                            WatchTimeInSeconds = 0,
                            LastAccessDate = DateTime.Now
                        };

                        await _unitOfWork.LearnerContentProgressRepository.AddAsync(contentProgress);
                    }
                    else
                    {
                        contentProgress.IsCompleted = true;
                        contentProgress.LastAccessDate = DateTime.Now;

                        await _unitOfWork.LearnerContentProgressRepository.UpdateAsync(contentProgress);
                    }

                    await _unitOfWork.SaveChangeAsync();
                }
                else if (isVideo)
                {
                    var contentProgress = await _unitOfWork.LearnerContentProgressRepository
                        .GetByLearnerAndContentItemAsync(learnerId, itemId);

                    if (contentProgress == null)
                    {
                        contentProgress = new Learner_Content_Progress
                        {
                            LearnerId = learnerId,
                            ItemId = itemId,
                            IsCompleted = false,
                            WatchTimeInSeconds = 0.1,
                            LastAccessDate = DateTime.Now
                        };

                        await _unitOfWork.LearnerContentProgressRepository.AddAsync(contentProgress);
                        await _unitOfWork.SaveChangeAsync();
                    }
                    else
                    {
                        contentProgress.LastAccessDate = DateTime.Now;
                        await _unitOfWork.LearnerContentProgressRepository.UpdateAsync(contentProgress);
                        await _unitOfWork.SaveChangeAsync();
                    }
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
                    double newPercentage = await RecalculateCourseCompletionPercentageAsync(
                        learnerId,
                        courseContent.CoursePackageId);

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
                        ContentItemId = itemId,
                        IsCompleted = isCompleted,
                        CoursePackageId = courseContent.CoursePackageId,
                        IsDocument = isDocument,
                        IsVideo = isVideo
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

                if (learnerCourse.CompletionPercentage < 0)
                {
                    double recalculatedPercentage = await RecalculateCourseCompletionPercentageAsync(
                        learnerId, coursePackageId);

                    await _unitOfWork.LearnerCourseRepository.UpdateProgressAsync(
                        learnerId, coursePackageId, recalculatedPercentage);

                    learnerCourse = await _unitOfWork.LearnerCourseRepository
                        .GetByLearnerAndCourseAsync(learnerId, coursePackageId);
                }

                var course = await _unitOfWork.CourseRepository.GetByIdAsync(coursePackageId);
                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learnerId);

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
        public async Task<ResponseDTO> UpdateVideoProgressAsync(UpdateVideoProgressDTO updateDto)
        {
            try
            {
                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(updateDto.LearnerId);
                if (learner == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy học viên."
                    };
                }

                var contentItem = await _unitOfWork.CourseContentItemRepository.GetByIdAsync(updateDto.ItemId);
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

                double contentDuration = updateDto.TotalDuration ?? contentItem.DurationInSeconds ?? 0;

                bool isCompleted = false;
                double completionPercentage = 0;

                if (contentDuration > 0)
                {

                    isCompleted = updateDto.WatchTimeInSeconds >= (contentDuration * 0.9);
                    completionPercentage = Math.Min(100, (updateDto.WatchTimeInSeconds / contentDuration) * 100);
                }
                else
                {
                    isCompleted = updateDto.WatchTimeInSeconds > 0;
                    completionPercentage = isCompleted ? 100 : 0;
                }

                if (updateDto.TotalDuration.HasValue && updateDto.TotalDuration.Value > 0 &&
                    (!contentItem.DurationInSeconds.HasValue || contentItem.DurationInSeconds.Value != updateDto.TotalDuration.Value))
                {
                    contentItem.DurationInSeconds = updateDto.TotalDuration.Value;
                    await _unitOfWork.CourseContentItemRepository.UpdateAsync(contentItem);
                    await _unitOfWork.SaveChangeAsync();
                }

                await _unitOfWork.LearnerContentProgressRepository.UpdateWatchTimeAsync(
                    updateDto.LearnerId,
                    updateDto.ItemId,
                    updateDto.WatchTimeInSeconds,
                    isCompleted);

                var contentProgress = await _unitOfWork.LearnerContentProgressRepository
                    .GetByLearnerAndContentItemAsync(updateDto.LearnerId, updateDto.ItemId);

                double totalWatchTime = await _unitOfWork.LearnerContentProgressRepository
                    .GetTotalWatchTimeForCourseAsync(updateDto.LearnerId, courseContent.CoursePackageId);

                double totalVideoDuration = await _unitOfWork.LearnerContentProgressRepository
                    .GetTotalVideoDurationForCourseAsync(courseContent.CoursePackageId);

                double courseCompletionPercentage = 0;
                if (totalVideoDuration > 0)
                {
                    courseCompletionPercentage = Math.Min(100, (totalWatchTime / totalVideoDuration) * 100);
                }

                await _unitOfWork.LearnerCourseRepository.UpdateProgressAsync(
                    updateDto.LearnerId,
                    courseContent.CoursePackageId,
                    courseCompletionPercentage);

                var progressDTO = new VideoProgressDTO
                {
                    LearnerId = updateDto.LearnerId,
                    ContentItemId = updateDto.ItemId,
                    WatchTimeInSeconds = contentProgress?.WatchTimeInSeconds ?? updateDto.WatchTimeInSeconds,
                    IsCompleted = isCompleted,
                    TotalDuration = contentDuration,
                    CompletionPercentage = completionPercentage
                };

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã cập nhật tiến độ xem video thành công.",
                    Data = new
                    {
                        VideoProgress = progressDTO,
                        CourseCompletionPercentage = courseCompletionPercentage
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi cập nhật tiến độ xem video: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetVideoProgressAsync(int learnerId, int itemId)
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

                var contentItem = await _unitOfWork.CourseContentItemRepository.GetByIdAsync(itemId);
                if (contentItem == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy nội dung khóa học."
                    };
                }

                var progress = await _unitOfWork.LearnerContentProgressRepository
                    .GetByLearnerAndContentItemAsync(learnerId, itemId);

                var progressDTO = new VideoProgressDTO
                {
                    LearnerId = learnerId,
                    ContentItemId = itemId,
                    WatchTimeInSeconds = progress?.WatchTimeInSeconds ?? 0,
                    IsCompleted = progress?.IsCompleted ?? false,
                    TotalDuration = contentItem.DurationInSeconds,
                    CompletionPercentage = contentItem.DurationInSeconds.HasValue && contentItem.DurationInSeconds > 0 && progress != null
                        ? Math.Min(100, (progress.WatchTimeInSeconds / contentItem.DurationInSeconds.Value) * 100)
                        : 0
                };

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã lấy tiến độ xem video thành công.",
                    Data = progressDTO
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lấy tiến độ xem video: {ex.Message}"
                };
            }
        }
        public async Task<ResponseDTO> GetCourseVideoProgressAsync(int learnerId, int coursePackageId)
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

                var course = await _unitOfWork.CourseRepository.GetByIdAsync(coursePackageId);
                if (course == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy khóa học."
                    };
                }

                double totalWatchTime = await _unitOfWork.LearnerContentProgressRepository
                    .GetTotalWatchTimeForCourseAsync(learnerId, coursePackageId);

                double totalVideoDuration = await _unitOfWork.LearnerContentProgressRepository
                    .GetTotalVideoDurationForCourseAsync(coursePackageId);

                double completionPercentage = 0;
                if (totalVideoDuration > 0)
                {
                    completionPercentage = Math.Min(100, (totalWatchTime / totalVideoDuration) * 100);
                }

                var progressDTO = new CourseVideoProgressDTO
                {
                    CoursePackageId = coursePackageId,
                    CourseName = course.CourseName,
                    TotalVideoDuration = totalVideoDuration,
                    TotalWatchTime = totalWatchTime,
                    CompletionPercentage = completionPercentage
                };

                await _unitOfWork.LearnerCourseRepository.UpdateProgressAsync(
                    learnerId,
                    coursePackageId,
                    completionPercentage);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã lấy tiến độ xem video của khóa học thành công.",
                    Data = progressDTO
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lấy tiến độ xem video của khóa học: {ex.Message}"
                };
            }
        }

        public async Task<Course_Content_Item> GetContentItemAsync(int itemId)
        {
            return await _unitOfWork.CourseContentItemRepository.GetByIdAsync(itemId);
        }

        public async Task<ItemTypes> GetItemTypeAsync(int itemTypeId)
        {
            return await _unitOfWork.ItemTypeRepository.GetByIdAsync(itemTypeId);
        }

        public async Task<bool> UpdateContentItemDurationAsync(int itemId, double duration)
        {
            var contentItem = await _unitOfWork.CourseContentItemRepository.GetByIdAsync(itemId);
            if (contentItem == null || contentItem.DurationInSeconds.HasValue)
                return false;

            contentItem.DurationInSeconds = duration;
            await _unitOfWork.CourseContentItemRepository.UpdateAsync(contentItem);
            await _unitOfWork.SaveChangeAsync();
            return true;
        }

        public async Task<ResponseDTO> GetAllCoursePackagesWithDetailsAsync(int learnerId, int coursePackageId)
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

                var course = await _unitOfWork.CourseRepository.GetByIdAsync(coursePackageId);
                if (course == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy khóa học."
                    };
                }

                var coursePackageDetailsList = new List<CoursePackageDetailsDTO>();

                var courseContents = await _unitOfWork.CourseContentRepository.GetQuery()
                    .Where(cc => cc.CoursePackageId == course.CoursePackageId)
                    .ToListAsync();

                var contentDetailsList = new List<CourseContentDetailsDTO>();
                int totalContentItems = 0;

                foreach (var content in courseContents)
                {
                    var contentItems = await _unitOfWork.CourseContentItemRepository.GetQuery()
                        .Where(cci => cci.ContentId == content.ContentId)
                        .ToListAsync();

                    totalContentItems += contentItems.Count;

                    var contentItemProgressList = new List<ContentItemProgressDTO>();

                    foreach (var item in contentItems)
                    {
                        var itemType = await _unitOfWork.ItemTypeRepository.GetByIdAsync(item.ItemTypeId);
                        var progress = await _unitOfWork.LearnerContentProgressRepository
                            .GetByLearnerAndContentItemAsync(learnerId, item.ItemId);

                        bool isLearned = progress != null && progress.IsCompleted;
                        double watchTime = progress?.WatchTimeInSeconds ?? 0;
                        double completionPercentage = 0;

                        if (itemType != null && itemType.ItemTypeName.ToLower().Contains("video") && item.DurationInSeconds.HasValue && item.DurationInSeconds.Value > 0)
                        {
                            completionPercentage = Math.Min(100, (watchTime / item.DurationInSeconds.Value) * 100);
                            isLearned = watchTime >= (item.DurationInSeconds.Value * 0.9);
                        }

                        contentItemProgressList.Add(new ContentItemProgressDTO
                        {
                            ItemId = item.ItemId,
                            ItemDes = item.ItemDes,
                            ItemTypeId = item.ItemTypeId,
                            ItemTypeName = itemType?.ItemTypeName ?? "Unknown",
                            IsLearned = isLearned,
                            DurationInSeconds = item.DurationInSeconds,
                            WatchTimeInSeconds = watchTime,
                            CompletionPercentage = completionPercentage,
                            LastAccessDate = progress?.LastAccessDate
                        });
                    }

                    contentDetailsList.Add(new CourseContentDetailsDTO
                    {
                        ContentId = content.ContentId,
                        Heading = content.Heading,
                        TotalContentItems = contentItems.Count,
                        ContentItems = contentItemProgressList
                    });
                }

                var learnerCourse = await _unitOfWork.LearnerCourseRepository
                    .GetByLearnerAndCourseAsync(learnerId, course.CoursePackageId);

                double progressPercentage = learnerCourse?.CompletionPercentage ?? 0;

                coursePackageDetailsList.Add(new CoursePackageDetailsDTO
                {
                    CoursePackageId = course.CoursePackageId,
                    CourseName = course.CourseName,
                    TotalContents = courseContents.Count,
                    TotalContentItems = totalContentItems,
                    OverallProgressPercentage = progressPercentage,
                    Contents = contentDetailsList
                });

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Course package retrieved successfully.",
                    Data = coursePackageDetailsList[0]
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving course package: {ex.Message}"
                };
            }
        }
        public async Task<bool> RecalculateAllLearnersProgressForCourse(int coursePackageId)
        {
            try
            {
                var learnerCourses = await _unitOfWork.LearnerCourseRepository.GetByCoursePackageIdAsync(coursePackageId);

                if (learnerCourses == null || !learnerCourses.Any())
                    return true;

                foreach (var learnerCourse in learnerCourses)
                {
                    double newPercentage = await CalculateTypeBasedCompletionPercentageAsync(
                        learnerCourse.LearnerId,
                        coursePackageId);

                    await _unitOfWork.LearnerCourseRepository.UpdateProgressAsync(
                        learnerCourse.LearnerId,
                        coursePackageId,
                        newPercentage);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<double> CalculateTypeBasedCompletionPercentageAsync(int learnerId, int coursePackageId)
        {
            var allContentItems = await GetAllCourseContentItemsAsync(coursePackageId);

            if (allContentItems.Count == 0)
                return 0;

            var videoItems = new List<Course_Content_Item>();
            var documentItems = new List<Course_Content_Item>();

            foreach (var item in allContentItems)
            {
                var itemType = await _unitOfWork.ItemTypeRepository.GetByIdAsync(item.ItemTypeId);
                if (itemType == null) continue;

                var typeName = itemType.ItemTypeName.ToLower();
                if (typeName.Contains("video"))
                    videoItems.Add(item);
                else
                    documentItems.Add(item);
            }

            int totalItemCount = allContentItems.Count;
            if (totalItemCount == 0)
                return 0;

            double videoPoints = 0;
            foreach (var video in videoItems)
            {
                var progress = await _unitOfWork.LearnerContentProgressRepository
                    .GetByLearnerAndContentItemAsync(learnerId, video.ItemId);

                if (progress != null && video.DurationInSeconds.HasValue && video.DurationInSeconds.Value > 0)
                {
                    double videoPercentage = Math.Min(100, (progress.WatchTimeInSeconds / video.DurationInSeconds.Value) * 100);
                    videoPoints += videoPercentage / 100;
                }
            }

            double documentPoints = 0;
            foreach (var doc in documentItems)
            {
                var progress = await _unitOfWork.LearnerContentProgressRepository
                    .GetByLearnerAndContentItemAsync(learnerId, doc.ItemId);

                if (progress != null && progress.IsCompleted)
                    documentPoints += 1;
            }

            double totalPoints = videoPoints + documentPoints;
            return Math.Min(100, (totalPoints / totalItemCount) * 100);
        }

        private async Task<double> RecalculateCourseCompletionPercentageAsync(int learnerId, int coursePackageId)
        {
            var allContentItems = await GetAllCourseContentItemsAsync(coursePackageId);
            int totalItems = allContentItems.Count;

            if (totalItems == 0)
                return 0;

            int completedItems = 0;

            foreach (var item in allContentItems)
            {
                var progress = await _unitOfWork.LearnerContentProgressRepository
                    .GetByLearnerAndContentItemAsync(learnerId, item.ItemId);

                var itemType = await _unitOfWork.ItemTypeRepository.GetByIdAsync(item.ItemTypeId);
                if (itemType != null && itemType.ItemTypeName.ToLower().Contains("video") &&
                    progress != null && item.DurationInSeconds.HasValue && item.DurationInSeconds > 0)
                {
                    if (progress.WatchTimeInSeconds >= (item.DurationInSeconds.Value * 0.9))
                        completedItems++;
                }
                else if (progress != null && progress.IsCompleted)
                {
                    completedItems++;
                }
            }

            return Math.Min(100, (double)completedItems / totalItems * 100);
        }
    }
}
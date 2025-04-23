using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Certification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class CertificationService : ICertificationService
    {
        private readonly ICertificationRepository _certificationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IGoogleSheetsService _googleSheetsService;

        public CertificationService(ICertificationRepository certificationRepository,
                                   IUnitOfWork unitOfWork,
                                   IMapper mapper,
                                   IGoogleSheetsService googleSheetsService)
        {
            _certificationRepository = certificationRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _googleSheetsService = googleSheetsService;
        }

        public async Task<List<ResponseDTO>> GetAllCertificationAsync()
        {
            var certificationList = await _unitOfWork.CertificationRepository.GetAllWithDetailsAsync();
            var certificationDtos = _mapper.Map<IEnumerable<CertificationDTO>>(certificationList);

            var responseList = new List<ResponseDTO>();

            foreach (var certificationDto in certificationDtos)
            {
                responseList.Add(new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Certificate list retrieved successfully.",
                    Data = certificationDto
                });
            }
            return responseList;
        }

        public async Task<ResponseDTO> GetCertificationByIdAsync(int certificationId)
        {
            var certification = await _unitOfWork.CertificationRepository.GetByIdWithDetailsAsync(certificationId);
            if (certification == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Certificate not found.",
                };
            }
            var certificationDto = _mapper.Map<CertificationDTO>(certification);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Certificate retrieved successfully.",
                Data = certificationDto
            };
        }

        public async Task<ResponseDTO> CreateCertificationAsync(CreateCertificationDTO createCertificationDTO)
        {
            try
            {
                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(createCertificationDTO.LearnerId);
                if (learner == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Learner not found",
                    };
                }

                var certificationObj = _mapper.Map<Certification>(createCertificationDTO);
                certificationObj.Learner = learner;
                certificationObj.IssueDate = DateTime.Now;

                switch (createCertificationDTO.CertificationType)
                {
                    case CertificationType.OneOnOne:
                    case CertificationType.CenterLearning:
                        if (!createCertificationDTO.LearningRegisId.HasValue)
                        {
                            return new ResponseDTO
                            {
                                IsSucceed = false,
                                Message = "Learning registration ID is required for 1-1 or center learning certificates",
                            };
                        }

                        var registration = await _unitOfWork.LearningRegisRepository
                            .GettWithIncludesAsync(
                                x => x.LearningRegisId == createCertificationDTO.LearningRegisId.Value,
                                "Teacher,Learner,Major"
                            );

                        if (registration == null)
                        {
                            return new ResponseDTO
                            {
                                IsSucceed = false,
                                Message = "Learning registration not found",
                            };
                        }

                        if (registration.LearnerId != createCertificationDTO.LearnerId)
                        {
                            return new ResponseDTO
                            {
                                IsSucceed = false,
                                Message = "The learning registration doesn't belong to this learner",
                            };
                        }

                        bool certificateExists = await _unitOfWork.CertificationRepository
                            .ExistsByLearningRegisIdAsync(createCertificationDTO.LearningRegisId.Value);

                        if (certificateExists)
                        {
                            return new ResponseDTO
                            {
                                IsSucceed = false,
                                Message = "A certificate already exists for this learning registration",
                            };
                        }

                        var schedules = await _unitOfWork.ScheduleRepository
                            .GetWhereAsync(s => s.LearningRegisId == createCertificationDTO.LearningRegisId.Value &&
                                              s.LearnerId == createCertificationDTO.LearnerId);

                        int totalSessions = registration.NumberOfSession;
                        int attendedSessions = schedules.Count(s => s.AttendanceStatus == AttendanceStatus.Present);
                        double attendanceRate = totalSessions > 0 ? (double)attendedSessions / totalSessions * 100 : 0;

                        if (attendanceRate < 75)
                        {
                            return new ResponseDTO
                            {
                                IsSucceed = false,
                                Message = "The learner's attendance rate is below 75%, cannot issue certificate.",
                                Data = new
                                {
                                    AttendanceRate = attendanceRate,
                                    TotalSessions = totalSessions,
                                    AttendedSessions = attendedSessions
                                }
                            };
                        }

                        certificationObj.LearningRegistration = registration;
                        certificationObj.TeacherName = registration.Teacher?.Fullname;
                        certificationObj.Subject = registration.Major?.MajorName;

                        if (string.IsNullOrEmpty(certificationObj.CertificationName))
                        {
                            string mode = createCertificationDTO.CertificationType == CertificationType.OneOnOne ?
                                "One-on-One" : "Center";
                            certificationObj.CertificationName = $"{mode} Learning Certificate - {registration.Major?.MajorName}";
                        }
                        break;

                    default:
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Only OneOnOne and CenterLearning certificate types are supported.",
                        };
                }

                await _unitOfWork.CertificationRepository.AddAsync(certificationObj);
                await _unitOfWork.SaveChangeAsync();

                var certificateDataForSheets = new CertificationDataDTO
                {
                    CertificationId = certificationObj.CertificationId,
                    LearnerName = learner.FullName,
                    LearnerEmail = learner.Account?.Email,
                    CertificationType = certificationObj.CertificationType.ToString(),
                    CertificationName = certificationObj.CertificationName,
                    IssueDate = certificationObj.IssueDate,
                    TeacherName = certificationObj.TeacherName,
                    Subject = certificationObj.Subject
                };

                bool googleSheetsSuccess = false;
                string googleSheetsMessage = "";

                try
                {
                    Console.WriteLine($"Attempting to save certificate {certificationObj.CertificationId} to Google Sheets");
                    googleSheetsSuccess = await _googleSheetsService.SaveCertificationDataAsync(certificateDataForSheets);

                    if (googleSheetsSuccess)
                    {
                        Console.WriteLine($"Successfully saved certificate {certificationObj.CertificationId} to Google Sheets");
                        googleSheetsMessage = "Certificate data was successfully recorded in Google Sheets.";
                    }
                    else
                    {
                        Console.WriteLine($"Failed to save certificate {certificationObj.CertificationId} to Google Sheets - returned false");
                        googleSheetsMessage = "Certificate was created but could not be recorded in Google Sheets.";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving certificate {certificationObj.CertificationId} to Google Sheets: {ex.Message}");
                    googleSheetsMessage = $"Certificate was created but an error occurred when recording in Google Sheets: {ex.Message}";
                }

                var response = new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Certificate created successfully. {googleSheetsMessage}",
                    Data = new
                    {
                        Certificate = _mapper.Map<CertificationDTO>(certificationObj),
                        GoogleSheetsUpdated = googleSheetsSuccess
                    }
                };

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateCertificationAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error creating certificate: {ex.Message}",
                };
            }
        }

        public async Task<ResponseDTO> UpdateCertificationAsync(int certificationId, UpdateCertificationDTO updateCertificationDTO)
        {
            var certificationUpdate = await _unitOfWork.CertificationRepository.GetByIdAsync(certificationId);
            if (certificationUpdate != null)
            {
                certificationUpdate = _mapper.Map(updateCertificationDTO, certificationUpdate);
                await _unitOfWork.CertificationRepository.UpdateAsync(certificationUpdate);
                var result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Certificate updated successfully!"
                    };
                }
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Failed to update certificate!"
                };
            }
            return new ResponseDTO
            {
                IsSucceed = false,
                Message = "Certificate not found!"
            };
        }

        public async Task<ResponseDTO> DeleteCertificationAsync(int certificationId)
        {
            var deleteCertification = await _unitOfWork.CertificationRepository.GetByIdAsync(certificationId);
            if (deleteCertification != null)
            {
                await _unitOfWork.CertificationRepository.DeleteAsync(certificationId);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Certificate deleted successfully"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Certificate with ID {certificationId} not found"
                };
            }
        }

        public async Task<ResponseDTO> GetLearnerCertificationsAsync(int learnerId)
        {
            try
            {
                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learnerId);
                if (learner == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Learner not found"
                    };
                }

                var certifications = await _unitOfWork.CertificationRepository.GetByLearnerIdAsync(learnerId);
                var certificationsDTO = _mapper.Map<List<CertificationDTO>>(certifications);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Learner's certificates retrieved successfully",
                    Data = certificationsDTO
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving learner's certificates: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> IsEligibleForCertificateAsync(int learnerId, CertificationType certificationType, int? learningRegisId = null)
        {
            try
            {
                switch (certificationType)
                {
                    case CertificationType.OneOnOne:
                    case CertificationType.CenterLearning:
                        if (!learningRegisId.HasValue)
                        {
                            return new ResponseDTO
                            {
                                IsSucceed = false,
                                Message = "Learning registration ID is required for learning certificate eligibility check"
                            };
                        }

                        // Use GettWithIncludesAsync to get a single result
                        var registration = await _unitOfWork.LearningRegisRepository
                            .GettWithIncludesAsync(
                                x => x.LearningRegisId == learningRegisId.Value && x.LearnerId == learnerId,
                                "Teacher,Learner,Schedules"
                            );

                        if (registration == null)
                        {
                            return new ResponseDTO
                            {
                                IsSucceed = false,
                                Message = "Learning registration not found for this learner",
                                Data = new { IsEligible = false }
                            };
                        }

                        // Check if the chosen certificate type matches the registration's actual type
                        bool isOneOnOne = registration.ClassId == null;
                        if ((certificationType == CertificationType.OneOnOne && !isOneOnOne) ||
                            (certificationType == CertificationType.CenterLearning && isOneOnOne))
                        {
                            return new ResponseDTO
                            {
                                IsSucceed = false,
                                Message = "Certificate type doesn't match learning registration type",
                                Data = new { IsEligible = false }
                            };
                        }

                        var scheduleMode = certificationType == CertificationType.OneOnOne ?
                            ScheduleMode.OneOnOne : ScheduleMode.Center;

                        var schedules = await _unitOfWork.ScheduleRepository
                            .GetWhereAsync(s => s.LearningRegisId == learningRegisId.Value &&
                                              s.LearnerId == learnerId &&
                                              s.Mode == scheduleMode);

                        int totalSessions = registration.NumberOfSession;
                        int attendedSessions = schedules.Count(s => s.AttendanceStatus == AttendanceStatus.Present);
                        double attendanceRate = totalSessions > 0 ? (double)attendedSessions / totalSessions * 100 : 0;

                        bool attendanceEligible = attendanceRate >= 75;

                        // Check for existing certificate
                        bool certificateExists = await _unitOfWork.CertificationRepository
                            .ExistsByLearningRegisIdAsync(learningRegisId.Value);

                        return new ResponseDTO
                        {
                            IsSucceed = true,
                            Message = attendanceEligible ?
                                (certificateExists ? "Certificate already exists for this registration" : "Learner is eligible for certificate") :
                                "Insufficient attendance for certificate",
                            Data = new
                            {
                                IsEligible = attendanceEligible && !certificateExists,
                                AttendanceRate = attendanceRate,
                                RequiredAttendanceRate = 75,
                                CertificateExists = certificateExists,
                                TotalSessions = totalSessions,
                                AttendedSessions = attendedSessions
                            }
                        };

                    default:
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Only OneOnOne and CenterLearning certificate types are supported.",
                            Data = new { IsEligible = false }
                        };
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error checking certificate eligibility: {ex.Message}",
                    Data = new { IsEligible = false }
                };
            }
        }
    }
}
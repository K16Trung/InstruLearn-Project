using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Certification;
using Microsoft.EntityFrameworkCore;
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

                if (createCertificationDTO.CertificationType != CertificationType.CenterLearning)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Only CenterLearning certificate type is supported.",
                    };
                }

                var certificationObj = new Certification
                {
                    LearnerId = createCertificationDTO.LearnerId,
                    Learner = learner,
                    CertificationType = CertificationType.CenterLearning,
                    IssueDate = DateTime.Now,
                    CertificationName = createCertificationDTO.CertificationName,
                    LearningMode = ScheduleMode.Center
                };

                string teacherName = null;
                string subjectName = null;

                if (createCertificationDTO.ClassId.HasValue)
                {
                    var classId = createCertificationDTO.ClassId.Value;

                    try
                    {
                        Console.WriteLine($"Attempting to get class with ID: {classId}");

                        var classDetail = await _unitOfWork.ClassRepository.GetByIdAsync(classId);

                        if (classDetail != null)
                        {
                            Console.WriteLine($"Class found with ID: {classId}, ClassName: {classDetail.ClassName}, TeacherId: {classDetail.TeacherId}, MajorId: {classDetail.MajorId}");

                            if (classDetail.Teacher != null)
                            {
                                teacherName = classDetail.Teacher.Fullname;
                                Console.WriteLine($"Teacher navigation property loaded successfully. TeacherId: {classDetail.TeacherId}, Name: {teacherName}");
                            }
                            else
                            {
                                Console.WriteLine($"Teacher navigation property is null despite having TeacherId: {classDetail.TeacherId}. Loading teacher separately...");
                                var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(classDetail.TeacherId);
                                if (teacher != null)
                                {
                                    teacherName = teacher.Fullname;
                                    Console.WriteLine($"Teacher loaded separately: ID={teacher.TeacherId}, Name={teacherName}");
                                }
                                else
                                {
                                    Console.WriteLine($"Failed to load teacher with ID: {classDetail.TeacherId}");
                                }
                            }

                            if (classDetail.Major != null)
                            {
                                subjectName = classDetail.Major.MajorName;
                                Console.WriteLine($"Major navigation property loaded successfully. MajorId: {classDetail.MajorId}, Name: {subjectName}");
                            }
                            else
                            {
                                Console.WriteLine($"Major navigation property is null despite having MajorId: {classDetail.MajorId}. Loading major separately...");
                                var major = await _unitOfWork.MajorRepository.GetByIdAsync(classDetail.MajorId);
                                if (major != null)
                                {
                                    subjectName = major.MajorName;
                                    Console.WriteLine($"Major loaded separately: ID={major.MajorId}, Name={subjectName}");
                                }
                                else
                                {
                                    Console.WriteLine($"Failed to load major with ID: {classDetail.MajorId}");
                                }
                            }

                            certificationObj.TeacherName = teacherName ?? "Unknown Teacher";
                            certificationObj.Subject = subjectName ?? "Unknown Subject";
                        }
                        else
                        {
                            Console.WriteLine($"Class with ID {classId} not found");
                            certificationObj.TeacherName = !string.IsNullOrEmpty(createCertificationDTO.TeacherName)
                                ? createCertificationDTO.TeacherName
                                : "Unknown Teacher";

                            certificationObj.Subject = !string.IsNullOrEmpty(createCertificationDTO.Subject)
                                ? createCertificationDTO.Subject
                                : "Unknown Subject";
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error while retrieving class information: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                        }

                        certificationObj.TeacherName = !string.IsNullOrEmpty(createCertificationDTO.TeacherName)
                            ? createCertificationDTO.TeacherName
                            : "Unknown Teacher";

                        certificationObj.Subject = !string.IsNullOrEmpty(createCertificationDTO.Subject)
                            ? createCertificationDTO.Subject
                            : "Unknown Subject";
                    }
                }
                else
                {
                    certificationObj.TeacherName = !string.IsNullOrEmpty(createCertificationDTO.TeacherName)
                        ? createCertificationDTO.TeacherName
                        : "Unknown Teacher";

                    certificationObj.Subject = !string.IsNullOrEmpty(createCertificationDTO.Subject)
                        ? createCertificationDTO.Subject
                        : "Unknown Subject";
                }

                if (string.IsNullOrEmpty(certificationObj.CertificationName))
                {
                    certificationObj.CertificationName = $"Center Learning Certificate - {certificationObj.Subject}";
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
                    Subject = certificationObj.Subject,
                    FileStatus = "N/A",
                    FileLink = "N/A"
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
                if (certificationType != CertificationType.CenterLearning)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Only CenterLearning certificate type is supported.",
                        Data = new { IsEligible = false }
                    };
                }

                if (!learningRegisId.HasValue)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Learning registration ID is required for center learning certificate eligibility check",
                        Data = new { IsEligible = false }
                    };
                }

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

                if (registration.ClassId == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "The provided registration is for one-on-one learning, not center learning",
                        Data = new { IsEligible = false }
                    };
                }

                var schedules = await _unitOfWork.ScheduleRepository
                    .GetWhereAsync(s => s.LearningRegisId == learningRegisId.Value &&
                                      s.LearnerId == learnerId);

                int totalSessions = registration.NumberOfSession;
                int attendedSessions = schedules.Count(s => s.AttendanceStatus == AttendanceStatus.Present);
                double attendanceRate = totalSessions > 0 ? (double)attendedSessions / totalSessions * 100 : 0;

                bool attendanceEligible = attendanceRate >= 75;

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
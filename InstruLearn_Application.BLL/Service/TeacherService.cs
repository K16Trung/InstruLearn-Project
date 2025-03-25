using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Helper;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Feedback;
using InstruLearn_Application.Model.Models.DTO.Syllabus;
using InstruLearn_Application.Model.Models.DTO.Teacher;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class TeacherService : ITeacherService
    {
        private IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly JwtHelper _jwtHelper;

        public TeacherService(IUnitOfWork unitOfWork, IMapper mapper, JwtHelper jwtHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _jwtHelper = jwtHelper;
        }


        // Get All Teachers
        public async Task<List<ResponseDTO>> GetAllTeachersAsync()
        {
            var teacherList = await _unitOfWork.TeacherRepository.GetAllAsync();
            var teacherDtos = _mapper.Map<IEnumerable<TeacherDTO>>(teacherList);

            var responseList = new List<ResponseDTO>();

            foreach (var teacherDto in teacherDtos)
            {
                responseList.Add(new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Teacher retrieved successfully.",
                    Data = teacherDto
                });
            }
            return responseList;
        }

        public async Task<ResponseDTO> CreateTeacherAsync(CreateTeacherDTO createTeacherDTO)
        {
            var response = new ResponseDTO();

            using (var transaction = await _unitOfWork.dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var majors = await _unitOfWork.MajorRepository.GetAllAsync();
                    var validMajorIds = majors.Select(m => m.MajorId).ToList();

                    if (!createTeacherDTO.MajorIds.All(id => validMajorIds.Contains(id)))
                    {
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "One or more Majors not found",
                        };
                    }

                    var accounts = _unitOfWork.AccountRepository.GetFilter(x => x.Email == createTeacherDTO.Email);
                    var existingAccount = accounts.Items.FirstOrDefault();
                    if (existingAccount != null)
                    {
                        response.Message = "Email already exists.";
                        return response;
                    }

                    var account = new Account
                    {
                        AccountId = Guid.NewGuid().ToString(),
                        Username = createTeacherDTO.Username,
                        Email = createTeacherDTO.Email,
                        PasswordHash = HashPassword(createTeacherDTO.Password),
                        PhoneNumber = createTeacherDTO.PhoneNumber,
                        DateOfEmployment = createTeacherDTO.DateOfEmployment,
                        Role = AccountRoles.Teacher,
                        IsActive = AccountStatus.Active,
                        RefreshToken = _jwtHelper.GenerateRefreshToken(),
                        RefreshTokenExpires = DateTime.Now.AddDays(7)
                    };

                    account.Token = _jwtHelper.GenerateJwtToken(account);
                    account.TokenExpires = DateTime.Now.AddHours(1);

                    await _unitOfWork.AccountRepository.AddAsync(account);
                    await _unitOfWork.dbContext.SaveChangesAsync();

                    var teacher = new Teacher
                    {
                        AccountId = account.AccountId,
                        Fullname = createTeacherDTO.Fullname
                    };

                    await _unitOfWork.TeacherRepository.AddAsync(teacher);
                    await _unitOfWork.dbContext.SaveChangesAsync();

                    var teacherMajors = createTeacherDTO.MajorIds
                        .Select(id => new TeacherMajor
                        {
                            TeacherId = teacher.TeacherId,
                            MajorId = id,
                            Status = TeacherMajorStatus.Free
                        })
                        .ToList();

                    _unitOfWork.dbContext.TeacherMajors.AddRange(teacherMajors);
                    await _unitOfWork.dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    response.IsSucceed = true;
                    response.Message = "Teacher created successfully!";
                    return response;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    response.IsSucceed = false;
                    response.Message = $"Error: {ex.InnerException?.Message ?? ex.Message}";
                    return response;
                }
            }
        }

        public async Task<ResponseDTO> DeleteTeacherAsync(int teacherId)
        {
            var response = new ResponseDTO();

            var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherId);
            if (teacher == null)
            {
                response.Message = "Teacher not found.";
                return response;
            }

            // Change status to Banned
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(teacher.AccountId);
            account.IsActive = AccountStatus.Banned;

            var updated = await _unitOfWork.AccountRepository.UpdateAsync(account);

            if (!updated)
            {
                response.Message = "Failed to delete teacher.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Teacher banned successfully!";
            return response;
        }

        // Ban Teacher

        public async Task<ResponseDTO> UnbanTeacherAsync(int teacherId)
        {
            var response = new ResponseDTO();

            var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherId);
            if (teacher == null)
            {
                response.Message = "Teacher not found.";
                return response;
            }

            // Change status to Banned
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(teacher.AccountId);
            account.IsActive = AccountStatus.Active;

            var updated = await _unitOfWork.AccountRepository.UpdateAsync(account);

            if (!updated)
            {
                response.Message = "Failed to unban teacher.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Teacher unban successfully!";
            return response;
        }


        // Get Teacher by Id

        public async Task<ResponseDTO> GetTeacherByIdAsync(int teacherId)
        {
            var response = new ResponseDTO();

            var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherId);
            if (teacher == null)
            {
                response.Message = "Teacher not found.";
                return response;
            }

            var teacherDTO = _mapper.Map<TeacherDTO>(teacher);
            response.IsSucceed = true;
            response.Data = teacherDTO;
            return response;
        }


        // Update Teacher
        public async Task<ResponseDTO> UpdateTeacherAsync(int teacherId, UpdateTeacherDTO updateTeacherDTO)
        {
            var response = new ResponseDTO();

            // Get the teacher by ID
            var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherId);
            if (teacher == null)
            {
                response.Message = "Teacher not found.";
                return response;
            }

            // Map the changes from DTO to the entity
            _mapper.Map(updateTeacherDTO, teacher);

            // Save changes
            var updated = await _unitOfWork.TeacherRepository.UpdateAsync(teacher);

            if (!updated)
            {
                response.Message = "Failed to update teacher.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Teacher updated successfully!";
            return response;
        }


        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password); // Explicit namespace
        }
    }
}

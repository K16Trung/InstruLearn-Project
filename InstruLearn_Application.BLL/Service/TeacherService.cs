using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository;
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


        public async Task<List<ResponseDTO>> GetAllTeachersAsync()
        {
            var teacherList = await _unitOfWork.TeacherRepository
                .GetQuery()
                .Include(t => t.Account)
                .Include(t => t.TeacherMajors)
                    .ThenInclude(tm => tm.Major)
                .ToListAsync();

            var teacherDtos = _mapper.Map<IEnumerable<TeacherDTO>>(teacherList);

            var responseList = new List<ResponseDTO>();

            foreach (var teacherDto in teacherDtos)
            {
                responseList.Add(new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã truy xuất thông tin giáo viên thành công.",
                    Data = teacherDto
                });
            }
            return responseList;
        }

        public async Task<ResponseDTO> GetTeacherByIdAsync(int teacherId)
        {
            var response = new ResponseDTO();

            var teacher = await _unitOfWork.TeacherRepository
                .GetQuery()
                .Include(t => t.Account)
                .Include(t => t.TeacherMajors)
                    .ThenInclude(tm => tm.Major)
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId);

            if (teacher == null)
            {
                response.Message = "Không tìm thấy giáo viên.";
                return response;
            }

            var teacherDTO = _mapper.Map<TeacherDTO>(teacher);
            response.IsSucceed = true;
            response.Data = teacherDTO;
            return response;
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
                            Message = "Một hoặc nhiều chuyên ngành không tồn tại",
                        };
                    }

                    var accountsByEmail = _unitOfWork.AccountRepository.GetFilter(x => x.Email == createTeacherDTO.Email);
                    if (accountsByEmail.Items.Any())
                    {
                        response.Message = "Email đã tồn tại.";
                        return response;
                    }

                    var accountsByUsername = _unitOfWork.AccountRepository.GetFilter(x => x.Username == createTeacherDTO.Username);
                    if (accountsByUsername.Items.Any())
                    {
                        response.Message = "Tên đăng nhập đã tồn tại.";
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
                        IsEmailVerified = true,
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
                    response.Message = "Tạo giáo viên thành công!";
                    return response;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    response.IsSucceed = false;
                    response.Message = $"Lỗi: {ex.InnerException?.Message ?? ex.Message}";
                    return response;
                }
            }
        }

        public async Task<ResponseDTO> UpdateTeacherAsync(int teacherId, UpdateTeacherDTO updateTeacherDTO)
        {
            var response = new ResponseDTO();

            var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherId);
            if (teacher == null)
            {
                response.Message = "Không tìm thấy giáo viên.";
                return response;
            }

            var account = await _unitOfWork.AccountRepository.GetByIdAsync(teacher.AccountId);
            if (account == null)
            {
                response.Message = "Không tìm thấy tài khoản giáo viên.";
                return response;
            }

            teacher.Heading = updateTeacherDTO.Heading;
            teacher.Details = updateTeacherDTO.Details;
            teacher.Links = updateTeacherDTO.Links;

            account.PhoneNumber = updateTeacherDTO.PhoneNumber;
            account.Gender = updateTeacherDTO.Gender;
            account.Address = updateTeacherDTO.Address;
            account.Avatar = updateTeacherDTO.Avatar;

            var teacherUpdated = await _unitOfWork.TeacherRepository.UpdateAsync(teacher);
            var accountUpdated = await _unitOfWork.AccountRepository.UpdateAsync(account);

            if (!teacherUpdated || !accountUpdated)
            {
                response.Message = "Không thể cập nhật thông tin giáo viên.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Cập nhật thông tin giáo viên thành công!";
            return response;
        }

        public async Task<ResponseDTO> UpdateMajorTeacherAsync(int teacherId, UpdateMajorTeacherDTO updateMajorTeacherDTO)
        {
            try
            {
                var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherId);
                if (teacher == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy giáo viên"
                    };
                }

                await using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var currentTeacherMajors = await _unitOfWork.dbContext.TeacherMajors
                        .AsNoTracking()
                        .Where(tm => tm.TeacherId == teacherId)
                        .ToListAsync();

                    _unitOfWork.dbContext.TeacherMajors.RemoveRange(
                        _unitOfWork.dbContext.TeacherMajors.Where(tm => tm.TeacherId == teacherId)
                    );
                    await _unitOfWork.SaveChangeAsync();

                    if (updateMajorTeacherDTO.MajorIds != null && updateMajorTeacherDTO.MajorIds.Any())
                    {
                        var newTeacherMajors = updateMajorTeacherDTO.MajorIds.Select(majorId => new TeacherMajor
                        {
                            TeacherId = teacherId,
                            MajorId = majorId,
                            Status = TeacherMajorStatus.Free
                        });

                        await _unitOfWork.dbContext.TeacherMajors.AddRangeAsync(newTeacherMajors);
                    }

                    await _unitOfWork.SaveChangeAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Cập nhật giáo viên thành công"
                    };
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi cập nhật giáo viên: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> DeleteTeacherAsync(int teacherId)
        {
            var response = new ResponseDTO();

            var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherId);
            if (teacher == null)
            {
                response.Message = "Không tìm thấy giáo viên.";
                return response;
            }

            var account = await _unitOfWork.AccountRepository.GetByIdAsync(teacher.AccountId);
            account.IsActive = AccountStatus.Banned;

            var updated = await _unitOfWork.AccountRepository.UpdateAsync(account);

            if (!updated)
            {
                response.Message = "Không thể xóa giáo viên.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Cấm giáo viên thành công!";
            return response;
        }

        public async Task<ResponseDTO> UnbanTeacherAsync(int teacherId)
        {
            var response = new ResponseDTO();

            var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherId);
            if (teacher == null)
            {
                response.Message = "Không tìm thấy giáo viên.";
                return response;
            }

            var account = await _unitOfWork.AccountRepository.GetByIdAsync(teacher.AccountId);
            account.IsActive = AccountStatus.Active;

            var updated = await _unitOfWork.AccountRepository.UpdateAsync(account);

            if (!updated)
            {
                response.Message = "Không thể bỏ cấm giáo viên.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Bỏ cấm giáo viên thành công!";
            return response;
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public async Task<ResponseDTO> DeleteMajorTeacherAsync(int teacherId, DeleteMajorTeacherDTO deleteMajorTeacherDTO)
        {
            try
            {
                var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherId);
                if (teacher == null)
                {
                    return new ResponseDTO { IsSucceed = false, Message = "Không tìm thấy giáo viên" };
                }

                await using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var teacherMajorsToDelete = await _unitOfWork.dbContext.TeacherMajors
                        .Where(tm => tm.TeacherId == teacherId && deleteMajorTeacherDTO.MajorIds.Contains(tm.MajorId))
                        .ToListAsync();

                    if (!teacherMajorsToDelete.Any())
                    {
                        return new ResponseDTO { IsSucceed = false, Message = "Không tìm thấy chuyên ngành phù hợp cho giáo viên này" };
                    }

                    _unitOfWork.dbContext.TeacherMajors.RemoveRange(teacherMajorsToDelete);
                    await _unitOfWork.SaveChangeAsync();

                    var remainingMajors = await _unitOfWork.dbContext.TeacherMajors
                        .Include(tm => tm.Major)
                        .Where(tm => tm.TeacherId == teacherId)
                        .ToListAsync();

                    await _unitOfWork.CommitTransactionAsync();

                    var response = new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = $"Đã xóa thành công {teacherMajorsToDelete.Count} chuyên ngành từ giáo viên",
                        Data = new
                        {
                            TeacherId = teacher.TeacherId,
                            TeacherName = teacher.Fullname,
                            RemovedMajorIds = teacherMajorsToDelete.Select(tm => tm.MajorId).ToList(),
                            RemainingMajors = remainingMajors.Select(tm => new
                            {
                                MajorId = tm.MajorId,
                                MajorName = tm.Major?.MajorName
                            }).ToList()
                        }
                    };

                    return response;
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi xóa chuyên ngành từ giáo viên: {ex.Message}"
                };
            }
        }
    }
}

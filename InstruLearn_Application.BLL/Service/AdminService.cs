using AutoMapper;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Helper;
using InstruLearn_Application.Model.Models.DTO.Learner;
using InstruLearn_Application.Model.Models.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.Model.Models.DTO.Admin;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO.Teacher;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.BLL.Service.IService;

namespace InstruLearn_Application.BLL.Service
{
    public class AdminService : IAdminService
    {
        private IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly JwtHelper _jwtHelper;

        public AdminService(IUnitOfWork unitOfWork, IMapper mapper, JwtHelper jwtHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _jwtHelper = jwtHelper;
        }
        public async Task<ResponseDTO> GetAllAdminAsync()
        {
            var response = new ResponseDTO();

            var admins = await _unitOfWork.AdminRepository
                .GetQuery()
                .Include(t => t.Account)
                .ToListAsync();

            var adminDTOs = _mapper.Map<IEnumerable<AdminDTO>>(admins);

            response.IsSucceed = true;
            response.Message = "Lấy ra danh sách thành công";
            response.Data = adminDTOs;
            return response;
        }

        public async Task<ResponseDTO> CreateAdminAsync(CreateAdminDTO createAdminDTO)
        {
            var response = new ResponseDTO();

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var accounts = _unitOfWork.AccountRepository.GetFilter(x => x.Email == createAdminDTO.Email);
                var existingAccount = accounts.Items.FirstOrDefault();
                if (existingAccount != null)
                {
                    response.Message = "Email đã tồn tại.";
                    return response;
                }

                var account = new Account
                {
                    AccountId = Guid.NewGuid().ToString(),
                    Username = createAdminDTO.Email,
                    Email = createAdminDTO.Email,
                    PasswordHash = HashPassword(createAdminDTO.Password),
                    Role = AccountRoles.Admin,
                    IsActive = AccountStatus.Active,
                    IsEmailVerified = true,
                    RefreshToken = _jwtHelper.GenerateRefreshToken(),
                    RefreshTokenExpires = DateTime.Now.AddDays(7)
                };

                account.Token = _jwtHelper.GenerateJwtToken(account);
                account.TokenExpires = DateTime.Now.AddHours(1);

                await _unitOfWork.AccountRepository.AddAsync(account);

                var admin = new Admin
                {
                    AccountId = account.AccountId,
                    Fullname = createAdminDTO.Fullname
                };

                await _unitOfWork.AdminRepository.AddAsync(admin);

                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                response.IsSucceed = true;
                response.Message = "Quản trị viên đã tạo thành công!";
                return response;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                response.Message = $"Error creating admin: {ex.Message}";
                return response;
            }
        }

        public async Task<ResponseDTO> GetAdminByIdAsync(int adminId)
        {
            var response = new ResponseDTO();
            var admin = await _unitOfWork.AdminRepository.GetByIdAsync(adminId);
            if (admin == null)
            {
                response.Message = "Không tìm thấy quản trị viên.";
                return response;
            }
            var adminDTO = _mapper.Map<AdminDTO>(admin);
            response.IsSucceed = true;
            response.Message = "Lấy ra danh sách quản trị viên thành công";
            response.Data = adminDTO;
            return response;
        }
        public async Task<ResponseDTO> UpdateAdminAsync(int adminId, UpdateAdminDTO updateAdminDTO)
        {
            var response = new ResponseDTO();

            var admin = await _unitOfWork.AdminRepository.GetByIdAsync(adminId);
            if (admin == null)
            {
                response.Message = "Không tìm thấy quản trị viên.";
                return response;
            }

            _mapper.Map(updateAdminDTO, admin);

            var updated = await _unitOfWork.AdminRepository.UpdateAsync(admin);

            if (!updated)
            {
                response.Message = "Cập nhật quản trị viên thất bại.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Cập nhật quản trị viên thành công!";
            return response;
        }
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}

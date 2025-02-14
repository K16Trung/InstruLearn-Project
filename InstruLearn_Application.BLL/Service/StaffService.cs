using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Helper;
using InstruLearn_Application.Model.Models.DTO.Manager;
using InstruLearn_Application.Model.Models.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.Model.Models.DTO.Staff;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;

namespace InstruLearn_Application.BLL.Service
{
    public class StaffService : IStaffService
    {
        private IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly JwtHelper _jwtHelper;

        public StaffService(IUnitOfWork unitOfWork, IMapper mapper, JwtHelper jwtHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _jwtHelper = jwtHelper;
        }

        public async Task<ResponseDTO> GetAllStaffAsync()
        {
            var response = new ResponseDTO();

            var staffs = await _unitOfWork.StaffRepository
                .GetQuery()
                .Include(t => t.Account)
                .ToListAsync();

            var staffDTOs = _mapper.Map<IEnumerable<StaffDTO>>(staffs);

            response.IsSucceed = true;
            response.Message = "data retrieved successfully";
            response.Data = staffDTOs;
            return response;
        }

        public async Task<ResponseDTO> CreateStaffAsync(CreateStaffDTO createStaffDTO)
        {
            var response = new ResponseDTO();

            var accounts = _unitOfWork.AccountRepository.GetFilter(x => x.Email == createStaffDTO.Email);
            var existingAccount = accounts.Items.FirstOrDefault();
            if (existingAccount != null)
            {
                response.Message = "Email already exists.";
                return response;
            }

            var account = new Account
            {
                AccountId = Guid.NewGuid().ToString(),
                Username = createStaffDTO.Email,
                Email = createStaffDTO.Email,
                PasswordHash = HashPassword(createStaffDTO.Password),
                Role = AccountRoles.Teacher,
                IsActive = AccountStatus.Active,

                RefreshToken = _jwtHelper.GenerateRefreshToken(),
                RefreshTokenExpires = DateTime.Now.AddDays(7)
            };

            account.Token = _jwtHelper.GenerateJwtToken(account);
            account.TokenExpires = DateTime.Now.AddHours(1);

            await _unitOfWork.AccountRepository.AddAsync(account);

            var staff = new Staff
            {
                AccountId = account.AccountId,
                Fullname = createStaffDTO.Fullname
            };

            await _unitOfWork.StaffRepository.AddAsync(staff);

            response.IsSucceed = true;
            response.Message = "Staff created successfully!";
            return response;
        }

        public async Task<ResponseDTO> DeleteStaffAsync(int staffId)
        {
            var response = new ResponseDTO();

            var staff = await _unitOfWork.StaffRepository.GetByIdAsync(staffId);
            if (staff == null)
            {
                response.Message = "Staff not found.";
                return response;
            }

            var account = await _unitOfWork.AccountRepository.GetByIdAsync(staff.AccountId);
            account.IsActive = AccountStatus.Banned;

            var updated = await _unitOfWork.AccountRepository.UpdateAsync(account);

            if (!updated)
            {
                response.Message = "Failed to delete staff.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Staff delete successfully!";
            return response;
        }

        public async Task<ResponseDTO> UnbanStaffAsync(int staffId)
        {
            var response = new ResponseDTO();

            var staff = await _unitOfWork.StaffRepository.GetByIdAsync(staffId);
            if (staff == null)
            {
                response.Message = "Staff not found.";
                return response;
            }

            // Change status to Banned
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(staff.AccountId);
            account.IsActive = AccountStatus.Active;

            var updated = await _unitOfWork.AccountRepository.UpdateAsync(account);

            if (!updated)
            {
                response.Message = "Failed to unban staff.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Staff unban successfully!";
            return response;
        }

        public async Task<ResponseDTO> GetStaffByIdAsync(int staffId)
        {
            var response = new ResponseDTO();
            var staff = await _unitOfWork.StaffRepository.GetByIdAsync(staffId);
            if (staff == null)
            {
                response.Message = "Staff not found.";
                return response;
            }
            var staffDTO = _mapper.Map<StaffDTO>(staff);
            response.IsSucceed = true;
            response.Message = "Staff retrieved successfully";
            response.Data = staffDTO;
            return response;
        }

        public async Task<ResponseDTO> UpdateStaffAsync(int staffId, UpdateStaffDTO updateStaffDTO)
        {
            var response = new ResponseDTO();

            var staff = await _unitOfWork.ManagerRepository.GetByIdAsync(staffId);
            if (staff == null)
            {
                response.Message = "Staff not found.";
                return response;
            }

            _mapper.Map(updateStaffDTO, staff);

            var updated = await _unitOfWork.ManagerRepository.UpdateAsync(staff);

            if (!updated)
            {
                response.Message = "Failed to update staff.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Staff updated successfully!";
            return response;
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password); // Explicit namespace
        }
    }
}

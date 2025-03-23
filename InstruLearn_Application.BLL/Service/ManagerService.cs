using AutoMapper;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Helper;
using InstruLearn_Application.Model.Models.DTO.Manager;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Teacher;
using Microsoft.EntityFrameworkCore;

namespace InstruLearn_Application.BLL.Service
{
    public class ManagerService : IManagerService
    {
        private IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly JwtHelper _jwtHelper;

        public ManagerService(IUnitOfWork unitOfWork, IMapper mapper, JwtHelper jwtHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _jwtHelper = jwtHelper;
        }

        public async Task<ResponseDTO> GetAllManagerAsync()
        {
            var response = new ResponseDTO();

            var managers = await _unitOfWork.ManagerRepository
                .GetQuery()
                .Include(t => t.Account)
                .ToListAsync();

            var managerDTOs = _mapper.Map<IEnumerable<ManagerDTO>>(managers);

            response.IsSucceed = true;
            response.Message = "data retrieved successfully";
            response.Data = managerDTOs;
            return response;
        }

        public async Task<ResponseDTO> GetManagerByIdAsync(int managerId)
        {
            var response = new ResponseDTO();
            var manager = await _unitOfWork.ManagerRepository.GetByIdAsync(managerId);
            if (manager == null)
            {
                response.Message = "Manager not found.";
                return response;
            }
            var managerDTO = _mapper.Map<ManagerDTO>(manager);
            response.IsSucceed = true;
            response.Message = "Manager retrieved successfully";
            response.Data = managerDTO;
            return response;
        }

        public async Task<ResponseDTO> CreateManagerAsync(CreateManagerDTO createManagerDTO)
        {
            var response = new ResponseDTO();

            var accounts = _unitOfWork.AccountRepository.GetFilter(x => x.Email == createManagerDTO.Email);
            var existingAccount = accounts.Items.FirstOrDefault();
            if (existingAccount != null)
            {
                response.Message = "Email already exists.";
                return response;
            }

            var account = new Account
            {
                AccountId = Guid.NewGuid().ToString(),
                Username = createManagerDTO.Username,
                Email = createManagerDTO.Email,
                PasswordHash = HashPassword(createManagerDTO.Password),
                Role = AccountRoles.Manager,
                IsActive = AccountStatus.Active,

                RefreshToken = _jwtHelper.GenerateRefreshToken(),
                RefreshTokenExpires = DateTime.Now.AddDays(7)
            };

            account.Token = _jwtHelper.GenerateJwtToken(account);
            account.TokenExpires = DateTime.Now.AddHours(1);

            await _unitOfWork.AccountRepository.AddAsync(account);

            var manager = new Manager
            {
                AccountId = account.AccountId,
                Fullname = createManagerDTO.Fullname
            };

            await _unitOfWork.ManagerRepository.AddAsync(manager);

            response.IsSucceed = true;
            response.Message = "Manager created successfully!";
            return response;
        }

        public async Task<ResponseDTO> UpdateManagerAsync(int managerId, UpdateManagerDTO updateManagerDTO)
        {
            var response = new ResponseDTO();

            var manager = await _unitOfWork.ManagerRepository.GetByIdAsync(managerId);
            if (manager == null)
            {
                response.Message = "Manager not found.";
                return response;
            }

            _mapper.Map(updateManagerDTO, manager);

            var updated = await _unitOfWork.ManagerRepository.UpdateAsync(manager);

            if (!updated)
            {
                response.Message = "Failed to update manager.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Manager updated successfully!";
            return response;
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password); // Explicit namespace
        }

        public async Task<ResponseDTO> DeleteManagerAsync(int managerId)
        {
            var response = new ResponseDTO();

            var manager = await _unitOfWork.ManagerRepository.GetByIdAsync(managerId);
            if (manager == null)
            {
                response.Message = "Manager not found.";
                return response;
            }

            var account = await _unitOfWork.AccountRepository.GetByIdAsync(manager.AccountId);
            account.IsActive = AccountStatus.Banned;

            var updated = await _unitOfWork.AccountRepository.UpdateAsync(account);

            if (!updated)
            {
                response.Message = "Failed to delete manager.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Manager delete successfully!";
            return response;
        }

        public async Task<ResponseDTO> UnbanManagerAsync(int managerId)
        {
            var response = new ResponseDTO();

            var manager = await _unitOfWork.ManagerRepository.GetByIdAsync(managerId);
            if (manager == null)
            {
                response.Message = "Manager not found.";
                return response;
            }

            // Change status to Banned
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(manager.AccountId);
            account.IsActive = AccountStatus.Active;

            var updated = await _unitOfWork.AccountRepository.UpdateAsync(account);

            if (!updated)
            {
                response.Message = "Failed to unban manager.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Manager unban successfully!";
            return response;
        }
    }
}

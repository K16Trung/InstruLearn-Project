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
            response.Message = "Đã lấy dữ liệu thành công";
            response.Data = managerDTOs;
            return response;
        }

        public async Task<ResponseDTO> GetManagerByIdAsync(int managerId)
        {
            var response = new ResponseDTO();
            var manager = await _unitOfWork.ManagerRepository.GetByIdAsync(managerId);
            if (manager == null)
            {
                response.Message = "Không tìm thấy quản lý.";
                return response;
            }
            var managerDTO = _mapper.Map<ManagerDTO>(manager);
            response.IsSucceed = true;
            response.Message = "Đã lấy thông tin quản lý thành công";
            response.Data = managerDTO;
            return response;
        }

        public async Task<ResponseDTO> CreateManagerAsync(CreateManagerDTO createManagerDTO)
        {
            var response = new ResponseDTO();

            var accountsByEmail = _unitOfWork.AccountRepository.GetFilter(x => x.Email == createManagerDTO.Email);
            if (accountsByEmail.Items.Any())
            {
                response.Message = "Email đã tồn tại.";
                return response;
            }

            var accountsByUsername = _unitOfWork.AccountRepository.GetFilter(x => x.Username == createManagerDTO.Username);
            if (accountsByUsername.Items.Any())
            {
                response.Message = "Tên đăng nhập đã tồn tại.";
                return response;
            }

            var account = new Account
            {
                AccountId = Guid.NewGuid().ToString(),
                Username = createManagerDTO.Username,
                Email = createManagerDTO.Email,
                PasswordHash = HashPassword(createManagerDTO.Password),
                Role = AccountRoles.Manager,
                PhoneNumber = createManagerDTO.PhoneNumber,
                DateOfEmployment = DateOnly.FromDateTime(DateTime.Now),
                IsActive = AccountStatus.Active,
                IsEmailVerified = true,
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
            response.Message = "Tạo quản lý thành công!";
            return response;
        }

        public async Task<ResponseDTO> UpdateManagerAsync(int managerId, UpdateManagerDTO updateManagerDTO)
        {
            var response = new ResponseDTO();

            var manager = await _unitOfWork.ManagerRepository.GetByIdAsync(managerId);
            if (manager == null)
            {
                response.Message = "Không tìm thấy quản lý.";
                return response;
            }

            _mapper.Map(updateManagerDTO, manager);

            var updated = await _unitOfWork.ManagerRepository.UpdateAsync(manager);

            if (!updated)
            {
                response.Message = "Cập nhật quản lý thất bại.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Cập nhật quản lý thành công!";
            return response;
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public async Task<ResponseDTO> DeleteManagerAsync(int managerId)
        {
            var response = new ResponseDTO();

            var manager = await _unitOfWork.ManagerRepository.GetByIdAsync(managerId);
            if (manager == null)
            {
                response.Message = "Không tìm thấy quản lý.";
                return response;
            }

            var account = await _unitOfWork.AccountRepository.GetByIdAsync(manager.AccountId);
            account.IsActive = AccountStatus.Banned;

            var updated = await _unitOfWork.AccountRepository.UpdateAsync(account);

            if (!updated)
            {
                response.Message = "Xóa quản lý thất bại.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Xóa quản lý thành công!";
            return response;
        }

        public async Task<ResponseDTO> UnbanManagerAsync(int managerId)
        {
            var response = new ResponseDTO();

            var manager = await _unitOfWork.ManagerRepository.GetByIdAsync(managerId);
            if (manager == null)
            {
                response.Message = "Không tìm thấy quản lý.";
                return response;
            }

            // Change status to Banned
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(manager.AccountId);
            account.IsActive = AccountStatus.Active;

            var updated = await _unitOfWork.AccountRepository.UpdateAsync(account);

            if (!updated)
            {
                response.Message = "Mở khóa quản lý thất bại.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Mở khóa quản lý thành công!";
            return response;
        }
    }
}

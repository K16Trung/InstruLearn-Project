using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.StaffNotification;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class StaffNotificationService : IStaffNotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<StaffNotificationService> _logger;

        public StaffNotificationService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<StaffNotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseDTO> GetAllTeacherChangeRequestsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all teacher change requests");

                var notifications = await _unitOfWork.StaffNotificationRepository.GetContinueWithTeacherChangeRequestsAsync();

                if (notifications == null || !notifications.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "No teacher change requests found.",
                        Data = new List<StaffNotificationDTO>()
                    };
                }

                var notificationDTOs = _mapper.Map<List<StaffNotificationDTO>>(notifications);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Retrieved {notifications.Count} teacher change requests.",
                    Data = notificationDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving teacher change requests");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving teacher change requests: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> MarkNotificationAsReadAsync(int notificationId)
        {
            try
            {
                await _unitOfWork.StaffNotificationRepository.MarkAsReadAsync(notificationId);
                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Notification marked as read."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error marking notification as read: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> MarkNotificationAsResolvedAsync(int notificationId)
        {
            try
            {
                await _unitOfWork.StaffNotificationRepository.MarkAsResolvedAsync(notificationId);
                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Notification marked as resolved."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as resolved", notificationId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error marking notification as resolved: {ex.Message}"
                };
            }
        }
    }
}

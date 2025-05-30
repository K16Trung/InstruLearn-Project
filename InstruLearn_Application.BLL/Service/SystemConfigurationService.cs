using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class SystemConfigurationService : ISystemConfigurationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SystemConfigurationService> _logger;

        public SystemConfigurationService(IUnitOfWork unitOfWork, ILogger<SystemConfigurationService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ResponseDTO> GetAllConfigurationsAsync()
        {
            try
            {
                var configurations = await _unitOfWork.SystemConfigurationRepository.GetAllAsync();

                if (configurations == null || !configurations.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "No configurations found in the system.",
                        Data = new List<object>()
                    };
                }

                var configData = configurations.Select(c => new
                {
                    Key = c.Key,
                    Value = c.Value,
                    LastUpdated = c.LastUpdated
                });

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "All configurations retrieved successfully.",
                    Data = configData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all system configurations");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving configurations: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ResponseDTO> GetConfigurationAsync(string key)
        {
            try
            {
                var config = await _unitOfWork.SystemConfigurationRepository.GetByKeyAsync(key);

                if (config == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Configuration with key '{key}' not found.",
                        Data = null
                    };
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Configuration retrieved successfully.",
                    Data = new
                    {
                        Key = config.Key,
                        Value = config.Value,
                        LastUpdated = config.LastUpdated
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving configuration with key '{key}'");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving configuration: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ResponseDTO> UpdateConfigurationAsync(string key, string value, string description = null)
        {
            try
            {
                var config = await _unitOfWork.SystemConfigurationRepository.GetByKeyAsync(key);

                if (config == null)
                {
                    // Create new configuration if it doesn't exist
                    config = new SystemConfiguration
                    {
                        Key = key,
                        Value = value,
                        LastUpdated = DateTime.Now
                    };

                    await _unitOfWork.SystemConfigurationRepository.AddAsync(config);
                }
                else
                {
                    // Update existing configuration
                    config.Value = value;
                    config.LastUpdated = DateTime.Now;

                    await _unitOfWork.SystemConfigurationRepository.UpdateAsync(config);
                }

                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Configuration updated successfully.",
                    Data = new
                    {
                        Key = config.Key,
                        Value = config.Value,
                        LastUpdated = config.LastUpdated
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating configuration with key '{key}'");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error updating configuration: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
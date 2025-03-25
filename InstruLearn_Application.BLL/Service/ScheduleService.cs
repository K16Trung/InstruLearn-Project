using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Models.DTO.Schedules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IMapper _mapper;

        public ScheduleService(IScheduleRepository scheduleRepository, IMapper mapper)
        {
            _scheduleRepository = scheduleRepository;
            _mapper = mapper;
        }

        public async Task<List<ScheduleDTO>> GetSchedulesByLearningRegisIdAsync(int learningRegisId)
        {
            var schedules = await _scheduleRepository.GetSchedulesByLearningRegisIdAsync(learningRegisId);
            return _mapper.Map<List<ScheduleDTO>>(schedules);
        }
    }
}

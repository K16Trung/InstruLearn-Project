﻿using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Teacher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ITeacherService
    {
        Task<List<ResponseDTO>> GetAllTeachersAsync();
        Task<ResponseDTO> GetTeacherByIdAsync(int teacherId);
        Task<ResponseDTO> CreateTeacherAsync(CreateTeacherDTO createTeacherDTO);
        Task<ResponseDTO> UpdateTeacherAsync(int teacherId, UpdateTeacherDTO updateTeacherDTO);
        Task<ResponseDTO> UpdateMajorTeacherAsync(int teacherId, UpdateMajorTeacherDTO updateMajorTeacherDTO);
        Task<ResponseDTO> DeleteTeacherAsync(int teacherId);
        Task<ResponseDTO> UnbanTeacherAsync(int teacherId);
        Task<ResponseDTO> DeleteMajorTeacherAsync(int teacherId, DeleteMajorTeacherDTO deleteMajorTeacherDTO);
    }
}

﻿using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.TeacherMajor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ITeacherMajorService
    {
        Task<List<ResponseDTO>> GetAllTeacherMajorAsync();
        Task<ResponseDTO> GetTeacherMajorByIdAsync(int teacherMajorId);
        Task<List<ResponseDTO>> GetTeacherMajorsByTeacherIdAsync(int teacherId);
        Task<ResponseDTO> UpdateBusyStatusTeacherMajorAsync(int teacherMajorId);
        Task<ResponseDTO> UpdateFreeStatusTeacherMajorAsync(int teacherMajorId);
        Task<ResponseDTO> DeleteTeacherMajorAsync(int teacherMajorId);
    }
}

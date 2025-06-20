﻿using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Feedback;
using InstruLearn_Application.Model.Models.DTO.Major;
using InstruLearn_Application.Model.Models.DTO.Syllabus;
using InstruLearn_Application.Model.Models.DTO.TeacherMajor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class TeacherMajorService : ITeacherMajorService
    {
        private readonly ITeacherMajorRepository _teacherMajorRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public TeacherMajorService(ITeacherMajorRepository teacherMajorRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _teacherMajorRepository = teacherMajorRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ResponseDTO>> GetAllTeacherMajorAsync()
        {
            var teacherMajorList = await _unitOfWork.TeacherMajorRepository.GetAllAsync();
            var responseList = new List<ResponseDTO>();

            var teacherGroups = teacherMajorList.GroupBy(tm => tm.TeacherId);

            foreach (var teacherGroup in teacherGroups)
            {
                var firstTeacherMajor = teacherGroup.First();
                var teacher = firstTeacherMajor.Teacher;

                foreach (var teacherMajor in teacherGroup)
                {
                    var teacherMajorDto = new TeacherMajorDTO
                    {
                        TeacherMajorId = teacherMajor.TeacherMajorId,
                        Status = teacherMajor.Status,
                        teacher = new TeacherMajorDetailDTO
                        {
                            TeacherId = teacher.TeacherId,
                            Fullname = teacher.Fullname,
                            Avatar = teacher.Account?.Avatar,
                            Majors = new List<MajorjustNameDTO>
                    {
                        new MajorjustNameDTO
                        {
                            MajorId = teacherMajor.Major.MajorId,
                            MajorName = teacherMajor.Major.MajorName
                        }
                    }
                        }
                    };

                    responseList.Add(new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Lấy ra danh sách chuyên ngành giáo viên thành công.",
                        Data = teacherMajorDto
                    });
                }
            }

            return responseList;
        }

        public async Task<ResponseDTO> GetTeacherMajorByIdAsync(int teacherMajorId)
        {
            var teacherMajor = await _unitOfWork.TeacherMajorRepository.GetByIdAsync(teacherMajorId);
            if (teacherMajor == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy chuyên ngành giáo viên.",
                };
            }
            var teacherMajorDto = _mapper.Map<TeacherMajorDTO>(teacherMajor);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Lấy ra chuyên ngành giáo viên thành công.",
                Data = teacherMajorDto
            };
        }
        public async Task<ResponseDTO> UpdateBusyStatusTeacherMajorAsync(int teacherMajorId)
        {
            var response = new ResponseDTO();

            var updated = await _unitOfWork.TeacherMajorRepository.UpdateStatusAsync(teacherMajorId, TeacherMajorStatus.Busy);

            if (!updated)
            {
                response.Message = "Failed to change status.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Change status teacher major successfully!";
            return response;
        }
        public async Task<ResponseDTO> UpdateFreeStatusTeacherMajorAsync(int teacherMajorId)
        {
            var response = new ResponseDTO();

            var updated = await _unitOfWork.TeacherMajorRepository.UpdateStatusAsync(teacherMajorId, TeacherMajorStatus.Free);

            if (!updated)
            {
                response.Message = "Failed to change status.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Change status teacher major successfully!";
            return response;
        }

        public async Task<List<ResponseDTO>> GetTeacherMajorsByTeacherIdAsync(int teacherId)
        {
            // Get all teacher majors for this teacher
            var teacherMajors = await _unitOfWork.TeacherMajorRepository.GetAllAsync();
            var teacherMajorsForTeacher = teacherMajors.Where(tm => tm.TeacherId == teacherId).ToList();

            if (teacherMajorsForTeacher == null || !teacherMajorsForTeacher.Any())
            {
                return new List<ResponseDTO>
        {
            new ResponseDTO
            {
                IsSucceed = false,
                Message = $"Không tìm thấy chuyên ngành nào cho giáo viên có ID {teacherId}."
            }
        };
            }

            var teacher = teacherMajorsForTeacher.First().Teacher;
            var account = teacher.Account;

            var responseList = new List<ResponseDTO>();

            // Create individual response for each teacher major
            foreach (var teacherMajor in teacherMajorsForTeacher)
            {
                var teacherMajorDto = new TeacherMajorDTO
                {
                    TeacherMajorId = teacherMajor.TeacherMajorId,
                    Status = teacherMajor.Status,
                    teacher = new TeacherMajorDetailDTO
                    {
                        TeacherId = teacher.TeacherId,
                        Fullname = teacher.Fullname,
                        Avatar = account?.Avatar,
                        Heading = teacher.Heading,
                        Details = teacher.Details,
                        Links = teacher.Links,
                        PhoneNumber = account?.PhoneNumber,
                        Gender = account?.Gender,
                        Address = account?.Address,
                        DateOfEmployment = account?.DateOfEmployment,
                        Majors = new List<MajorjustNameDTO>
                {
                    new MajorjustNameDTO
                    {
                        MajorId = teacherMajor.Major.MajorId,
                        MajorName = teacherMajor.Major.MajorName
                    }
                }
                    }
                };

                responseList.Add(new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Lấy ra chuyên ngành giáo viên thành công.",
                    Data = teacherMajorDto
                });
            }

            return responseList;
        }

        public async Task<ResponseDTO> DeleteTeacherMajorAsync(int teacherMajorId)
        {
            var deleteteacherMajor = await _unitOfWork.TeacherMajorRepository.GetByIdAsync(teacherMajorId);
            if (deleteteacherMajor != null)
            {
                await _unitOfWork.TeacherMajorRepository.DeleteAsync(teacherMajorId);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã xóa giáo viên chuyên ngành thành công"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $" Không tìm thấy giáo viên chuyên ngành có ID {teacherMajorId}"
                };
            }
        }
    }
}

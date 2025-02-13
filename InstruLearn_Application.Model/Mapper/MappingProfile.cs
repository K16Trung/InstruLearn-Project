using AutoMapper;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO.Account;
using InstruLearn_Application.Model.Models.DTO.Admin;
using InstruLearn_Application.Model.Models.DTO.Auth;
using InstruLearn_Application.Model.Models.DTO.Learner;
using InstruLearn_Application.Model.Models.DTO.Manager;
using InstruLearn_Application.Model.Models.DTO.Staff;
using InstruLearn_Application.Model.Models.DTO.Teacher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<Account, AccountDTO>().ReverseMap();
            CreateMap<Learner, LearnerDTO>().ReverseMap();
            CreateMap<Teacher, TeacherDTO>().ReverseMap();
            CreateMap<Staff, StaffDTO>().ReverseMap();
            CreateMap<Admin, AdminDTO>().ReverseMap();
            CreateMap<Manager, ManagerDTO>().ReverseMap();
            CreateMap<RegisterDTO, Account>().ReverseMap();

            CreateMap<Account, RegisterDTO>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

            CreateMap<Learner, RegisterDTO>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Account.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));

        }
    }
}

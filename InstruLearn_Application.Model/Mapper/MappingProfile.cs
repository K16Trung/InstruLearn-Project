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
            
            CreateMap<Staff, StaffDTO>().ReverseMap();
            CreateMap<Admin, AdminDTO>().ReverseMap();
            CreateMap<Manager, ManagerDTO>().ReverseMap();

            // Account mapping
            CreateMap<RegisterDTO, Account>().ReverseMap();
            CreateMap<Account, AccountDTO>().ReverseMap();
            CreateMap<Account, RegisterDTO>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

            // Learner mapping
            CreateMap<Learner, LearnerDTO>().ReverseMap();
            CreateMap<Learner, RegisterDTO>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Account.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));


            // Teacher mapping
            CreateMap<Teacher, TeacherDTO>().ReverseMap();
            CreateMap<CreateTeacherDTO, Teacher>().ReverseMap();
            CreateMap<UpdateTeacherDTO, Teacher>().ReverseMap();

            CreateMap<Teacher, TeacherResponseDTO>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Account.CreatedAt))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Account.IsActive))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Account.Role));

            CreateMap<UpdateTeacherDTO, Teacher>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));


        }
    }
}

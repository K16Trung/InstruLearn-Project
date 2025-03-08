using AutoMapper;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO.Account;
using InstruLearn_Application.Model.Models.DTO.Admin;
using InstruLearn_Application.Model.Models.DTO.Auth;
using InstruLearn_Application.Model.Models.DTO.CenterCourse;
using InstruLearn_Application.Model.Models.DTO.Class;
using InstruLearn_Application.Model.Models.DTO.ClassDay;
using InstruLearn_Application.Model.Models.DTO.Course;
using InstruLearn_Application.Model.Models.DTO.Course_Content;
using InstruLearn_Application.Model.Models.DTO.Course_Content_Item;
using InstruLearn_Application.Model.Models.DTO.CourseType;
using InstruLearn_Application.Model.Models.DTO.Curriculum;
using InstruLearn_Application.Model.Models.DTO.Feedback;
using InstruLearn_Application.Model.Models.DTO.FeedbackReplies;
using InstruLearn_Application.Model.Models.DTO.ItemTypes;
using InstruLearn_Application.Model.Models.DTO.Learner;
using InstruLearn_Application.Model.Models.DTO.Major;
using InstruLearn_Application.Model.Models.DTO.Manager;
using InstruLearn_Application.Model.Models.DTO.QnA;
using InstruLearn_Application.Model.Models.DTO.QnAReplies;
using InstruLearn_Application.Model.Models.DTO.Staff;
using InstruLearn_Application.Model.Models.DTO.Teacher;
using InstruLearn_Application.Model.Models.DTO.Wallet;
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

            //Admin mapping
            CreateMap<Admin, AdminDTO>().ReverseMap();
            CreateMap<CreateAdminDTO, Admin>().ReverseMap();
            CreateMap<UpdateAdminDTO, Admin>().ReverseMap();

            CreateMap<Admin, AdminResponseDTO>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Account.CreatedAt))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Account.IsActive))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Account.Role));

            // Manager mapping
            CreateMap<Manager, ManagerDTO>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Account.Username))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Account.IsActive))
                .ReverseMap();
            CreateMap<CreateManagerDTO, Manager>().ReverseMap();
            CreateMap<UpdateManagerDTO, Manager>().ReverseMap();

            CreateMap<Manager, ManagerResponseDTO>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Account.CreatedAt))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Account.IsActive))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Account.Role));

            // Staff mapping
            CreateMap<Staff, StaffDTO>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Account.Username))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Account.IsActive))
                .ReverseMap();
            CreateMap<CreateStaffDTO, Staff>().ReverseMap();
            CreateMap<UpdateStaffDTO, Staff>().ReverseMap();

            CreateMap<Staff, StaffResponseDTO>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Account.CreatedAt))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Account.IsActive))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Account.Role));

            // Learner mapping
            CreateMap<Learner, LearnerDTO>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Account.Username))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Account.IsActive))
                .ReverseMap();
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

            // Course mapping
            CreateMap<Course, CourseDTO>()
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.TypeName))
            .ForMember(dest => dest.CourseContents, opt => opt.MapFrom(src => src.CourseContents))
            .ForMember(dest => dest.FeedBacks, opt => opt.MapFrom(src => src.FeedBacks))
            .ForMember(dest => dest.QnAs, opt => opt.MapFrom(src => src.QnAs));
            CreateMap<Course, GetAllCourseDTO>()
             .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.TypeName))
             .ReverseMap();
            CreateMap<CreateCourseDTO, Course>().ReverseMap();
            CreateMap<UpdateCourseDTO, Course>().ReverseMap();

            CreateMap<Course, CourseDTO>()
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.TypeName));

            // Course Type mapping
            CreateMap<CourseType, CourseTypeDTO>().ReverseMap();
            CreateMap<CreateCourseTypeDTO, CourseType>().ReverseMap();
            CreateMap<UpdateCourseTypeDTO, CourseType>().ReverseMap();

            // Course_Content mapping
            CreateMap<Course_Content, CourseContentDTO>().ReverseMap()
            .ForMember(dest => dest.CourseContentItems, opt => opt.MapFrom(src => src.CourseContentItems))
            .ReverseMap();
            CreateMap<CreateCourseContentDTO, Course_Content>().ReverseMap();
            CreateMap<UpdateCourseContentDTO, Course_Content>().ReverseMap();

            // Item Type mapping
            CreateMap<ItemTypes, ItemTypeDTO>().ReverseMap()
            .ForMember(dest => dest.ItemTypeName, opt => opt.MapFrom(src => src.ItemTypeName));
            CreateMap<CreateItemTypeDTO, ItemTypes>().ReverseMap();
            CreateMap<UpdateItemTypeDTO, ItemTypes>().ReverseMap();

            // Course_Content_Item mapping
            CreateMap<Course_Content_Item, CourseContentItemDTO>()
            .ReverseMap();
            CreateMap<CreateCourseContentItemDTO, Course_Content_Item>().ReverseMap();
            CreateMap<UpdateCourseContentItemDTO, Course_Content_Item>().ReverseMap();

            // 🔹 Feedback Mappings
            CreateMap<FeedBack, FeedbackDTO>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Account.Role.ToString()))
                .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.FeedbackReplies))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreateAt))
                .ReverseMap();

            CreateMap<CreateFeedbackDTO, FeedBack>();
            CreateMap<UpdateFeedbackDTO, FeedBack>();

            // 🔹 FeedbackReplies Mappings
            CreateMap<FeedbackReplies, FeedbackRepliesDTO>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Account.Role.ToString()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreateAt))
                .ReverseMap();

            CreateMap<CreateFeedbackRepliesDTO, FeedbackReplies>();
            CreateMap<UpdateFeedbackRepliesDTO, FeedbackReplies>();

            // 🔹 QnA Mappings
            CreateMap<QnA, QnADTO>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Account.Role.ToString()))
                .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.QnAReplies))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreateAt))
                .ReverseMap();

            CreateMap<CreateQnADTO, QnA>();
            CreateMap<UpdateQnADTO, QnA>();

            // 🔹 QnAReplies Mappings
            CreateMap<QnAReplies, QnARepliesDTO>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Account.Role.ToString()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreateAt))
                .ReverseMap();

            CreateMap<CreateQnARepliesDTO, QnAReplies>();
            CreateMap<UpdateQnARepliesDTO, QnAReplies>();

            // Wallet
            CreateMap<Wallet, WalletDTO>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Learner.FullName))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Learner.Account.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Learner.Account.Email));

            // 🔹 Class Mappings
            CreateMap<Class, ClassDTO>()
                .ForMember(dest => dest.TeacherId, opt => opt.MapFrom(src => src.Teacher.TeacherId))
                .ForMember(dest => dest.CenterCourseId, opt => opt.MapFrom(src => src.CenterCourse.CenterCourseId))
                .ForMember(dest => dest.CenterCourseId, opt => opt.MapFrom(src => src.CenterCourse.CenterCourseName))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ReverseMap();
            CreateMap<CreateClassDTO, Class>();

            // 🔹 ClassDay Mappings
            CreateMap<ClassDay, ClassDayDTO>()
                .ForMember(dest => dest.ClassId, opt => opt.MapFrom(src => src.Class.ClassId))
                .ForMember(dest => dest.Day, opt => opt.MapFrom(src => src.Day))
                .ReverseMap();
            CreateMap<CreateClassDayDTO, ClassDay>();

            // 🔹 CenterCourse Mappings
            CreateMap<Center_Course, CenterCourseDTO>()
                .ForMember(dest => dest.CenterCourseId, opt => opt.MapFrom(src => src.CenterCourseId))
                .ForMember(dest => dest.CenterCourseName, opt => opt.MapFrom(src => src.CenterCourseName))
                .ReverseMap();
            CreateMap<CreateCenterCourseDTO, Center_Course>();

            // 🔹 Curriculum Mappings
            CreateMap<Curriculum, CurriculumDTO>()
                .ForMember(dest => dest.CurriculumId, opt => opt.MapFrom(src => src.CurriculumId))
                .ForMember(dest => dest.CurriculumName, opt => opt.MapFrom(src => src.CurriculumName))
                .ReverseMap();
            CreateMap<CreateCurriculumDTO, Curriculum>();

            //🔹 Major Mappings
            CreateMap<Major, MajorDTO>().ReverseMap();
            CreateMap<CreateMajorDTO, Major>().ReverseMap();
            CreateMap<UpdateMajorDTO, Major>().ReverseMap();
        }
    }
}

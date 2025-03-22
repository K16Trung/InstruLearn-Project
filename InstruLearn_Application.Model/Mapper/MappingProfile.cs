using AutoMapper;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Helper;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO.Account;
using InstruLearn_Application.Model.Models.DTO.Admin;
using InstruLearn_Application.Model.Models.DTO.Auth;
using InstruLearn_Application.Model.Models.DTO.Certification;
using InstruLearn_Application.Model.Models.DTO.Class;
using InstruLearn_Application.Model.Models.DTO.ClassDay;
using InstruLearn_Application.Model.Models.DTO.Course;
using InstruLearn_Application.Model.Models.DTO.Course_Content;
using InstruLearn_Application.Model.Models.DTO.Course_Content_Item;
using InstruLearn_Application.Model.Models.DTO.CourseType;
using InstruLearn_Application.Model.Models.DTO.Feedback;
using InstruLearn_Application.Model.Models.DTO.FeedbackReplies;
using InstruLearn_Application.Model.Models.DTO.ItemTypes;
using InstruLearn_Application.Model.Models.DTO.Learner;
using InstruLearn_Application.Model.Models.DTO.LearningRegistration;
using InstruLearn_Application.Model.Models.DTO.LearningRegistrationType;
using InstruLearn_Application.Model.Models.DTO.Major;
using InstruLearn_Application.Model.Models.DTO.MajorTest;
using InstruLearn_Application.Model.Models.DTO.Manager;
using InstruLearn_Application.Model.Models.DTO.Purchase;
using InstruLearn_Application.Model.Models.DTO.PurchaseItem;
using InstruLearn_Application.Model.Models.DTO.QnA;
using InstruLearn_Application.Model.Models.DTO.QnAReplies;
using InstruLearn_Application.Model.Models.DTO.Staff;
using InstruLearn_Application.Model.Models.DTO.Teacher;
using InstruLearn_Application.Model.Models.DTO.Test_Result;
using InstruLearn_Application.Model.Models.DTO.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static InstruLearn_Application.Model.Models.DTO.PurchaseItem.CreatePurchaseItemDTO;

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
            CreateMap<Teacher, TeacherDTO>()
            .ForMember(dest => dest.Major, opt => opt.MapFrom(src => src.Major))
            .ReverseMap();

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
            CreateMap<Course_Package, CourseDTO>()
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.CourseTypeName))
            .ForMember(dest => dest.CourseContents, opt => opt.MapFrom(src => src.CourseContents))
            .ForMember(dest => dest.FeedBacks, opt => opt.MapFrom(src => src.FeedBacks))
            .ForMember(dest => dest.QnAs, opt => opt.MapFrom(src => src.QnAs));
            CreateMap<Course_Package, GetAllCourseDTO>()
             .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.CourseTypeName))
             .ReverseMap();
            CreateMap<CreateCourseDTO, Course_Package>().ReverseMap();
            CreateMap<UpdateCourseDTO, Course_Package>().ReverseMap();

            CreateMap<Course_Package, CourseDTO>()
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.CourseTypeName));

            // Course Type mapping
            CreateMap<CourseType, CourseTypeDTO>().ReverseMap();
            CreateMap<CreateCourseTypeDTO, CourseType>().ReverseMap();
            CreateMap<UpdateCourseTypeDTO, CourseType>().ReverseMap();

            // Course_Content mapping
            CreateMap<Course_Content, CourseContentDTO>()
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
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ReverseMap();
            CreateMap<CreateClassDTO, Class>();

            //🔹 Major Mappings
            CreateMap<Major, MajorDTO>().ReverseMap();
            CreateMap<CreateMajorDTO, Major>().ReverseMap();
            CreateMap<UpdateMajorDTO, Major>().ReverseMap();

            //

            CreateMap<MajorTest, MajorTestDTO>().ReverseMap();
            CreateMap<CreateMajorTestDTO, MajorTest>().ReverseMap();
            CreateMap<UpdateMajorTestDTO, MajorTest>().ReverseMap();

            //🔹 Learning_Registration Mappings
            CreateMap<Learning_Registration, LearningRegisDTO>().ReverseMap();
                /*.ForMember(dest => dest.LearnerId, opt => opt.MapFrom(src => src.LearnerId))
                .ForMember(dest => dest.ClassId, opt => opt.MapFrom(src => src.ClassId))
                .ForMember(dest => dest.RegisTypeId, opt => opt.MapFrom(src => src.RegisTypeId));*/
            CreateMap<CreateLearningRegisDTO, Learning_Registration>().ReverseMap();

            CreateMap<Learning_Registration, OneOnOneRegisDTO>()
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher.Fullname))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Learner.FullName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Learner.PhoneNumber))
                .ForMember(dest => dest.RegisTypeName, opt => opt.MapFrom(src => src.Learning_Registration_Type.RegisTypeName))
                .ForMember(dest => dest.MajorName, opt => opt.MapFrom(src => src.Major.MajorName))
                .ForMember(dest => dest.LearningDays, opt => opt.MapFrom(src =>
                    src.LearningRegistrationDay.Select(ld => DateTimeHelper.GetDayName((int)ld.DayOfWeek)).ToList()))
                .ForMember(dest => dest.TimeEnd, opt => opt.MapFrom(src => src.TimeStart.AddHours(2))); // Ensure 2-hour session

            CreateMap<UpdateLearningRegisDTO, Learning_Registration>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            //🔹 Learning_Registration_Type Mappings
            CreateMap<Learning_Registration_Type, RegisTypeDTO>().ReverseMap();
            CreateMap<CreateRegisTypeDTO, Learning_Registration_Type>().ReverseMap();

            //🔹 Purchase_Items mapping
            CreateMap<Purchase_Items, PurchaseItemDTO>()
                .ForMember(dest => dest.PurchaseItemId, opt => opt.MapFrom(src => src.PurchaseItemId))
                .ForMember(dest => dest.CoursePackageId, opt => opt.MapFrom(src => src.CoursePackageId))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.CoursePackage, opt => opt.MapFrom(src => src.CoursePackage))
                .ReverseMap();

            CreateMap<CoursePackageItem, Purchase_Items>()
                .ForMember(dest => dest.CoursePackageId, opt => opt.MapFrom(src => src.CoursePackageId))
                .ForMember(dest => dest.PurchaseId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore());

            CreateMap<Course_Package, CourseDetailPurchaseDTO>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.CourseName))
                .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.CourseTypeName))
                .ForMember(dest => dest.CourseDescription, opt => opt.MapFrom(src => src.CourseDescription))
                .ForMember(dest => dest.Headline, opt => opt.MapFrom(src => src.Headline))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.CourseContents, opt => opt.MapFrom(src => src.CourseContents))
                .ReverseMap();

            //🔹 Purchase mapping
            CreateMap<Purchase, PurchaseDTO>()
                .ForMember(dest => dest.PurchaseId, opt => opt.MapFrom(src => src.PurchaseId))
                .ForMember(dest => dest.Learner, opt => opt.MapFrom(src => src.Learner))
                .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => src.PurchaseDate))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.PurchaseItems, opt => opt.MapFrom(src => src.PurchaseItems))
                .ReverseMap();
            CreateMap<Learner, LearnerInfoDTO>()
                .ForMember(dest => dest.LearnerId, opt => opt.MapFrom(src => src.LearnerId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ReverseMap();
            CreateMap<CreatePurchaseDTO, Purchase>().ReverseMap();

            //🔹Certification mapping
            CreateMap<Certification, CertificationDTO>()
                .ForMember(dest => dest.learner, opt => opt.MapFrom(src => src.Learner))
                .ForMember(dest => dest.course, opt => opt.MapFrom(src => src.CoursePackages))
                .ReverseMap();

            CreateMap<Learner, LearnerCertificationDTO>()
                .ForMember(dest => dest.LearnerId, opt => opt.MapFrom(src => src.LearnerId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ReverseMap();

            CreateMap<Course_Package, CourseCertificationDTO>()
                .ForMember(dest => dest.CoursePackageId, opt => opt.MapFrom(src => src.CoursePackageId))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.CourseName))
                .ReverseMap();

            CreateMap<CreateCertificationDTO, Certification>().ReverseMap();
            CreateMap<UpdateCertificationDTO, Certification>().ReverseMap();

            // 🔹Test_result mapping
            CreateMap<Test_Result, TestResultDTO>().ReverseMap();
            CreateMap<CreateTestResultDTO, Test_Result>().ReverseMap();
        }   
    }
}

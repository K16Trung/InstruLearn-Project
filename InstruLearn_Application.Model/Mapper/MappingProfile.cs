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
using InstruLearn_Application.Model.Models.DTO.LearnerClass;
using InstruLearn_Application.Model.Models.DTO.LearnerCourse;
using InstruLearn_Application.Model.Models.DTO.LearningPathSession;
using InstruLearn_Application.Model.Models.DTO.LearningRegistration;
using InstruLearn_Application.Model.Models.DTO.LearningRegistrationType;
using InstruLearn_Application.Model.Models.DTO.LevelAssigned;
using InstruLearn_Application.Model.Models.DTO.Major;
using InstruLearn_Application.Model.Models.DTO.MajorTest;
using InstruLearn_Application.Model.Models.DTO.Manager;
using InstruLearn_Application.Model.Models.DTO.Payment;
using InstruLearn_Application.Model.Models.DTO.Purchase;
using InstruLearn_Application.Model.Models.DTO.PurchaseItem;
using InstruLearn_Application.Model.Models.DTO.QnA;
using InstruLearn_Application.Model.Models.DTO.QnAReplies;
using InstruLearn_Application.Model.Models.DTO.Response;
using InstruLearn_Application.Model.Models.DTO.ResponseType;
using InstruLearn_Application.Model.Models.DTO.ScheduleDays;
using InstruLearn_Application.Model.Models.DTO.Schedules;
using InstruLearn_Application.Model.Models.DTO.Staff;
using InstruLearn_Application.Model.Models.DTO.Syllabus;
using InstruLearn_Application.Model.Models.DTO.SyllbusContent;
using InstruLearn_Application.Model.Models.DTO.Teacher;
using InstruLearn_Application.Model.Models.DTO.TeacherMajor;
using InstruLearn_Application.Model.Models.DTO.Test_Result;
using InstruLearn_Application.Model.Models.DTO.Wallet;
using InstruLearn_Application.Model.Models.DTO.WalletTransaction;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DateTimeHelper;
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
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));

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
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Account.PhoneNumber))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Account.Gender))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Account.Address))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Account.Avatar))
                .ForMember(dest => dest.DateOfEmployment, opt => opt.MapFrom(src => src.Account.DateOfEmployment))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Account.IsActive))
                .ReverseMap();

            CreateMap<Manager, UpdateManagerDTO>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Account.PhoneNumber))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Account.Gender))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Account.Address))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Account.Avatar))
                .ForMember(dest => dest.DateOfEmployment, opt => opt.MapFrom(src => src.Account.DateOfEmployment))
                .ReverseMap();

            CreateMap<CreateManagerDTO, Manager>().ReverseMap();

            CreateMap<Manager, ManagerResponseDTO>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Account.CreatedAt))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Account.IsActive))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Account.Role));

            // Staff mapping
            CreateMap<Staff, StaffDTO>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Account.Username))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Account.PhoneNumber))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Account.Gender))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Account.Address))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Account.Avatar))
                .ForMember(dest => dest.DateOfEmployment, opt => opt.MapFrom(src => src.Account.DateOfEmployment))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Account.IsActive))
                .ReverseMap();

            CreateMap<Staff, UpdateStaffDTO>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Account.PhoneNumber))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Account.Gender))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Account.Address))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Account.Avatar))
                .ForMember(dest => dest.DateOfEmployment, opt => opt.MapFrom(src => src.Account.DateOfEmployment))
                .ReverseMap();
            CreateMap<CreateStaffDTO, Staff>().ReverseMap();

            CreateMap<Staff, StaffResponseDTO>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Account.CreatedAt))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Account.IsActive))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Account.Role));

            // Learner mapping
            CreateMap<Learner, LearnerDTO>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Account.Username))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Account.PhoneNumber))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Account.Gender))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Account.Address))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Account.Avatar))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Account.IsActive))
                .ReverseMap();
            CreateMap<Learner, UpdateLearnerDTO>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Account.PhoneNumber))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Account.Gender))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Account.Address))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Account.Avatar))
                .ReverseMap();
            CreateMap<Learner, RegisterDTO>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Account.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Account.PhoneNumber));


            // Teacher mapping
            CreateMap<Teacher, TeacherDTO>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Account.IsActive))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Account.PhoneNumber))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Account.Gender))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Account.Avatar))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Account.Address))
                .ForMember(dest => dest.DateOfEmployment, opt => opt.MapFrom(src => src.Account.DateOfEmployment))
                .ForMember(dest => dest.Majors, opt => opt.MapFrom(src =>
                    src.TeacherMajors != null
                        ? src.TeacherMajors.Where(tm => tm.Major != null)
                              .Select(tm => new MajorDTO
                              {
                                  MajorId = tm.Major.MajorId,
                                  MajorName = tm.Major.MajorName,
                                  Status = tm.Major.Status
                              }).ToList()
                        : new List<MajorDTO>()))
                .ReverseMap()
                .ForMember(dest => dest.TeacherMajors, opt => opt.Ignore());

            // New mapping for ValidTeacherDTO
            CreateMap<Teacher, ValidTeacherDTO>()
                .ForMember(dest => dest.TeacherId, opt => opt.MapFrom(src => src.TeacherId))
                .ForMember(dest => dest.Fullname, opt => opt.MapFrom(src => src.Fullname))
                .ForMember(dest => dest.Majors, opt => opt.MapFrom(src =>
                    src.TeacherMajors.Select(tm => new MajorDTO
                    {
                        MajorId = tm.Major.MajorId,
                        MajorName = tm.Major.MajorName
                    }).ToList()));

            CreateMap<Teacher, UpdateTeacherDTO>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
                .ForMember(dest => dest.Heading, opt => opt.MapFrom(src => src.Heading))
                .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.Details))
                .ForMember(dest => dest.Links, opt => opt.MapFrom(src => src.Links))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Account.PhoneNumber))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Account.Gender))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Account.Avatar))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Account.Address))
                .ReverseMap();

            CreateMap<Teacher, UpdateMajorTeacherDTO>()
                .ForMember(dest => dest.MajorIds, opt => opt.MapFrom(src => src.TeacherMajors.Select(tm => tm.MajorId).ToList()))
                .ReverseMap();

            CreateMap<CreateTeacherDTO, Teacher>()
                .ForMember(dest => dest.TeacherMajors, opt =>
                    opt.MapFrom(src => src.MajorIds.Select(id => new TeacherMajor { MajorId = id })));

            CreateMap<Teacher, TeacherResponseDTO>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Account.CreatedAt))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Account.IsActive))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Account.Role));

            CreateMap<UpdateTeacherDTO, Teacher>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<DeleteMajorTeacherDTO, TeacherMajor>()
            .ForMember(dest => dest.MajorId, opt => opt.MapFrom(src => src.MajorIds))
            .ReverseMap();

            // Course mapping
            CreateMap<Course_Package, CourseDTO>()
            .ForMember(dest => dest.CourseTypeName, opt => opt.MapFrom(src => src.Type.CourseTypeName))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.CourseContents, opt => opt.MapFrom(src => src.CourseContents))
            .ForMember(dest => dest.FeedBacks, opt => opt.MapFrom(src => src.FeedBacks))
            .ForMember(dest => dest.QnAs, opt => opt.MapFrom(src => src.QnAs));
            CreateMap<Course_Package, GetAllCourseDTO>()
             .ForMember(dest => dest.CourseTypeName, opt => opt.MapFrom(src => src.Type.CourseTypeName))
             .ReverseMap();
            
            CreateMap<Course_Package, CoursePackageTypeDTO>()
             .ForMember(dest => dest.CourseTypeName, opt => opt.MapFrom(src => src.Type.CourseTypeName))
             .ReverseMap();

            CreateMap<CreateCourseDTO, Course_Package>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ReverseMap();
            CreateMap<UpdateCourseDTO, Course_Package>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ReverseMap();

            CreateMap<Course_Package, CourseDTO>()
            .ForMember(dest => dest.CourseTypeName, opt => opt.MapFrom(src => src.Type.CourseTypeName));

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
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ReverseMap();
            CreateMap<CreateCourseContentItemDTO, Course_Content_Item>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ReverseMap();
            CreateMap<UpdateCourseContentItemDTO, Course_Content_Item>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ReverseMap();

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
            .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher.Fullname))
            .ForMember(dest => dest.MajorName, opt => opt.MapFrom(src => src.Major.MajorName))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src =>
                DateTimeHelper.CalculateClassEndDate(
                    src.StartDate,
                    src.totalDays,
                    src.ClassDays.Select(cd => (int)cd.Day).ToList()
                )
            ))
            .ForMember(dest => dest.ClassEndTime, opt => opt.MapFrom(src => src.ClassTime.AddHours(2)))
            .ForMember(dest => dest.ClassDays, opt => opt.MapFrom(src => src.ClassDays));

            CreateMap<CreateClassDTO, Class>()
                .ForMember(dest => dest.SyllabusId, opt => opt.MapFrom(src => src.SyllabusId))
                .ForMember(dest => dest.ClassDays, opt => opt.MapFrom(src => src.ClassDays.Select(day => new Models.ClassDay { Day = day })));

            CreateMap<UpdateClassDTO, Class>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));


            CreateMap<Models.ClassDay, ClassDayDTO>()
                .ForMember(dest => dest.Day, opt => opt.MapFrom(src => src.Day));

            CreateMap<Class, ClassDetailDTO>()
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher != null ? src.Teacher.Fullname : "N/A"))
                .ForMember(dest => dest.MajorName, opt => opt.MapFrom(src => src.Major != null ? src.Major.MajorName : "N/A"))
                .ForMember(dest => dest.SyllabusName, opt => opt.MapFrom(src => src.Syllabus != null ? src.Syllabus.SyllabusName : "N/A"))
                .ForMember(dest => dest.TotalDays, opt => opt.MapFrom(src => src.totalDays))
                .ForMember(dest => dest.ClassDays, opt => opt.Ignore())
                .ForMember(dest => dest.StudentCount, opt => opt.Ignore())
                .ForMember(dest => dest.Students, opt => opt.Ignore());

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
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Learner.Account.PhoneNumber))
                .ForMember(dest => dest.RegisTypeName, opt => opt.MapFrom(src => src.Learning_Registration_Type.RegisTypeName))
                .ForMember(dest => dest.MajorName, opt => opt.MapFrom(src => src.Major.MajorName))
                .ForMember(dest => dest.ResponseTypeId, opt => opt.MapFrom(src => src.Response.ResponseType.ResponseTypeId))
                .ForMember(dest => dest.ResponseTypeName, opt => opt.MapFrom(src => src.Response.ResponseType.ResponseTypeName))
                .ForMember(dest => dest.ResponseDescription, opt => opt.MapFrom(src => src.Response.ResponseName))
                .ForMember(dest => dest.LevelName, opt => opt.MapFrom(src => src.LevelAssigned.LevelName))
                .ForMember(dest => dest.LevelPrice, opt => opt.MapFrom(src => src.LevelAssigned.LevelPrice))
                .ForMember(dest => dest.SyllabusLink, opt => opt.MapFrom(src => src.LevelAssigned.SyllabusLink))
                .ForMember(dest => dest.LearningDays, opt => opt.MapFrom(src =>
                    src.LearningRegistrationDay.Select(ld => DateTimeHelper.GetDayName((int)ld.DayOfWeek)).ToList()))
                .ForMember(dest => dest.TimeEnd, opt => opt.MapFrom(src => src.TimeStart.AddMinutes(src.TimeLearning)));

            CreateMap<UpdateLearningRegisDTO, Learning_Registration>()
                .ForMember(dest => dest.LearningRegisId, opt => opt.Ignore())
                .ForMember(dest => dest.Price, opt => opt.Ignore())
                .ForMember(dest => dest.NumberOfSession, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
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
                .ForMember(dest => dest.CourseTypeName, opt => opt.MapFrom(src => src.Type.CourseTypeName))
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
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Account.PhoneNumber))
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
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Account.PhoneNumber))
                .ReverseMap();

            CreateMap<Course_Package, CourseCertificationDTO>()
                .ForMember(dest => dest.CoursePackageId, opt => opt.MapFrom(src => src.CoursePackageId))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.CourseName))
                .ReverseMap();

            CreateMap<CreateCertificationDTO, Certification>().ReverseMap();
            CreateMap<UpdateCertificationDTO, Certification>().ReverseMap();

            //🔹Test_result mapping
            CreateMap<Test_Result, TestResultDTO>().ReverseMap();
            CreateMap<CreateTestResultDTO, Test_Result>().ReverseMap();

            // 🔹Schedules mapping

            // For DateOnly conversion
            CreateMap<string, DateOnly>()
                .ConvertUsing(new DateOnlyTypeConverter());

            // For TimeOnly conversion
            CreateMap<string, TimeOnly>()
                .ConvertUsing(new TimeOnlyTypeConverter());

            CreateMap<DateOnly, string>()
                .ConvertUsing(d => d.ToString("yyyy-MM-dd"));

            CreateMap<TimeOnly, string>()
                .ConvertUsing(t => t.ToString("HH:mm"));

            CreateMap<Schedules, ScheduleDTO>()
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Class != null ? src.Class.ClassName : null))
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher.Fullname))
                .ForMember(dest => dest.ScheduleDays, opt => opt.MapFrom(src => src.ScheduleDays))
                .ForMember(dest => dest.classDayDTOs, opt => opt.MapFrom(src => src.ClassDays))
                .ForMember(dest => dest.StartDay, opt => opt.MapFrom(src => src.StartDay))

                .ForMember(dest => dest.Mode, opt => opt.MapFrom(src => src.Mode));

            CreateMap<Schedules, ScheduleDTO>()
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher.Fullname))
                .ForMember(dest => dest.LearnerName, opt => opt.MapFrom(src => src.Learner.FullName))
                .ForMember(dest => dest.LearnerAddress, opt => opt.MapFrom(src => src.Learner.Account.Address))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Class.ClassName))
                .ForMember(dest => dest.StartDay, opt => opt.MapFrom(src => src.StartDay))
                .ForMember(dest => dest.TimeStart, opt => opt.MapFrom(src => src.TimeStart.ToString("HH:mm")))
                .ForMember(dest => dest.TimeEnd, opt => opt.MapFrom(src => src.TimeEnd.ToString("HH:mm")))
                .ForMember(dest => dest.ScheduleDays, opt => opt.MapFrom(src => src.ScheduleDays))
                .ForMember(dest => dest.classDayDTOs, opt => opt.MapFrom(src => src.ClassDays))
                .ForMember(dest => dest.RegistrationStartDay, opt => opt.MapFrom(src => src.Registration != null ? src.Registration.StartDay : null))
                .ForMember(dest => dest.LearningRegisId, opt => opt.MapFrom(src => src.LearningRegisId ?? 0))
                .ForMember(dest => dest.AttendanceStatus, opt => opt.MapFrom(src => src.AttendanceStatus))
                .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src =>
                    string.Join(", ", src.ScheduleDays.Select(sd => sd.DayOfWeeks.ToString()))));
                //.ForMember(dest => dest.StartDay, opt => opt.MapFrom(src => src.Class.StartDate.ToString("yyyy-MM-dd")));

            CreateMap<Schedules, OneOnOneRegisDTO>()
            .ForMember(dest => dest.TimeStart, opt => opt.MapFrom(src => src.TimeEnd)) // Adjust as per your properties
            .ForMember(dest => dest.TimeEnd, opt => opt.MapFrom(src => src.TimeEnd))     // Adjust as per your properties
            .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher.Fullname)); // Adjust as per your properties


            CreateMap<CreateScheduleDTO, Schedules>()
                .ForMember(dest => dest.TimeEnd, opt => opt.MapFrom(src => src.TimeEnd))
                .ForMember(dest => dest.ScheduleDays, opt => opt.MapFrom(src => src.ScheduleDays));

            // Add mapping for ClassDay to ScheduleDaysDTO if not already defined
            CreateMap<Models.ClassDay, ScheduleDaysDTO>()
                .ForMember(dest => dest.DayOfWeeks, opt => opt.MapFrom(src => src.Day));

            // Add mapping for ClassDay to ClassDayDTO if not already defined
            CreateMap<Models.ClassDay, ClassDayDTO>()
                .ForMember(dest => dest.Day, opt => opt.MapFrom(src => src.Day));

            // 🔹ScheduleDays mapping
            CreateMap<ScheduleDaysDTO, ScheduleDays>()
                .ForMember(dest => dest.DayOfWeeks, opt => opt.MapFrom(src => src.DayOfWeeks));

            CreateMap<ScheduleDays, ScheduleDaysDTO>()
                .ForMember(dest => dest.DayOfWeeks, opt => opt.MapFrom(src => src.DayOfWeeks));


            //🔹TeacherMajor mapping
            CreateMap<TeacherMajor, TeacherMajorDTO>()
                .ForMember(dest => dest.teacher, opt => opt.MapFrom(src => src.Teacher))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ReverseMap();
            CreateMap<Major, MajorjustNameDTO>()
                .ForMember(dest => dest.MajorId, opt => opt.MapFrom(src => src.MajorId))
                .ForMember(dest => dest.MajorName, opt => opt.MapFrom(src => src.MajorName))
                .ReverseMap();
            CreateMap<Teacher, TeacherMajorDetailDTO>()
                .ForMember(dest => dest.Majors, opt => opt.MapFrom(src => src.TeacherMajors.Select(tm => tm.Major)))
                .ForMember(dest => dest.TeacherId, opt => opt.MapFrom(src => src.TeacherId))
                .ForMember(dest => dest.Fullname, opt => opt.MapFrom(src => src.Fullname))
                .ReverseMap();

            // Syllabus mapping
            CreateMap<Syllabus, SyllabusDTO>().ReverseMap();
            CreateMap<CreateSyllabusDTO, Syllabus>().ReverseMap();

            //🔹LevelAssigned mappings
            CreateMap<LevelAssigned, LevelAssignedDTO>()
                .ForMember(dest => dest.LevelAssignedId, opt => opt.MapFrom(src => src.LevelId))
                .ForMember(dest => dest.LevelName, opt => opt.MapFrom(src => src.LevelName))
                .ForMember(dest => dest.MajorId, opt => opt.MapFrom(src => src.MajorId))
                .ForMember(dest => dest.LevelPrice, opt => opt.MapFrom(src => src.LevelPrice))
                .ReverseMap();

            CreateMap<CreateLevelAssignedDTO, LevelAssigned>().ReverseMap();
            CreateMap<UpdateLevelAssignedDTO, LevelAssigned>().ReverseMap();

            //🔹Response mappings
            CreateMap<Response, ResponseForLearningRegisDTO>()
                .ForMember(dest => dest.ResponseId, opt => opt.MapFrom(src => src.ResponseId))
                .ForMember(dest => dest.ResponseDescription, opt => opt.MapFrom(src => src.ResponseName)) // Changed from ResponseName
                .ForMember(dest => dest.ResponseTypes, opt => opt.MapFrom(src => src.ResponseType));

            CreateMap<CreateResponseDTO, Response>()
                .ForMember(dest => dest.ResponseName, opt => opt.MapFrom(src => src.ResponseDescription)) // Changed from ResponseName
                .ReverseMap();

            CreateMap<UpdateResponseDTO, Response>()
                .ForMember(dest => dest.ResponseName, opt => opt.MapFrom(src => src.ResponseDescription)) // Changed from ResponseName
                .ReverseMap();

            //🔹ResponseType mappings
            CreateMap<ResponseType, ReponseTypeDTO>()
                .ForMember(dest => dest.ResponseTypeId, opt => opt.MapFrom(src => src.ResponseTypeId))
                .ForMember(dest => dest.ResponseTypeName, opt => opt.MapFrom(src => src.ResponseTypeName))
                .ReverseMap();

            CreateMap<CreateResponseTypeDTO, ResponseType>().ReverseMap();
            CreateMap<UpdateResponseTypeDTO, ResponseType>().ReverseMap();

            // payment mapping
            CreateMap<Payment, PaymentDTO>().ReverseMap();

            //wallet transaction mapping
            CreateMap<WalletTransaction, WalletTransactionDTO>()
            .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => src.TransactionType.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.LearnerFullname, opt => opt.MapFrom(src =>
                src.Wallet.Learner != null ? src.Wallet.Learner.FullName : string.Empty));

            //wallet learner_class mapping
            CreateMap<Learner_class, LearnerClassPaymentDTO>().ReverseMap();

            // Syllabus Content mapping
            CreateMap<Syllabus, SyllabusContentDTO>()
                .ForMember(dest => dest.SyllabusId, opt => opt.MapFrom(src => src.SyllabusId))
                .ForMember(dest => dest.SyllabusName, opt => opt.MapFrom(src => src.SyllabusName))
                .ForMember(dest => dest.SyllabusContents, opt => opt.MapFrom(src => src.SyllabusContents));

            CreateMap<Syllabus_Content, SyllabusContentDetailDTO>()
                .ForMember(dest => dest.SyllabusContentId, opt => opt.MapFrom(src => src.SyllabusContentId))
                .ForMember(dest => dest.ContentName, opt => opt.MapFrom(src => src.ContentName));

            CreateMap<Syllabus_Content, SyllabusContentDTO>()
                .ForMember(dest => dest.SyllabusId, opt => opt.MapFrom(src => src.SyllabusId))
                .ForMember(dest => dest.SyllabusName, opt => opt.MapFrom(src => src.Syllabus.SyllabusName))
                .ForMember(dest => dest.SyllabusContents, opt => opt.Ignore());

            CreateMap<CreateSyllabusContentDTO, Syllabus_Content>()
                .ForMember(dest => dest.SyllabusId, opt => opt.MapFrom(src => src.SyllabusId))
                .ForMember(dest => dest.ContentName, opt => opt.MapFrom(src => src.ContentName));

            // Learning Path Session mapping
            CreateMap<LearningPathSession, LearningPathSessionDTO>().ReverseMap();
            CreateMap<LearningPathSessionDTO, LearningPathSession>().ReverseMap();
            CreateMap<CreateLearningPathSessionDTO, LearningPathSession>()
                .ForMember(dest => dest.LearningRegisId, opt => opt.MapFrom(src => src.LearningRegisId));

            // Learner Course mapping
            CreateMap<Learner_Course, LearnerCourseDTO>()
                .ForMember(dest => dest.LearnerName, opt => opt.MapFrom(src => src.Learner.FullName))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.CoursePackage.CourseName));



        }
    }
}

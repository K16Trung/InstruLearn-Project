
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.Repository;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Helper;
using InstruLearn_Application.Model.Mapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.BLL.Service;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.DAL.UoW;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using InstruLearn_Application.Model.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InstruLearn_Application
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])),

                        ValidateIssuer = true, 
                        ValidateAudience = true, 

                        ValidIssuer = builder.Configuration["Jwt:validissuer"],
                        ValidAudience = builder.Configuration["Jwt:validAudience"], 

                        ClockSkew = TimeSpan.Zero // Ensure token expiration is strict
                    };
                });

            builder.Services.AddAuthorization();


            // Add DB
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.Services.AddDbContext<ApplicationDbContext>(option =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DB");
                option.UseSqlServer(connectionString);
            });

            //PayOS
            builder.Services.Configure<PayOSSettings>(builder.Configuration.GetSection("PayOS"));
            builder.Services.AddSingleton<PayOSSettings>(sp => sp.GetRequiredService<IOptions<PayOSSettings>>().Value);

            // Inject app Dependency Injection
            builder.Services.AddScoped<ApplicationDbContext>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<JwtHelper>();
            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<IAdminRepository, AdminRepository>();
            builder.Services.AddScoped<IManagerRepository, ManagerRepository>();
            builder.Services.AddScoped<IStaffRepository, StaffRepository>();
            builder.Services.AddScoped<ILearnerRepository, LearnerRepository>();
            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
            builder.Services.AddScoped<ICourseRepository, CourseRepository>();
            builder.Services.AddScoped<ICourseTypeRepository, CourseTypeRepository>();
            builder.Services.AddScoped<ICourseContentRepository, CourseContentRepository>();
            builder.Services.AddScoped<IItemTypeRepository, ItemTypeRepository>();
            builder.Services.AddScoped<ICourseContentItemRepository, CourseContentItemRepository>();
            builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            builder.Services.AddScoped<IFeedbackRepliesRepository, FeedbackRepliesRepository>();
            builder.Services.AddScoped<IQnARepository, QnARepository>();
            builder.Services.AddScoped<IQnARepliesRepository, QnARepliesRepository>();
            builder.Services.AddScoped<IWalletRepository, WalletRepository>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
            builder.Services.AddScoped<IClassRepository, ClassRepository>();
            builder.Services.AddScoped<IClassDayRepository, ClassDayRepository>();
            builder.Services.AddScoped<IMajorRepository, MajorRepository>();
            builder.Services.AddScoped<IMajorTestRepository, MajorTestRepository>();
            builder.Services.AddScoped<ILearningRegisRepository, LearningRegisRepository>();
            builder.Services.AddScoped<ILearningRegisTypeRepository, LearningRegisTypeRepository>();
            builder.Services.AddScoped<ILearningRegisDayRepository, LearningRegisDayRepository>();
            builder.Services.AddScoped<ISyllabusRepository, SyllabusRepository>();
            builder.Services.AddScoped<ITestResultRepository, TestResultRepository>();
            builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
            builder.Services.AddScoped<IPurchaseItemRepository, PurchaseItemRepository>();
            builder.Services.AddScoped<ICertificationRepository, CertificationRepository>();
            builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
            builder.Services.AddScoped<ITeacherMajorRepository, TeacherMajorRepository>();
            builder.Services.AddScoped<ILevelAssignedRepository, LevelAssignedRepository>();
            builder.Services.AddScoped<IResponseRepository, ResponseRepository>();
            builder.Services.AddScoped<IResponseTypeRepository, ResponseTypeRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IManagerService, ManagerService>();
            builder.Services.AddScoped<IStaffService, StaffService>();
            builder.Services.AddScoped<ILearnerService, LearnerService>();
            builder.Services.AddScoped<ITeacherService, TeacherService>();
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<ICourseTypeService, CourseTypeService>();
            builder.Services.AddScoped<ICourseContentService, CourseContentService>();
            builder.Services.AddScoped<IItemTypeService, ItemTypeService>();
            builder.Services.AddScoped<ICourseContentItemService, CourseContentItemService>();
            builder.Services.AddScoped<IFeedbackService, FeedbackService>();
            builder.Services.AddScoped<IFeedbackRepliesService, FeedbackRepliesService>();
            builder.Services.AddScoped<IQnAService, QnAService>();
            builder.Services.AddScoped<IQnARepliesService, QnARepliesService>();
            builder.Services.AddScoped<IWalletService, WalletService>();
            builder.Services.AddScoped<IPayOSWebhookService, PayOSWebhookService>();
            builder.Services.AddScoped<IClassService, ClassService>();
            builder.Services.AddScoped<IClassDayService, ClassDayService>();
            builder.Services.AddScoped<IMajorService, MajorService>();
            builder.Services.AddScoped<IMajorTestService, MajorTestService>();
            builder.Services.AddScoped<ILearningRegisService, LearningRegisService>();
            builder.Services.AddScoped<ILearningRegisTypeService, LearningRegisTypeService>();
            builder.Services.AddScoped<ISyllabusService, SyllabusService>();
            builder.Services.AddScoped<ITestResultService, TestResultService>();
            builder.Services.AddScoped<IPurchaseService, PurchaseService>();
            builder.Services.AddScoped<IPurchaseItemService, PurchaseItemService>();
            builder.Services.AddScoped<ICertificationService, CertificationService>();
            builder.Services.AddScoped<IScheduleService, ScheduleService>();
            builder.Services.AddScoped<ITeacherMajorService, TeacherMajorService>();
            builder.Services.AddScoped<ILevelAssignedService, LevelAssignedService>();
            builder.Services.AddScoped<IResponseService, ResponseService>();
            builder.Services.AddScoped<IResponseTypeService, ResponseTypeService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IWalletTransactionService, WalletTransactionService>();
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyHeader()
                               .AllowAnyMethod();
                    });
            });


            // Bear
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(option =>
            {
                option.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme, securityScheme: new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter the Bearer Authorization string as following: Bearer Generated-JWT-Token",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            }
                        }, new string[] { }
                    }
                });
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseCors("AllowSpecificOrigin");
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                if (!app.Environment.IsDevelopment())
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "User API");
                    c.RoutePrefix = string.Empty;
                }
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
    public class JsonTimeOnlyConverter : JsonConverter<TimeOnly>
    {
        private const string TimeFormat = "HH:mm:ss";

        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return TimeOnly.ParseExact(reader.GetString(), TimeFormat);
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(TimeFormat));
        }
    }
}

using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Models.Settings;
using API_BANKING_PAYMENT.Respositories;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Services;
using API_BANKING_PAYMENT.Services.IServices;
using API_SmartLibrary.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
namespace API_BANKING_PAYMENT
{
    public class Program
    {
        public static async Task Main(string[] args)   
        {
            var builder = WebApplication.CreateBuilder(args);
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var cloudinarySettings = builder.Configuration.GetSection("CloudinarySettings");

            builder.Services.AddDbContext<BankDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("BankDatabase")));
            
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAutoMapper(typeof(Program));

            //User
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();

            //Bank 
            builder.Services.AddScoped<IBankService, BankService>();
            builder.Services.AddScoped<IBankRepository, BankRepository>();

            //Employee
            builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            builder.Services.AddScoped<IEmployeeService, EmployeeService>();

            //Client
            builder.Services.AddScoped<IClientRepository, ClientRepository>();
            //builder.Services.AddScoped<IClientService, ClientService>();

            //Document
            builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
            builder.Services.AddScoped<IDocumentService, DocumentService>();

            //Settings for 3rd party Api and Service
            builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
            builder.Services.Configure<ReCaptchaSettings>(builder.Configuration.GetSection("ReCaptchaSettings"));
            builder.Services.AddHttpClient(); 
            builder.Services.AddScoped<ReCaptchaService>();


            //Admin
            builder.Services.AddScoped<ISuperAdminService, SuperAdminService>();



            // Add Authentication
            builder.Services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = bool.Parse(jwtSettings["ValidateIssuer"]),
                    ValidateAudience = bool.Parse(jwtSettings["ValidateAudience"]),
                    ValidateLifetime = bool.Parse(jwtSettings["ValidateLifetime"]),
                    ValidateIssuerSigningKey = bool.Parse(jwtSettings["ValidateIssuerSigningKey"]),
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings["Key"]))
                };
            });

            //Seri Logger
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File("Logs/myapp-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
                    builder.Host.UseSerilog(); 


            // Configure JWT to use Swagger
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Banking App"
                });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter Jwt Token Only",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { securityScheme, new string[] { } }
                });

            });

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });


            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<BankDbContext>();
                try
                {
                    var connection = dbContext.Database.GetDbConnection();
                    await connection.OpenAsync();
                    Console.WriteLine(" Successfully Connected to DB: +++++++++++++++++++++++++++++++++++++++++++++++++++" + connection.Database);
                    await connection.CloseAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" Connection failed: +++++++++++++++++++++++++++++++++++++++++++++++++++" + ex.Message);
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Banking App");
                    options.EnablePersistAuthorization();
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}

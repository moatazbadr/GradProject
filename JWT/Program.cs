using JWT.DATA;
using JWT.Model.Settings;
using JWT.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace JWT
{
    //comment 
    // again comment
    // moataz comment
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

          
            // Role Seeding (Admin Role)
            builder.Services.AddScoped<RoleManager<IdentityRole>>();

            // DbContext
            #region Connection string
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
                }); 
            #endregion


            // Identity
            #region UserName handling
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
            })
                  .AddEntityFrameworkStores<ApplicationDbContext>()
                  .AddDefaultTokenProviders(); 
            #endregion

            // Send Email
            #region Send Email
            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
            builder.Services.AddTransient<IMailingServices, MailingService>(); 
            #endregion

            // Session
            #region Session handling
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            #endregion
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5);
                options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);

            });


            // CORS
            #region Adjusting Cors
            builder.Services.AddCors(options =>
               {
                   options.AddDefaultPolicy(builder =>
                   {
                       builder.AllowAnyOrigin() // Specify your frontend URL
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                   });
               }); 
            #endregion
            // JWT
            #region JWT specifications
            builder.Services.AddAuthentication(options =>
               {
                   options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                   options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
               })
               .AddJwtBearer(options =>
               {
                   options.SaveToken = true;
                   options.RequireHttpsMetadata = false;
                   options.TokenValidationParameters = new TokenValidationParameters()
                   {
                       ValidateIssuer = true,
                       ValidIssuer = builder.Configuration["JWT:IssuerIP"],
                       ValidateAudience = true,
                       ValidAudience = builder.Configuration["JWT:AudienceIP"],
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]))
                   };
               }); 
            #endregion

            #region Swagger Setting
            builder.Services.AddSwaggerGen(swagger =>
            {
                // Generate the Swagger documentation with metadata for the API
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "ASP.NET 6 Web API",
                    Description = "EduPlat"
                });

                // Add Security Definition for JWT Authentication
                swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' followed by a space and your token.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
                });

                // Add Security Requirement for Swagger UI to use the Bearer token
                swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
            });
            #endregion

            #region Roles For users
            // Build application
            var app = builder.Build();

            // Seed Admin Role if not already seeded
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var roles = new[] { "Admin", "Doctor", "Student" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Seed Admin User if not exists
                string adminEmail = "Saleh@sci.asu.edu.eg";
                var admin = await userManager.FindByEmailAsync(adminEmail);
                if (admin == null)
                {
                    var adminUser = new ApplicationUser
                    {
                        UserName = adminEmail.Split('@')[0],
                        Email = adminEmail
                    };
                    var result = await userManager.CreateAsync(adminUser, "Saleh@123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }
            } 
            #endregion
         


            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }


    }
}

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace RestfulAPI_FarmTimeManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);



            // ====== JWT options (có thể đọc từ appsettings hoặc Secret Manager) ======
            const string JwtIssuer = "FarmTimeManagement";
            const string JwtAudience = "FarmTimeManagement.Clients";
            var JwtKey = builder.Configuration["Jwt:Key"] ?? "REPLACE_WITH_LONG_RANDOM_SECRET_32+CHARS";





            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
 


            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "API document for Farm Time Management",
                    Version = "V1",
                    Description = "Sprint-2"
                     
                });



                // Add Bearer definition
                var scheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Provide token follow: Bearer {token}"
                };
                c.AddSecurityDefinition("Bearer", scheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement{
        { scheme, new List<string>() } });


            });


            // ====== Authentication + Authorization ======
            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = JwtIssuer,
                        ValidAudience = JwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
                        ClockSkew = TimeSpan.FromMinutes(1) // giảm lệch giờ
                    };
                });

            builder.Services.AddAuthorization();


            // ====== Middleware gắn Staff vào HttpContext.Items["Staff"] ======
            builder.Services.AddScoped<CurrentStaffMiddleware>(); // đăng ký DI





            var app = builder.Build();
            

            app.UseSwagger(); 
            app.UseSwaggerUI(); 

            app.UseHttpsRedirection();

          
            app.UseAuthentication();

         
            app.UseMiddleware<CurrentStaffMiddleware>();


            app.UseAuthorization(); 
            app.MapControllers(); 



            app.Run();
        }
    }
}

// ====== Middleware: lấy StaffId từ claims, load Staff, gắn vào HttpContext.Items ======
public class CurrentStaffMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Nếu đã auth (có Principal & có claim StaffId)
        var user = context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var staffIdClaim = user.FindFirst("staff_id")?.Value;
            if (int.TryParse(staffIdClaim, out var staffId))
            {
                try
                {
                    // Dùng service sẵn có để lấy staff (service của bạn trả về Staff, đã null Password) 
                    var staff = await RestfulAPI_FarmTimeManagement.Services.Sprint1.Tom.StaffsServices.GetStaffById(staffId);
                    context.Items["Staff"] = staff; // <-- Lưu vào Items
                }
                catch { /* tránh chặn pipeline nếu DB lỗi; có thể log */ }
            }
        }
        await next(context);
    }
}





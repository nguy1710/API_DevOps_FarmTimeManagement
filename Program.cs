
namespace RestfulAPI_FarmTimeManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);



            //// 1) Định nghĩa CORS policy cho dev (localhost & 127.0.0.1:5500)
            //const string CorsPolicy = "AllowLocalDev";
            //builder.Services.AddCors(options =>
            //{
            //    options.AddPolicy(name: CorsPolicy, policy =>
            //    {
            //        policy
            //            .WithOrigins(
            //                "http://127.0.0.1:5500",
            //                "http://localhost:5500"
            //            // Thêm origin web production của bạn nếu có, ví dụ:
            //            // "https://your-frontend-domain.com"
            //            )
            //            .AllowAnyHeader()
            //            .AllowAnyMethod();
            //        // Nếu sau này bạn dùng cookie/bearer gửi kèm & cần credentials:
            //        // .AllowCredentials();
            //    });
            //});










            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddTransient<RestfulAPI_FarmTimeManagement.Services.StaffsService>();
            builder.Services.AddTransient<RestfulAPI_FarmTimeManagement.Services.HistoriesService>();



            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "API document for Farm Time Management",
                    Version = "V1",
                    Description = "Sprint-1"
                     
                });
            });


            var app = builder.Build();

            //// Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}




            //// 2) Bật CORS SỚM (trước MapControllers / MapGroup)
            //app.UseCors(CorsPolicy);

        



            app.UseSwagger();

              app.UseSwaggerUI();




            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

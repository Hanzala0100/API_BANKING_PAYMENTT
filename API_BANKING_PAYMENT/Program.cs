using API_BANKING_PAYMENT.Models;
using Microsoft.EntityFrameworkCore;

namespace API_BANKING_PAYMENT
{
    public class Program
    {
        public static async Task Main(string[] args)   
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<BankDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("BankDatabase")));

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<BankDbContext>();
                try
                {
                    var connection = dbContext.Database.GetDbConnection();
                    await connection.OpenAsync();
                    Console.WriteLine("  Connected to DB: +++++++++++++++++++++++++++++++++++++++++++++++++++" + connection.Database);
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
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}

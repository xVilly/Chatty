using Chatty.Hubs;
using Chatty.Filters;
using Microsoft.AspNetCore.SignalR;
using Chatty.Data;
using Chatty.Services;

namespace Chatty
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSqlite<ChatContext>("Data Source=Chatty.db");
            builder.Services.AddScoped<UserService>();
            builder.Services.AddSignalR(hubOptions =>
            {
                hubOptions.AddFilter<AuthorizeUserFilter>();
            });
            builder.Services.AddSingleton<AuthorizeUserFilter>();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();


            app.UseHttpsRedirection();

            app.UseAuthorization();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.MapControllers();
            app.MapHub<ChatHub>("/chatHub");

            app.Run();
        }
    }
}
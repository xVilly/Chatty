using Chatty.Hubs;
using Chatty.Filters;
using Microsoft.AspNetCore.SignalR;
using Chatty.Data;

namespace Chatty
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSqlite<ChatContext>("Data Source=Chatty.db");
            builder.Services.AddSignalR(hubOptions =>
            {
                hubOptions.AddFilter<AuthorizationFilter>();
            });
            builder.Services.AddSingleton<AuthorizationFilter>();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();


            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            app.MapHub<ChatHub>("/chatHub");

            app.Run();
        }
    }
}
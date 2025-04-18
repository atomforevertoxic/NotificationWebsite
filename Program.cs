using FluentEmail.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NotificationWebsite.Data;
using NotificationWebsite.Services;
using System.Net;
using System.Net.Mail;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();


builder.Services.AddControllers();


builder.Services.AddDbContext<WebDbContext>(options =>
    options.UseInMemoryDatabase("WebsiteDb"));

builder.Services.AddScoped<UserService>();

var configuration = builder.Configuration;
var emailSettings = configuration.GetSection("EmailSettings");


builder.Services.AddFluentEmail(emailSettings["DefaultFromEmail"])
    .AddSmtpSender(
        emailSettings["SMTPSettings:Host"],
        emailSettings.GetValue<int>("Port"),
        emailSettings["UserName"],
        emailSettings["Password"])
    .AddRazorRenderer();


builder.Services.AddTransient<EmailService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();


app.MapControllers(); 
app.MapRazorPages();  

app.Run();
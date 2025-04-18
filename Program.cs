using FluentEmail.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NotificationWebsite.Data;
using NotificationWebsite.Services;
using NotificationWebsite.Extensions;
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

var defaultFromEmail = emailSettings["DefaultFromEmail"];
var host = emailSettings["SMTPSettings:Host"];
var port = emailSettings.GetValue<int>("Port");
var userName = emailSettings["UserName"];
var password = emailSettings["Password"];

builder.Services.AddFluentEmail(defaultFromEmail)
    .AddSmtpSender(host, port, userName, password)
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
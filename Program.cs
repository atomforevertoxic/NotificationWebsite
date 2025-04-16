using Microsoft.EntityFrameworkCore;
using NotificationWebsite.Data;
using NotificationWebsite.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();


builder.Services.AddControllers();


builder.Services.AddDbContext<WebDbContext>(options =>
    options.UseInMemoryDatabase("WebsiteDb"));

builder.Services.AddScoped<UserService>();

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
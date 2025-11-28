using Microsoft.EntityFrameworkCore;
using SmartMotoRental.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<SmartMotoRentalContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Seed data
// Sử dụng scope để lấy dịch vụ cần thiết
using (var scope = app.Services.CreateScope())
{
    // Lấy dịch vụ cần thiết
    var services = scope.ServiceProvider;
    try
    {
        // Lấy context của database
        var context = services.GetRequiredService<SmartMotoRentalContext>();
        // Seed data
        await SeedData.SeedAsync(context);
    }
    catch (Exception ex)
    {
        // Lấy logger để log lỗi
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}


// Cấu hình đường dẫn yêu cầu HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


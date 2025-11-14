using Microsoft.EntityFrameworkCore;
using RetailappPOE.Data;
using RetailappPOE.Services;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Register services
builder.Services.AddSingleton<TableStorageService>(provider =>
    new TableStorageService(
        configuration.GetConnectionString("AzureTableStorage"),
        "Customers"
    ));
builder.Services.AddSingleton<BlobService>(provider =>
    new BlobService(configuration.GetConnectionString("AzureBlobStorage")));
builder.Services.AddSingleton<FilesService>(provider =>
    new FilesService(configuration.GetConnectionString("AzureStorage"), "fileshare"));
builder.Services.AddSingleton<QueueService>();

// EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("AzureSQL")));

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(2);
});

// ADD THIS LINE
builder.Services.AddScoped<IServiceScopeFactory>(provider => provider.GetRequiredService<IServiceScopeFactory>());

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
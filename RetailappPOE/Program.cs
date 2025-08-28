using RetailappPOE.Services;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllersWithViews();

// For Customers
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



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
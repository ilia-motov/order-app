using Microsoft.AspNetCore.ResponseCompression;
using OrderApp.Server;
using OrderApp.Server.DataAcsess;
using TanvirArjel.EFCore.GenericRepository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<OrderDbContext>();
builder.Services.AddScoped<IDataSeedingService, DataSeedingService>();
builder.Services.AddGenericRepository<OrderDbContext>();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

var app = builder.Build();

app.Services
    .CreateScope()
    .ServiceProvider
    .GetRequiredService<IDataSeedingService>()
    .SeedDatabase();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.UseErrorInterceptorMiddleware();

app.Run();

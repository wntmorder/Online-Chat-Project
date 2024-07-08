using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OnlineChat.Data;
using OnlineChat.Services;
using System.Text.Json.Serialization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database context
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add connection configuration
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => // CookieAuthenticationOptions
                    options.LoginPath = new PathString("/Account/Login"));

// Read encryption settings from configuration
IConfigurationSection encryptionSettings = builder.Configuration.GetSection("EncryptionSettings");
string? key = encryptionSettings["Key"];
string? iv = encryptionSettings["IV"];

// Validate that key and IV are not null or empty
if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(iv))
{
    throw new InvalidOperationException("Encryption settings (Key and IV) must be specified in the configuration.");
}

// Add EncryptionService
builder.Services.AddSingleton(new EncryptionService(key, iv));

WebApplication app = builder.Build();

// Apply migrations at startup
using (IServiceScope scope = app.Services.CreateScope())
{
    ChatDbContext dbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

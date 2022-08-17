using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Solar.Common.Roles;
using Solar.Data;
using Solar.Services.Token;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add cors
builder.Services.AddCors(options => options.AddDefaultPolicy(
    policy => policy
        .WithOrigins("http://localhost:8080")
        .AllowCredentials()
        .AllowAnyHeader()
        .AllowAnyMethod()
));

builder.Services.AddControllers();

builder.Services.AddDbContext<SolarDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
}).AddRoles<IdentityRole>().AddEntityFrameworkStores<SolarDbContext>();

// Add JWTs
builder.Services.AddAuthentication(auth =>
{
    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies[builder.Configuration["Jwt:CookieName"]];
            return Task.CompletedTask;
        },
    };
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add token service
builder.Services.AddTransient<ITokenService, TokenService>();

var app = builder.Build();

// Ensure database is migrated and seeded with any required data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    // Migrate the database
    var db = services.GetRequiredService<SolarDbContext>();
    db.Database.Migrate();

    // Add the roles
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleManager.RoleExistsAsync(Roles.Admin))
    {
        await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
    }
    if (!await roleManager.RoleExistsAsync(Roles.User))
    {
        await roleManager.CreateAsync(new IdentityRole(Roles.User));
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

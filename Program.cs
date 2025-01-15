using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Net.Http.Headers;
using RentalManagementSystem.Services;
using AutoMapper;
using RentalManagementSystem;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());  // Changed this line
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor(); 
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<RentalManagementContext>();
builder.Services.AddScoped<ILandlordDashboardService, LandlordDashboardService>();
builder.Services.AddScoped<ILeaseDocumentService, LeaseDocumentService>();
builder.Services.AddScoped<IUtilityService, UtilityService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ITenantDashboardService, TenantDashboardService>();
builder.Services.AddScoped<IBankService, BankService>();
builder.Services.AddScoped<IProfileService, ProfileService>();

// Add session support
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
	options.Cookie.HttpOnly = true; // Ensures that the cookie is accessible only through the HTTP request
	options.Cookie.IsEssential = true; // Required for session cookies to function properly
});

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
	c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
	{
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Description = "Please enter JWT with Bearer into field",
		Name = "Authorization",
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
	});
	c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
	{
		{
			new Microsoft.OpenApi.Models.OpenApiSecurityScheme
			{
				Reference = new Microsoft.OpenApi.Models.OpenApiReference
				{
					Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			Array.Empty<string>()
		}
	});
});

// Replace your existing authentication configuration with this:
builder.Services.AddAuthentication(options => 
{
	options.DefaultScheme = "JWT_OR_COOKIE";
	options.DefaultChallengeScheme = "JWT_OR_COOKIE";
})
.AddCookie(options =>
{
	options.LoginPath = "/Auth/Login";
	options.LogoutPath = "/Auth/Logout";
	options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = builder.Configuration["Jwt:Issuer"],
		ValidAudience = builder.Configuration["Jwt:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
	};
})
.AddPolicyScheme("JWT_OR_COOKIE", "JWT_OR_COOKIE", options =>
{
	options.ForwardDefaultSelector = context =>
	{
		string authorization = context.Request.Headers[HeaderNames.Authorization];
		if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
			return JwtBearerDefaults.AuthenticationScheme;
		
		return CookieAuthenticationDefaults.AuthenticationScheme;
	};
});

// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("LandlordOnly", policy =>
		policy.RequireRole("Landlord"));
	options.AddPolicy("TenantOnly", policy =>
		policy.RequireRole("Tenant"));
});

// Configure DbContext
builder.Services.AddDbContext<RentalManagementContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure CORS
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", builder =>
		builder
			.AllowAnyMethod()
			.AllowAnyHeader()
			.AllowCredentials() // Important for authentication
			.SetIsOriginAllowed(_ => true)); // Be careful with this in production
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
	app.UseSwagger();
	app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RentalManagement API v1"));
}
else
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

// Correct middleware order is important!
// Add session middleware
app.UseSession();

// CORS must come before authentication
app.UseCors("AllowAll");

// Routing must come before authentication and authorization
app.UseRouting();

// Authentication must come before authorization
app.UseAuthentication();
app.UseAuthorization();

// EndpointMiddleware
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

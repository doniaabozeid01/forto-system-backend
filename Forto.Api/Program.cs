
using System.Text;
using Forto.Api.Common;
using Forto.Api.Middleware;
using Forto.Api.Seed;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.Abstractions.Services.Bookings;
using Forto.Application.Abstractions.Services.Bookings.Admin;
using Forto.Application.Abstractions.Services.Bookings.Cashier;
using Forto.Application.Abstractions.Services.Bookings.Closing;
using Forto.Application.Abstractions.Services.Cars;
using Forto.Application.Abstractions.Services.Catalogs.Categories;
using Forto.Application.Abstractions.Services.Catalogs.Recipes;
using Forto.Application.Abstractions.Services.Catalogs.Service;
using Forto.Application.Abstractions.Services.Clients;
using Forto.Application.Abstractions.Services.Dashboard;
using Forto.Application.Abstractions.Services.Employees;
using Forto.Application.Abstractions.Services.Employees.Tasks;
using Forto.Application.Abstractions.Services.EmployeeServices;
using Forto.Application.Abstractions.Services.Inventory.Materials;
using Forto.Application.Abstractions.Services.Inventory.MaterialsCheck;
using Forto.Application.Abstractions.Services.Inventory.Products;
using Forto.Application.Abstractions.Services.Invoices;
using Forto.Application.Abstractions.Services.Ops.Products;
using Forto.Application.Abstractions.Services.Ops.Products.StockMovement;
using Forto.Application.Abstractions.Services.Ops.Stock;
using Forto.Application.Abstractions.Services.Ops.Stock.StockMovement;
using Forto.Application.Abstractions.Services.Ops.Usage;
using Forto.Application.Abstractions.Services.Schedule;
using Forto.Application.Abstractions.Services.Shift;
using Forto.Domain.Entities.Identity;
using Forto.Infrastructure.Data;
using Forto.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;

namespace Forto.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);



            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
                })
                .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            k => k.Key,
                            v => v.Value!.Errors
                            .Select(e =>
                                !string.IsNullOrWhiteSpace(e.ErrorMessage)
                                    ? e.ErrorMessage
                                    : (e.Exception?.Message ?? "Invalid value"))
                            .ToArray()

                        );

                    var traceId = context.HttpContext.TraceIdentifier;

                    var response = ApiResponse<object>.Fail(
                        message: "Validation failed",
                        errors: errors,
                        traceId: traceId
                    );

                    return new BadRequestObjectResult(response);
                };
            });



            // Add services to the container.
            builder.Services.AddDbContext<FortoDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });


            builder.Services
                .AddIdentityCore<ApplicationUser>(options =>
                {
                    options.User.RequireUniqueEmail = false;

                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<FortoDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            var jwt = builder.Configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt["Issuer"],
                        ValidateAudience = true,
                        ValidAudience = jwt["Audience"],
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(2)
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IShiftService, ShiftService>();
            builder.Services.AddScoped<IEmployeeService, EmployeeService>();
            builder.Services.AddScoped<IEmployeeScheduleService, EmployeeScheduleService>();
            builder.Services.AddScoped<IClientService, ClientService>();
            builder.Services.AddScoped<ICarService, CarService>();
            builder.Services.AddScoped<ICatalogService, CatalogService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IEmployeeCapabilityService, EmployeeCapabilityService>();
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<IEmployeeTaskService, EmployeeTaskService>();
            builder.Services.AddScoped<IInvoiceService, InvoiceService>();
            // ? BookingService ???? ????? InvoiceService
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<IBookingAdminService, BookingAdminService>();
            builder.Services.AddScoped<IMaterialService, MaterialService>();
            builder.Services.AddScoped<IBranchStockService, BranchStockService>();
            builder.Services.AddScoped<IServiceRecipeService, ServiceRecipeService>();
            builder.Services.AddScoped<IMaterialsCheckService, MaterialsCheckService>();
            builder.Services.AddScoped<IBookingItemMaterialsService, BookingItemMaterialsService>();
            builder.Services.AddScoped<IBookingClosingService, BookingClosingService>();
            builder.Services.AddScoped<IStockMovementService, StockMovementService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IBranchProductStockService, BranchProductStockService>();
            builder.Services.AddScoped<IProductStockMovementService, ProductStockMovementService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IBookingLifecycleService, BookingLifecycleService>();
            builder.Services.AddScoped<IBookingItemOpsService, BookingItemOpsService>();








            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();



            // Middleware registration
            builder.Services.AddTransient<ExceptionHandlingMiddleware>();


            // ? CORS Policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFortoOrigins", policy =>
                    policy.WithOrigins(
                            "http://localhost:4200",
                            "https://fortolaundry.com"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());

                options.AddPolicy("ContactFormOnly", p => p
                    .WithOrigins("https://doniaabozeid01.github.io") // **??????? ???** ???? ??????
                    .WithHeaders("Content-Type")
                    .WithMethods("POST"));

            });




            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseCors("AllowFortoOrigins");

            await IdentitySeeder.SeedRolesAsync(app.Services);

            app.MapControllers();

            app.Run();
        }
    }
}



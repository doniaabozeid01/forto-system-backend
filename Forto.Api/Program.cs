
using Forto.Api.Common;
using Forto.Api.Middleware;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.Abstractions.Services.Bookings;
using Forto.Application.Abstractions.Services.Bookings.Admin;
using Forto.Application.Abstractions.Services.Cars;
using Forto.Application.Abstractions.Services.Catalogs.Categories;
using Forto.Application.Abstractions.Services.Catalogs.Service;
using Forto.Application.Abstractions.Services.Clients;
using Forto.Application.Abstractions.Services.Employees;
using Forto.Application.Abstractions.Services.Employees.Tasks;
using Forto.Application.Abstractions.Services.EmployeeServices;
using Forto.Application.Abstractions.Services.Invoices;
using Forto.Application.Abstractions.Services.Schedule;
using Forto.Application.Abstractions.Services.Shift;
using Forto.Infrastructure.Data;
using Forto.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Forto.Api
{
    public class Program
    {
        public static void Main(string[] args)
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


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Middleware registration
            builder.Services.AddTransient<ExceptionHandlingMiddleware>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.UseMiddleware<ExceptionHandlingMiddleware>();


            app.MapControllers();

            app.Run();
        }
    }
}


















//using Forto.Api.Common;
//using Forto.Api.Middleware;
//using Forto.Application.Abstractions.Repositories;
//using Forto.Infrastructure.Data;
//using Forto.Infrastructure.UnitOfWork;
//using Microsoft.AspNetCore.Mvc;

//var builder = WebApplication.CreateBuilder(args);

//// Logging (????????? ???? ??????)
//// builder.Logging.ClearProviders(); // ???????
//// builder.Logging.AddConsole();     // ???? ?????? ????????


//builder.Services.AddDbContext<FortoDbContext>(options =>
//{
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
//});

//builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


//builder.Services.AddControllers()
//    .ConfigureApiBehaviorOptions(options =>
//    {
//        options.InvalidModelStateResponseFactory = context =>
//        {
//            var errors = context.ModelState
//                .Where(x => x.Value?.Errors.Count > 0)
//                .ToDictionary(
//                    k => k.Key,
//                    v => v.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
//                );

//            var traceId = context.HttpContext.TraceIdentifier;

//            var response = ApiResponse<object>.Fail(
//                message: "Validation failed",
//                errors: errors,
//                traceId: traceId
//            );

//            return new BadRequestObjectResult(response);
//        };
//    });

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//// Middleware registration
//builder.Services.AddTransient<ExceptionHandlingMiddleware>();

//var app = builder.Build();

//app.UseSwagger();
//app.UseSwaggerUI();

//app.UseMiddleware<ExceptionHandlingMiddleware>();

//app.MapControllers();
//app.Run();

using Forto.Application.Abstractions.Repositories;
using Forto.Domain.Entities.Catalog;
using Forto.Domain.Entities.Employees;
using Forto.Domain.Entities.Inventory;
using Forto.Domain.Entities.Ops;
using Forto.Domain.Enum;
using Microsoft.Extensions.Logging;

namespace Forto.Api.Seed
{
    /// <summary>بيانات أولية للتجربة: فرع، تصنيف، خدمة، مادة، كاشير، شيفت (HR).</summary>
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var log = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // ─── Branch ───
                var branchRepo = uow.Repository<Branch>();
                if (!await branchRepo.AnyAsync(_ => true))
                {
                    await branchRepo.AddAsync(new Branch { Name = "الفرع الرئيسي", CapacityPerHour = 4, IsActive = true });
                    await uow.SaveChangesAsync();
                    log.LogInformation("Seed: Branch added.");
                }

                // ─── Category ───
                var categoryRepo = uow.Repository<Category>();
                if (!await categoryRepo.AnyAsync(_ => true))
                {
                    await categoryRepo.AddAsync(new Category { Name = "غسيل سيارات", IsActive = true });
                    await uow.SaveChangesAsync();
                    log.LogInformation("Seed: Category added.");
                }

                // ─── Service + ServiceRate ───
                var serviceRepo = uow.Repository<Service>();
                var rateRepo = uow.Repository<ServiceRate>();
                if (!await serviceRepo.AnyAsync(_ => true))
                {
                    var categories = await categoryRepo.FindAsync(_ => true);
                    var catId = categories.FirstOrDefault()?.Id ?? 1;
                    var service = new Service { CategoryId = catId, Name = "غسيل أساسي", Description = "غسيل خارجي", IsActive = true };
                    await serviceRepo.AddAsync(service);
                    await uow.SaveChangesAsync();

                    await rateRepo.AddAsync(new ServiceRate
                    {
                        ServiceId = service.Id,
                        BodyType = CarBodyType.Sedan,
                        Price = 50,
                        DurationMinutes = 30,
                        IsActive = true
                    });
                    await rateRepo.AddAsync(new ServiceRate
                    {
                        ServiceId = service.Id,
                        BodyType = CarBodyType.SUV,
                        Price = 70,
                        DurationMinutes = 45,
                        IsActive = true
                    });
                    await uow.SaveChangesAsync();
                    log.LogInformation("Seed: Service and ServiceRates added.");
                }

                // ─── Material ───
                var materialRepo = uow.Repository<Material>();
                if (!await materialRepo.AnyAsync(_ => true))
                {
                    await materialRepo.AddAsync(new Material
                    {
                        Name = "شامبو غسيل",
                        Unit = MaterialUnit.Ml,
                        CostPerUnit = 0.5m,
                        ChargePerUnit = 1m,
                        IsActive = true
                    });
                    await materialRepo.AddAsync(new Material
                    {
                        Name = "ملمع",
                        Unit = MaterialUnit.Piece,
                        CostPerUnit = 2,
                        ChargePerUnit = 5,
                        IsActive = true
                    });
                    await uow.SaveChangesAsync();
                    log.LogInformation("Seed: Materials added.");
                }

                // ─── HR Shift (شيفت دوام) ───
                var shiftRepo = uow.Repository<Shift>();
                if (!await shiftRepo.AnyAsync(_ => true))
                {
                    await shiftRepo.AddAsync(new Shift
                    {
                        Name = "صباحي",
                        StartTime = new TimeOnly(8, 0),
                        EndTime = new TimeOnly(14, 0)
                    });
                    await shiftRepo.AddAsync(new Shift
                    {
                        Name = "مسائي",
                        StartTime = new TimeOnly(14, 0),
                        EndTime = new TimeOnly(22, 0)
                    });
                    await uow.SaveChangesAsync();
                    log.LogInformation("Seed: HR Shifts added.");
                }

                // ─── BranchMaterialStock (مخزون مواد بالفرع) ───
                var stockRepo = uow.Repository<BranchMaterialStock>();
                if (!await stockRepo.AnyAsync(_ => true))
                {
                    var branches = await branchRepo.FindAsync(_ => true);
                    var materials = await materialRepo.FindAsync(_ => true);
                    if (branches.Count > 0 && materials.Count > 0)
                    {
                        foreach (var mat in materials)
                            await stockRepo.AddAsync(new BranchMaterialStock
                            {
                                BranchId = branches[0].Id,
                                MaterialId = mat.Id,
                                OnHandQty = 100,
                                ReservedQty = 0
                            });
                        await uow.SaveChangesAsync();
                        log.LogInformation("Seed: BranchMaterialStock added.");
                    }
                }

                // ─── Employees (كاشير + مشرف) ───
                var employeeRepo = uow.Repository<Employee>();
                if (!await employeeRepo.AnyAsync(e => e.Role == EmployeeRole.Cashier || e.Role == EmployeeRole.Supervisor))
                {
                    await employeeRepo.AddAsync(new Employee
                    {
                        Name = "كاشير تجريبي",
                        Age = 25,
                        PhoneNumber = "0500000001",
                        IsActive = true,
                        Role = EmployeeRole.Cashier
                    });
                    await employeeRepo.AddAsync(new Employee
                    {
                        Name = "مشرف تجريبي",
                        Age = 30,
                        PhoneNumber = "0500000002",
                        IsActive = true,
                        Role = EmployeeRole.Supervisor
                    });
                    await uow.SaveChangesAsync();
                    log.LogInformation("Seed: Cashier and Supervisor employees added.");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Seed data failed.");
                throw;
            }
        }
    }
}

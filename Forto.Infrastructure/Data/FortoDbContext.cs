using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Forto.Application.Abstractions.Services.Employees;
using Forto.Domain.Entities;
using Forto.Domain.Entities.Billings;
using Forto.Domain.Entities.Bookings;
using Forto.Domain.Entities.Catalog;
using Forto.Domain.Entities.Clients;
using Forto.Domain.Entities.Employees;
using Forto.Domain.Entities.Inventory;
using Forto.Domain.Entities.Ops;
using Microsoft.EntityFrameworkCore;

namespace Forto.Infrastructure.Data
{
    public class FortoDbContext : DbContext
    {
        public FortoDbContext(DbContextOptions<FortoDbContext> options) : base(options) { }

        // هتضيفي DbSet لكل Entity لما تبدأي الجداول
        // public DbSet<Service> Services => Set<Service>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global Query Filter للـ soft delete (اختياري)
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(FortoDbContext)
                        .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                        .MakeGenericMethod(entityType.ClrType);

                    method.Invoke(null, new object[] { modelBuilder });
                }
            }

            modelBuilder.Entity<Employee>().ToTable("Employees", "hr");
            modelBuilder.Entity<Shift>().ToTable("Shifts", "hr");
            modelBuilder.Entity<EmployeeWorkSchedule>().ToTable("EmployeeWorkSchedules", "hr");

            modelBuilder.Entity<EmployeeWorkSchedule>()
                .HasIndex(x => new { x.EmployeeId, x.DayOfWeek })
                .IsUnique();






            modelBuilder.Entity<Client>().ToTable("Clients", "crm");
            modelBuilder.Entity<Car>().ToTable("Cars", "crm");

            modelBuilder.Entity<Client>()
                .HasIndex(x => x.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<Car>()
                .HasIndex(x => new { x.ClientId, x.PlateNumber })
                .IsUnique();

            modelBuilder.Entity<Car>()
                .Property(x => x.BodyType)
                .HasConversion<int>(); // يخزن enum كـ int










            modelBuilder.Entity<Category>().ToTable("Categories", "catalog");
            modelBuilder.Entity<Service>().ToTable("Services", "catalog");
            modelBuilder.Entity<ServiceRate>().ToTable("ServiceRates", "catalog");

            modelBuilder.Entity<Category>()
                .HasIndex(x => new { x.ParentId, x.Name })
                .IsUnique(false); // optional

            modelBuilder.Entity<Service>()
                .HasIndex(x => new { x.CategoryId, x.Name })
                .IsUnique(false); // optional

            modelBuilder.Entity<ServiceRate>()
                .Property(x => x.BodyType)
                .HasConversion<int>();

            modelBuilder.Entity<ServiceRate>()
    .HasIndex(x => new { x.ServiceId, x.BodyType })
    .IsUnique();

            modelBuilder.Entity<ServiceRate>()
                .HasCheckConstraint("CK_ServiceRates_Price", "[Price] >= 0");

            modelBuilder.Entity<ServiceRate>()
                .HasCheckConstraint("CK_ServiceRates_DurationMinutes", "[DurationMinutes] > 0");


            modelBuilder.Entity<Domain.Entities.Employees.EmployeeService>().ToTable("EmployeeServices", "hr");

            //modelBuilder.Entity<EmployeeService>()
            //    .HasKey(x => new { x.EmployeeId, x.ServiceId });
            modelBuilder.Entity<Domain.Entities.Employees.EmployeeService>()
                .HasIndex(x => new { x.EmployeeId, x.ServiceId })
                .IsUnique();


            modelBuilder.Entity<Domain.Entities.Employees.EmployeeService>()
                .HasOne(x => x.Employee)
                .WithMany(e => e.EmployeeServices)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Domain.Entities.Employees.EmployeeService>()
                .HasOne(x => x.Service)
                .WithMany() // مش لازم navigation في Service دلوقتي
                .HasForeignKey(x => x.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);







            modelBuilder.Entity<Branch>().ToTable("Branches", "ops");

            modelBuilder.Entity<Booking>().ToTable("Bookings", "booking");
            modelBuilder.Entity<BookingItem>().ToTable("BookingItems", "booking");

            modelBuilder.Entity<Booking>()
                .HasIndex(x => new { x.BranchId, x.SlotHourStart }); // مهم للبحث

            modelBuilder.Entity<BookingItem>()
                .Property(x => x.BodyType)
                .HasConversion<int>();

            modelBuilder.Entity<Booking>()
                .Property(x => x.Status)
                .HasConversion<int>();

            modelBuilder.Entity<BookingItem>()
                .Property(x => x.Status)
                .HasConversion<int>();

            modelBuilder.Entity<BookingItem>()
                .Property(x => x.RowVersion)
                .IsRowVersion();

            modelBuilder.Entity<BookingItem>()
                .Property(x => x.MaterialAdjustment)
                .HasPrecision(18, 3);


            modelBuilder.Entity<Booking>()
                .HasMany(x => x.Items)
                .WithOne(x => x.Booking)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Client)
                .WithMany()
                .HasForeignKey(b => b.ClientId)
                .OnDelete(DeleteBehavior.Restrict); // أو NoAction

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Car)
                .WithMany()
                .HasForeignKey(b => b.CarId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Branch)
                .WithMany()
                .HasForeignKey(b => b.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // وده نخليه Cascade عادي
            modelBuilder.Entity<Booking>()
                .HasMany(b => b.Items)
                .WithOne(i => i.Booking)
                .HasForeignKey(i => i.BookingId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Employee>()
                .Property(x => x.Role)
                .HasConversion<int>();





            modelBuilder.Entity<Invoice>().ToTable("Invoices", "billing");
            modelBuilder.Entity<InvoiceLine>().ToTable("InvoiceLines", "billing");

            modelBuilder.Entity<Invoice>()
                .Property(x => x.Status)
                .HasConversion<int>();

            modelBuilder.Entity<Invoice>()
                .Property(x => x.PaymentMethod)
                .HasConversion<int?>();

            modelBuilder.Entity<Invoice>()
                .HasIndex(x => x.BookingId)
                .IsUnique(); // Invoice واحدة لكل booking

            modelBuilder.Entity<Invoice>()
                .HasMany(x => x.Lines)
                .WithOne(x => x.Invoice)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);











            modelBuilder.Entity<Material>().ToTable("Materials", "inventory");
            modelBuilder.Entity<BranchMaterialStock>().ToTable("BranchMaterialStocks", "ops");

            modelBuilder.Entity<Material>()
                .Property(x => x.Unit)
                .HasConversion<int>();

            // decimal precision
            modelBuilder.Entity<Material>()
                .Property(x => x.CostPerUnit)
                .HasPrecision(18, 3);

            modelBuilder.Entity<Material>()
                .Property(x => x.ChargePerUnit)
                .HasPrecision(18, 3);

            modelBuilder.Entity<BranchMaterialStock>()
                .Property(x => x.OnHandQty)
                .HasPrecision(18, 3);

            modelBuilder.Entity<BranchMaterialStock>()
                .Property(x => x.ReservedQty)
                .HasPrecision(18, 3);

            modelBuilder.Entity<BranchMaterialStock>()
                .Property(x => x.ReorderLevel)
                .HasPrecision(18, 3);

            // Unique stock per (Branch, Material)
            modelBuilder.Entity<BranchMaterialStock>()
                .HasIndex(x => new { x.BranchId, x.MaterialId })
                .IsUnique();

            // prevent negatives (optional but good)
            modelBuilder.Entity<BranchMaterialStock>()
                .HasCheckConstraint("CK_BranchStock_OnHand_NonNegative", "[OnHandQty] >= 0");

            modelBuilder.Entity<BranchMaterialStock>()
                .HasCheckConstraint("CK_BranchStock_Reserved_NonNegative", "[ReservedQty] >= 0");

            // relationships (avoid cascade path problems => NoAction)
            modelBuilder.Entity<BranchMaterialStock>()
                .HasOne(x => x.Branch)
                .WithMany()
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BranchMaterialStock>()
                .HasOne(x => x.Material)
                .WithMany()
                .HasForeignKey(x => x.MaterialId)
                .OnDelete(DeleteBehavior.NoAction);








            modelBuilder.Entity<ServiceMaterialRecipe>().ToTable("ServiceMaterialRecipes", "catalog");

            modelBuilder.Entity<ServiceMaterialRecipe>()
                .Property(x => x.BodyType)
                .HasConversion<int>();

            modelBuilder.Entity<ServiceMaterialRecipe>()
                .Property(x => x.DefaultQty)
                .HasPrecision(18, 3);

            modelBuilder.Entity<ServiceMaterialRecipe>()
                .HasIndex(x => new { x.ServiceId, x.BodyType, x.MaterialId })
                .IsUnique();

            // avoid cascade path issues
            modelBuilder.Entity<ServiceMaterialRecipe>()
                .HasOne(x => x.Service)
                .WithMany()
                .HasForeignKey(x => x.ServiceId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ServiceMaterialRecipe>()
                .HasOne(x => x.Material)
                .WithMany()
                .HasForeignKey(x => x.MaterialId)
                .OnDelete(DeleteBehavior.NoAction);




            modelBuilder.Entity<BookingItemMaterialUsage>()
                .ToTable("BookingItemMaterialUsages", "ops");

            modelBuilder.Entity<BookingItemMaterialUsage>()
                .HasIndex(x => new { x.BookingItemId, x.MaterialId })
                .IsUnique();

            modelBuilder.Entity<BookingItemMaterialUsage>()
                .Property(x => x.DefaultQty).HasPrecision(18, 3);
            modelBuilder.Entity<BookingItemMaterialUsage>()
                .Property(x => x.ReservedQty).HasPrecision(18, 3);
            modelBuilder.Entity<BookingItemMaterialUsage>()
                .Property(x => x.ActualQty).HasPrecision(18, 3);

            modelBuilder.Entity<BookingItemMaterialUsage>()
                .Property(x => x.UnitCost).HasPrecision(18, 3);
            modelBuilder.Entity<BookingItemMaterialUsage>()
                .Property(x => x.UnitCharge).HasPrecision(18, 3);
            modelBuilder.Entity<BookingItemMaterialUsage>()
                .Property(x => x.ExtraCharge).HasPrecision(18, 3);

            // relationships (NoAction لتفادي cascade paths)
            modelBuilder.Entity<BookingItemMaterialUsage>()
                .HasOne(x => x.BookingItem)
                .WithMany()
                .HasForeignKey(x => x.BookingItemId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BookingItemMaterialUsage>()
                .HasOne(x => x.Material)
                .WithMany()
                .HasForeignKey(x => x.MaterialId)
                .OnDelete(DeleteBehavior.NoAction);










            modelBuilder.Entity<MaterialMovement>().ToTable("MaterialMovements", "ops");

            modelBuilder.Entity<MaterialMovement>()
                .Property(x => x.MovementType)
                .HasConversion<int>();

            modelBuilder.Entity<MaterialMovement>()
                .Property(x => x.Qty)
                .HasPrecision(18, 3);

            modelBuilder.Entity<MaterialMovement>()
                .Property(x => x.UnitCostSnapshot)
                .HasPrecision(18, 3);

            modelBuilder.Entity<MaterialMovement>()
                .Property(x => x.TotalCost)
                .HasPrecision(18, 3);

            modelBuilder.Entity<MaterialMovement>()
                .HasIndex(x => new { x.BranchId, x.OccurredAt });

            modelBuilder.Entity<MaterialMovement>()
                .HasIndex(x => new { x.MaterialId, x.OccurredAt });

            // علاقات بدون Cascade
            modelBuilder.Entity<MaterialMovement>()
                .HasOne(x => x.Branch)
                .WithMany()
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MaterialMovement>()
                .HasOne(x => x.Material)
                .WithMany()
                .HasForeignKey(x => x.MaterialId)
                .OnDelete(DeleteBehavior.NoAction);

        }

        private static void SetSoftDeleteFilter<TEntity>(ModelBuilder builder) where TEntity : BaseEntity
        {
            builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
        }
    }
}

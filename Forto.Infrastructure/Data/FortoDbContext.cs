using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Entities;
using Forto.Domain.Entities.Catalog;
using Forto.Domain.Entities.Clients;
using Forto.Domain.Entities.Employee;
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


            modelBuilder.Entity<EmployeeService>().ToTable("EmployeeServices", "hr");

            //modelBuilder.Entity<EmployeeService>()
            //    .HasKey(x => new { x.EmployeeId, x.ServiceId });
            modelBuilder.Entity<EmployeeService>()
                .HasIndex(x => new { x.EmployeeId, x.ServiceId })
                .IsUnique();


            modelBuilder.Entity<EmployeeService>()
                .HasOne(x => x.Employee)
                .WithMany(e => e.EmployeeServices)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeService>()
                .HasOne(x => x.Service)
                .WithMany() // مش لازم navigation في Service دلوقتي
                .HasForeignKey(x => x.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);


        }

        private static void SetSoftDeleteFilter<TEntity>(ModelBuilder builder) where TEntity : BaseEntity
        {
            builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
        }
    }
}

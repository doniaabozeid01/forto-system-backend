using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Entities;
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



        }

        private static void SetSoftDeleteFilter<TEntity>(ModelBuilder builder) where TEntity : BaseEntity
        {
            builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
        }
    }
}

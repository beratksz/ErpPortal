using ErpPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace ErpPortal.Infrastructure.Data
{
    public class ErpPortalDbContext : DbContext
    {
        public DbSet<ShopOrder> ShopOrders { get; set; }
        public DbSet<ShopOrderOperation> ShopOrderOperations { get; set; }
        public DbSet<User> Users => Set<User>();
        public DbSet<WorkCenter> WorkCenters => Set<WorkCenter>();
        public DbSet<WorkLog> WorkLogs => Set<WorkLog>();
        public DbSet<UserWorkCenter> UserWorkCenters { get; set; }

        public ErpPortalDbContext(DbContextOptions<ErpPortalDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ShopOrder>(entity =>
            {
                entity.HasKey(e => e.OrderNo);
                entity.Property(e => e.OrderNo).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.PartNo).HasMaxLength(50);
                entity.Property(e => e.PartDescription).HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(1000);

                entity.Property(e => e.Quantity).HasPrecision(18, 4);

                entity.HasMany(e => e.Operations)
                    .WithOne(e => e.ShopOrder)
                    .HasForeignKey(e => e.OrderNo)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ShopOrderOperation>(entity =>
            {
                entity.HasKey(e => new { e.OrderNo, e.OperationNo });
                entity.Property(e => e.OrderNo).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.WorkCenterCode).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.AssignedTo).HasMaxLength(100);
                entity.Property(e => e.ReportedBy).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.InterruptionReason).HasMaxLength(500);
                entity.Property(e => e.LastSyncError).HasMaxLength(1000);

                entity.Property(e => e.RevisedQtyDue).HasPrecision(18, 4);
                entity.Property(e => e.QtyComplete).HasPrecision(18, 4);
                entity.Property(e => e.QtyScrapped).HasPrecision(18, 4);
                entity.Property(e => e.MachSetupTime).HasPrecision(18, 4);
                entity.Property(e => e.MachRunFactor).HasPrecision(18, 4);
                entity.Property(e => e.LaborSetupTime).HasPrecision(18, 4);
                entity.Property(e => e.LaborRunFactor).HasPrecision(18, 4);
                entity.Property(e => e.MoveTime).HasPrecision(18, 4);
                entity.Property(e => e.QueueTime).HasPrecision(18, 4);
                entity.Property(e => e.EfficiencyFactor).HasPrecision(18, 4);
                entity.Property(e => e.QuantityCompleted).HasPrecision(18, 4);
                entity.Property(e => e.QuantityScrapped).HasPrecision(18, 4);
            });

            // User entity konfigürasyonu
            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(u => u.Id);
                e.Property(u => u.Username).HasMaxLength(50).IsRequired();
                e.Property(u => u.FullName).HasMaxLength(100).IsRequired();
                e.Property(u => u.Password).HasMaxLength(255).IsRequired();
                e.HasIndex(u => u.Username).IsUnique();
            });

            // WorkCenter entity konfigürasyonu
            modelBuilder.Entity<WorkCenter>(e =>
            {
                e.HasKey(w => w.Id);
                e.Property(w => w.Code).HasMaxLength(20).IsRequired();
                e.Property(w => w.Name).HasMaxLength(100).IsRequired();
                e.Property(w => w.Description).HasMaxLength(500);
                e.HasIndex(w => w.Code).IsUnique();
            });

            modelBuilder.Entity<UserWorkCenter>()
                .HasKey(uw => new { uw.UserId, uw.WorkCenterId });

            modelBuilder.Entity<UserWorkCenter>()
                .HasOne(uw => uw.User)
                .WithMany(u => u.UserWorkCenters)
                .HasForeignKey(uw => uw.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserWorkCenter>()
                .HasOne(uw => uw.WorkCenter)
                .WithMany(wc => wc.UserWorkCenters)
                .HasForeignKey(uw => uw.WorkCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            // WorkLog entity konfigürasyonu - basit tablo olarak
            modelBuilder.Entity<WorkLog>(e =>
            {
                e.HasKey(w => w.Id);
                e.Property(w => w.Description).HasMaxLength(500);
                e.Property(w => w.Notes).HasMaxLength(1000);
                e.Property(w => w.Status).HasMaxLength(50);
                e.Property(w => w.OrderNo).HasMaxLength(50);

                // Navigation ilişkileri olmadan basit tablo
                e.HasIndex(w => new { w.OrderNo, w.OperationNo, w.UserId, w.StartTime });
            });
        }
    }
}

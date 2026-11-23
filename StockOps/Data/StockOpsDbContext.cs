using Microsoft.EntityFrameworkCore;
using StockOps.Models;

namespace StockOps.Data
{
    public class StockOpsDbContext : DbContext
    {
        public StockOpsDbContext(
            DbContextOptions<StockOpsDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;

        public DbSet<User> Users { get; set; } = null!;

        public DbSet<Request> Requests { get; set; } = null!;

        public DbSet<StockTransaction> StockTransactions { get; set; } = null!;

        public DbSet<Category> Categories { get; set; } = null!;

        public DbSet<Supplier> Suppliers { get; set; } = null!;

        public DbSet<Warehouse> Warehouses { get; set; } = null!;

        public DbSet<WarehouseStock> WarehouseStocks { get; set; } = null!;

        public DbSet<WarehouseZone> WarehouseZones { get; set; } = null!;

        public DbSet<StorageLocation> StorageLocations { get; set; } = null!;

        public DbSet<Forklift> Forklifts { get; set; } = null!;

        public DbSet<ForkliftOperator> ForkliftOperators { get; set; } = null!;

        public DbSet<ForkliftMaintenance> ForkliftMaintenances { get; set; }
            = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------------------------------------
            // PRODUCT
            // -------------------------------------------------

            modelBuilder.Entity<Product>()
                .HasIndex(x => x.Barcode)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(x => x.ProductCode)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);

            // -------------------------------------------------
            // USER
            // -------------------------------------------------

            modelBuilder.Entity<User>()
                .HasIndex(x => x.Username)
                .IsUnique();

            // -------------------------------------------------
            // WAREHOUSE
            // -------------------------------------------------

            modelBuilder.Entity<Warehouse>()
                .HasIndex(x => x.WarehouseCode)
                .IsUnique();

            modelBuilder.Entity<Warehouse>()
                .Property(x => x.TotalCapacity)
                .HasPrecision(18, 2);

            // -------------------------------------------------
            // WAREHOUSE ZONE
            // -------------------------------------------------

            modelBuilder.Entity<WarehouseZone>()
                .HasOne(x => x.Warehouse)
                .WithMany(x => x.WarehouseZones)
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WarehouseZone>()
                .HasIndex(x => new
                {
                    x.WarehouseId,
                    x.ZoneCode
                })
                .IsUnique();

            // -------------------------------------------------
            // STORAGE LOCATION
            // -------------------------------------------------

            modelBuilder.Entity<StorageLocation>()
                .HasOne(x => x.WarehouseZone)
                .WithMany(x => x.StorageLocations)
                .HasForeignKey(x => x.WarehouseZoneId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StorageLocation>()
                .HasIndex(x => new
                {
                    x.WarehouseZoneId,
                    x.LocationCode
                })
                .IsUnique();

            modelBuilder.Entity<StorageLocation>()
                .Property(x => x.MaximumCapacity)
                .HasPrecision(18, 2);

            // -------------------------------------------------
            // WAREHOUSE STOCK
            // -------------------------------------------------

            modelBuilder.Entity<WarehouseStock>()
                .HasOne(x => x.Product)
                .WithMany(x => x.WarehouseStocks)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WarehouseStock>()
                .HasOne(x => x.Warehouse)
                .WithMany(x => x.WarehouseStocks)
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WarehouseStock>()
                .HasOne(x => x.StorageLocation)
                .WithMany(x => x.WarehouseStocks)
                .HasForeignKey(x => x.StorageLocationId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WarehouseStock>()
                .HasIndex(x => new
                {
                    x.ProductId,
                    x.WarehouseId,
                    x.StorageLocationId
                })
                .IsUnique();

            modelBuilder.Entity<WarehouseStock>()
                .Property(x => x.Quantity)
                .HasPrecision(18, 3);

            modelBuilder.Entity<WarehouseStock>()
                .Property(x => x.ReservedQuantity)
                .HasPrecision(18, 3);

            modelBuilder.Entity<WarehouseStock>()
                .Property(x => x.MinimumStockLevel)
                .HasPrecision(18, 3);

            // -------------------------------------------------
            // STOCK TRANSACTION
            // -------------------------------------------------

            modelBuilder.Entity<StockTransaction>()
                .HasOne(x => x.Product)
                .WithMany(x => x.StockTransactions)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockTransaction>()
                .HasOne(x => x.Warehouse)
                .WithMany(x => x.StockTransactions)
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockTransaction>()
                .HasOne(x => x.User)
                .WithMany(x => x.StockTransactions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<StockTransaction>()
                .Property(x => x.Quantity)
                .HasPrecision(18, 3);

            // -------------------------------------------------
            // FORKLIFT OPERATOR
            // -------------------------------------------------

            modelBuilder.Entity<ForkliftOperator>()
                .HasIndex(x => x.EmployeeNumber)
                .IsUnique();

            // -------------------------------------------------
            // FORKLIFT
            // -------------------------------------------------

            modelBuilder.Entity<Forklift>()
                .HasIndex(x => x.ForkliftCode)
                .IsUnique();

            modelBuilder.Entity<Forklift>()
                .HasIndex(x => x.SerialNumber)
                .IsUnique();

            modelBuilder.Entity<Forklift>()
                .HasOne(x => x.Warehouse)
                .WithMany(x => x.Forklifts)
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Forklift>()
                .HasOne(x => x.AssignedOperator)
                .WithMany(x => x.Forklifts)
                .HasForeignKey(x => x.AssignedOperatorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Forklift>()
                .Property(x => x.CarryingCapacityKg)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Forklift>()
                .Property(x => x.TotalWorkingHours)
                .HasPrecision(18, 2);

            // -------------------------------------------------
            // FORKLIFT MAINTENANCE
            // -------------------------------------------------

            modelBuilder.Entity<ForkliftMaintenance>()
                .HasOne(x => x.Forklift)
                .WithMany(x => x.MaintenanceRecords)
                .HasForeignKey(x => x.ForkliftId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForkliftMaintenance>()
                .Property(x => x.WorkingHoursAtMaintenance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ForkliftMaintenance>()
                .Property(x => x.Cost)
                .HasPrecision(18, 2);
        }
    }
}
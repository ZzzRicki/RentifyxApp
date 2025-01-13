using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rentifyx.DAL.Entities;
using Rentifyx.DAL.Entities.Comunication;
using Rentifyx.DAL.Entities.Vehicle;

namespace Rentifyx.DAL.Context
{
    public class RentifyxContext : IdentityDbContext
    {
        public RentifyxContext(DbContextOptions<RentifyxContext> options) : base(options) { }

        #region DbSets
        public DbSet<RentifyxUser>? RentifyxUsers { get; set; }
        public DbSet<Vehicle>? Vehicles { get; set; }
        public DbSet<Payment>? Payments { get; set; }
        public DbSet<Reservation>? Reservations { get; set; }
        public DbSet<VehicleCharacteristic> VehicleCharacteristics { get; set; }
        public DbSet<Message> Messages { get; set; }

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Vehicle>()
                .HasMany(v => v.Characteristics)
                .WithOne(vc => vc.Vehicle)
                .HasForeignKey(vc => vc.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>().HasKey(m => m.Id);
        }
    }
}
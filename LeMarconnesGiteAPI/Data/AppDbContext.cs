using LeMarconnesGiteAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LeMarconnesGiteAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Gite> Gites { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<GiteFacility> GiteFacilities { get; set; }
        public DbSet<Pricing> Pricings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Samengestelde primaire sleutel voor de koppeltabel Gite_Facility
            modelBuilder.Entity<GiteFacility>()
                .HasKey(gf => new { gf.GiteId, gf.FacilityId });

            // Gite <-> Facility via GiteFacility
            modelBuilder.Entity<GiteFacility>()
                .HasOne(gf => gf.Gite)
                .WithMany(g => g.GiteFacilities)
                .HasForeignKey(gf => gf.GiteId);

            modelBuilder.Entity<GiteFacility>()
                .HasOne(gf => gf.Facility)
                .WithMany(f => f.GiteFacilities)
                .HasForeignKey(gf => gf.FacilityId);

            // Reservation -> Gite
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Gite)
                .WithMany(g => g.Reservations)
                .HasForeignKey(r => r.GiteId);

            // Reservation -> Guest
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Guest)
                .WithMany(g => g.Reservations)
                .HasForeignKey(r => r.GuestId);

            // Payment -> Reservation
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Reservation)
                .WithMany(r => r.Payments)
                .HasForeignKey(p => p.ReservationId);

            // Pricing -> Gite
            modelBuilder.Entity<Pricing>()
                .HasOne(p => p.Gite)
                .WithMany(g => g.Pricings)
                .HasForeignKey(p => p.GiteId);
        }
    }
}

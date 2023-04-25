using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultipleAuthIdentity.Areas.Identity.Data;
using MultipleAuthIdentity.Models;
using System.Reflection.Emit;

namespace MultipleAuthIdentity.Data;

public class AuthDbContext : IdentityDbContext<AppUser>
{
    public DbSet<Review> Review { get; set; }
    public DbSet<Bus> Bus { get; set; }
    public DbSet<Routes> Routes { get; set; }
    public DbSet<Reservation> Reservations { get; set; }

    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.Content).HasMaxLength(2048);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Subject).HasMaxLength(100);
            

        });
        builder.Entity<Bus>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.Bus_Plate_number).HasMaxLength(50);
            

        });
        builder.Entity<Routes>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            

        });
        builder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();

        });
    }



}

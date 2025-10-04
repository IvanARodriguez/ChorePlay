using ChorePlay.Api.Shared.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChorePlay.Api.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Ulid>
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);
    builder.HasPostgresExtension("uuid-ossp");
    builder.Entity<AppUser>(b =>
    {
      b.Property(u => u.Id)
      .HasConversion(
        v => v.ToString(),
        v => Ulid.Parse(v));
    });

    builder.Entity<AppRole>(b =>
    {
      b.Property(r => r.Id)
      .HasConversion(
        v => v.ToString(),
        v => Ulid.Parse(v));
    });

    builder.Entity<IdentityUserRole<Ulid>>(b =>
    {
      b.Property(r => r.UserId).HasConversion(
        v => v.ToString(),
        v => Ulid.Parse(v));

      b.Property(r => r.RoleId).HasConversion(
        v => v.ToString(),
        v => Ulid.Parse(v));
    });

    builder.Entity<IdentityUserLogin<Ulid>>(b =>
      b.Property(l => l.UserId).HasConversion(
        v => v.ToString(),
        v => Ulid.Parse(v)));

    builder.Entity<IdentityUserToken<Ulid>>(b =>
    {
      b.Property(t => t.UserId).HasConversion(
        v => v.ToString(),
        v => Ulid.Parse(v));
    });
    builder.Entity<IdentityUserClaim<Ulid>>(b =>
    {
      b.Property(t => t.UserId).HasConversion(
        v => v.ToString(),
        v => Ulid.Parse(v));
    });
    builder.Entity<IdentityRoleClaim<Ulid>>(b =>
    {
      b.Property(t => t.RoleId).HasConversion(
        v => v.ToString(),
        v => Ulid.Parse(v));
    });
  }
};
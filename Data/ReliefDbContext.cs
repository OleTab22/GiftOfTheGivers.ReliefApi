using GiftOfTheGivers.ReliefApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.ReliefApi.Data;

public class ReliefDbContext : DbContext
{
    public ReliefDbContext(DbContextOptions<ReliefDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<Volunteer> Volunteers => Set<Volunteer>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Donation> Donations => Set<Donation>();
}



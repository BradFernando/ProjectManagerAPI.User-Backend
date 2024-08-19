using Microsoft.EntityFrameworkCore;
using ProjectManager.Api.User.Models;

namespace ProjectManager.Api.User.Context;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    public DbSet<Users> Users { get; set; }
}
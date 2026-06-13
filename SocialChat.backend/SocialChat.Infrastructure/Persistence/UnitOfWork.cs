using Microsoft.EntityFrameworkCore;
using SocialChat.Application.Abstractions;
using SocialChat.Application.Abstractions.Repositories;
using SocialChat.Domain.Entities;
using SocialChat.Domain.Enums;
using SocialChat.Infrastructure.Persistence.Repositories;

namespace SocialChat.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (!await context.Roles.AnyAsync())
        {
            context.Roles.AddRange(
                Role.Create(Role.UserRole),
                Role.Create(Role.AdminRole));
            await context.SaveChangesAsync();
        }
    }
}

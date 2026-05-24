using Microsoft.EntityFrameworkCore;
using UserService.Domain;
using UserService.Domain.Entities;

namespace UserService.Application.Common.Interfaces;

public interface IUserDbContext
{
    DbSet<User> Users { get; }
    DbSet<OtpCode> OtpCodes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

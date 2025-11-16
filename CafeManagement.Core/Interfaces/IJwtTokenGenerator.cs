using CafeManagement.Core.Entities;

namespace CafeManagement.Core.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
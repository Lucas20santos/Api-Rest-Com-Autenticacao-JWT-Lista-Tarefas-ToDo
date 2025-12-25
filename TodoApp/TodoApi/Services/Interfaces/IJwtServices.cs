using TodoApi.Models.Domain;

namespace TodoApi.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerationToken(User user);
    }
}
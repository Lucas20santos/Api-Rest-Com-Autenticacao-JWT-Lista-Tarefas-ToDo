using TodoApi.Repositories;
using TodoApi.Services.Interfaces;
using TodoApi.Models.DTOs;
using TodoApi.Models.Domain;

namespace TodoApi.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IJwtService _jwtService; // vamos criar
        public AuthService(IUserRepository userRepo, IJwtService jwtService)
        {
            _userRepo = userRepo;
            _jwtService = jwtService;
        }
        public async Task RegisterAsync(RegisterDto dto)
        {
            var existing = await _userRepo.GetByUsernameAsync(dto.Username);
            if (existing != null) throw new Exception("Usuário já existe");

            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = hash
            };
            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();
        }
        public async Task<string> LoginAsync(LoginDto dto)
        {
            var user = await _userRepo.GetByUsernameAsync(dto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password,
                user.PasswordHash))
                throw new Exception("Credenciais inválidas");

            return _jwtService.GenerateToken(user);
        }
    }
}
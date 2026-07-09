using Microsoft.EntityFrameworkCore;
using Shortener.Data;
using Shortener.DTOs;
using Shortener.Entities;

namespace Shortener.Services;

public class UserService
{
    private readonly Serilog.ILogger _logger;
    private readonly ApplicationDbContext _db;

    public  UserService(ApplicationDbContext db)
    {
        _db = db;
        _logger = Serilog.Log.ForContext<UserService>();
    }

    public async Task<User> CreateUserAsync(UserCreateRequest  requestData)
    {
        bool loginAlreadyInUse = await _db.Users.AnyAsync(u => u.Login == requestData.Login);
        if (loginAlreadyInUse)
        {
            _logger.Warning("Login is already in use");
            throw new ArgumentException("Login is already in use");
        }

        User user = new User()
        {
            Login = requestData.Login,
            Password = requestData.Password
        };

        _db.Add(user);
        await _db.SaveChangesAsync();

        _logger.Information("User saved to database");

        return user;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _db.Users.SingleOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserByLoginAsync(string login)
    {
        return await _db.Users.SingleOrDefaultAsync(u => u.Login == login);
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        User? user = await GetUserByIdAsync(userId);
        if (user is null)
        {
            _logger.Warning("User not found");
            return false;
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        _logger.Debug("User deleted");

        return true;
    }
}
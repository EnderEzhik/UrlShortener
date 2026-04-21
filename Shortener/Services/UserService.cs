using Microsoft.EntityFrameworkCore;
using Shortener.Data;
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

    public async Task<User> CreateUser(string login, string password)
    {
        bool loginAlreadyInUse = await _db.Users.AnyAsync(u => u.Login == login);
        if (loginAlreadyInUse)
        {
            _logger.Warning("Login is already in use");
            
            throw new ArgumentException("Login is already in use");
        }
        
        User user = new User()
        {
            Login = login,
            Password = password
        };
        
        try
        {
            _db.Add(user);
            await _db.SaveChangesAsync();
            
            _logger.Information("User saved to database");
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Database error while saving user");
            throw;
        }
    }

    public async Task<User?> GetUserById(int userId)
    {
        try
        {
            return await _db.Users.SingleOrDefaultAsync(u => u.Id == userId);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Database error while getting user by id");
            throw;
        }
    }

    public async Task<User?> GetUserByLogin(string login)
    {
        try
        {
            return await _db.Users.SingleOrDefaultAsync(u => u.Login == login);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Database error while getting user by login");
            throw;
        }
    }

    public async Task<bool> DeleteUser(int userId)
    {
        User? user = await GetUserById(userId);
        if (user is null)
        {
            _logger.Warning("User not found");
            return false;
        }

        try
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            
            _logger.Debug("User deleted");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Database error while deleting user");
            throw;
        }
    }
}
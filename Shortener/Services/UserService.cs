using Microsoft.EntityFrameworkCore;
using Shortener.Data;
using Shortener.Entities;

namespace Shortener.Services;

public class UserService
{
    private readonly ApplicationDbContext _db;
    
    public  UserService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<User> CreateUser(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login) || password.Length < 8)
        {
            throw new ArgumentException("Password must contain at least 8 characters");
        }
        
        var loginAlreadyInUse = await _db.Users.AnyAsync(u => u.Login == login);
        if (loginAlreadyInUse)
        {
            throw new ArgumentException("Login is already in use");
        }
        
        User newUser = new User()
        {
            Login = login,
            Password = password
        };
        
        _db.Add(newUser);
        await _db.SaveChangesAsync();
        
        return newUser;
    }

    public async Task<User?> GetUser(string login)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Login == login);
        return user;
    }

    public async Task<bool> DeleteUser(string login)
    {
        var user = await GetUser(login);
        if (user is null)
        {
            return false;
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return true;
    }
}
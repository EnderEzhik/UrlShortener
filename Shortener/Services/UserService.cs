using Microsoft.EntityFrameworkCore;
using Serilog;
using Shortener.Data;
using Shortener.Entities;

namespace Shortener.Services;

public class UserService
{
    private readonly Serilog.ILogger logger = Log.ForContext<UserService>();
    private readonly ApplicationDbContext _db;
    
    public  UserService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<User> CreateUser(string login, string password)
    {
        logger.Information("Creating new user");
        
        bool loginAlreadyInUse = await _db.Users.AnyAsync(u => u.Login == login);
        if (loginAlreadyInUse)
        {
            logger.Warning("User with login {login} already exists", login);
            throw new ArgumentException("Login is already in use");
        }
        
        User newUser = new User()
        {
            Login = login,
            Password = password
        };
        
        logger.Information("Saving new user to database");
        _db.Add(newUser);
        
        try
        {
            await _db.SaveChangesAsync();
            logger.Information("successfully saved new user to database");
            return newUser;
        }
        catch (Exception e)
        {
            logger.Error(e, "Error when saving new user to database");
            throw;
        }
    }

    public async Task<User?> GetUserById(int userId)
    {
        logger.Information("Searching user by id");
        try
        {
            User? user = await _db.Users.SingleOrDefaultAsync(u => u.Id == userId);
            logger.Information("User found in database: {userFound}", user is not null);
            return user;
        }
        catch (Exception e)
        {
            logger.Error(e, "Error when searching user by id");
            throw;
        }
    }

    public async Task<User?> GetUserByLogin(string login)
    {
        logger.Information("Searching user by login");
        try
        {
            User? user = await _db.Users.SingleOrDefaultAsync(u => u.Login == login);
            logger.Information("User found in database: {userFound}", user is not null);
            return user;
        }
        catch (Exception e)
        {
            logger.Error(e, "Error when searching user by login");
            throw;
        }
    }

    public async Task<bool> DeleteUser(int userId)
    {
        logger.Information("Searching user to delete");
        User? user = await GetUserById(userId);
        if (user is null)
        {
            logger.Information("User not found");
            return false;
        }
        
        logger.Information("User found");
        logger.Information("Deleting user");

        _db.Users.Remove(user);
        
        try
        {
            await _db.SaveChangesAsync();
            logger.Information("User deleted");
            return true;
        }
        catch (Exception e)
        {
            logger.Error(e, "Error when deleting User");
            throw;
        }
    }
}
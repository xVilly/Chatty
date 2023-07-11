using Chatty.Models;
using Chatty.Data;
using Microsoft.EntityFrameworkCore;

namespace Chatty.Services;

public class UserService
{
    private readonly ChatContext _context;
    public UserService(ChatContext context)
    {
        _context = context;
    }

    public IEnumerable<User> GetAll()
    {
        return _context.Users
            .AsNoTracking()
            .ToList();
    }

    public User? GetById(int id)
    {
        return _context.Users
            .AsNoTracking()
            .SingleOrDefault(u => u.Id == id);
    }

    public User? GetByName(string name)
    {
        return _context.Users
            .AsNoTracking()
            .SingleOrDefault(u => u.Name == name);
    }

    public User Create(User newUser)
    {
        _context.Users.Add(newUser);
        _context.SaveChanges();

        return newUser;
    }
}

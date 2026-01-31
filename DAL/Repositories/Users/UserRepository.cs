using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repositories.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly CarConfiguratorDbContext _context;

        public UserRepository(CarConfiguratorDbContext context)
        {
            _context = context;
        }

        //CRUD

        public IEnumerable<User> GetAll()
        {
            return _context.Users
                .OrderBy(u => u.UserName)
                .ToList();
        }

        public User? GetById(int id)
        {
            return _context.Users.Find(id);
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = _context.Users
                .Include(u => u.CarConfigurations)
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
                return;


            if (user.CarConfigurations != null && user.CarConfigurations.Any())
            {
                _context.CarConfigurations.RemoveRange(user.CarConfigurations);
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        //Auth helpers

        public User? GetByUsername(string username)
        {
            return _context.Users
                .FirstOrDefault(u => u.UserName == username);
        }

        public User? GetByEmail(string email)
        {
            return _context.Users
                .FirstOrDefault(u => u.Email == email);
        }

        public bool ExistsUsername(string username)
        {
            return _context.Users.Any(u => u.UserName == username);
        }

        public bool ExistsEmail(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }
    }
}

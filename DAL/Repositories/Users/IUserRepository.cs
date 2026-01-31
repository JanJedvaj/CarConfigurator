using DAL.Models;
using System.Collections.Generic;

namespace DAL.Repositories.Users
{
    public interface IUserRepository
    {
        // CRUD (admin)
        IEnumerable<User> GetAll();
        User? GetById(int id);
        void Add(User user);
        void Update(User user);
        void Delete(int id);

        // Auth helpers
        User? GetByUsername(string username);
        User? GetByEmail(string email);

        bool ExistsUsername(string username);
        bool ExistsEmail(string email);
    }
}

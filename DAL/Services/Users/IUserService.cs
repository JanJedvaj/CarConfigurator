using DAL.Models;
using System.Collections.Generic;

namespace DAL.Services.Users
{
    public interface IUserService
    {
        IEnumerable<User> GetAllUsers();
        User? GetUser(int id);
        void UpdateUser(User user);
        void DeleteUser(int id);

        User? GetByUsername(string username);
    }
}

using DAL.Models;
using System.Collections.Generic;

namespace DAL.Services.Users
{
    public interface IUserService
    {
        IEnumerable<User> GetAllUsers();
        User? GetUser(int id);
        void CreateUser(User user, string plainPassword);
        void UpdateUser(User user);
        void DeleteUser(int id);

        User Login(string usernameOrEmail, string plainPassword);

        void Register(User user, string plainPassword);

        User? GetByUsername(string username);
        void ChangePassword(string username, string oldPassword, string newPassword);

    }
}

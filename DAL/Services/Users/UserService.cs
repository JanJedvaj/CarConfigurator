using DAL.Models;
using DAL.Repositories.Users;
using System;

namespace DAL.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public System.Collections.Generic.IEnumerable<User> GetAllUsers()
            => _userRepository.GetAll();

        public User? GetUser(int id)
            => _userRepository.GetById(id);

        public void UpdateUser(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (user.Id <= 0) throw new InvalidOperationException("Id is required.");
            if (string.IsNullOrWhiteSpace(user.UserName)) throw new InvalidOperationException("UserName is required.");
            if (string.IsNullOrWhiteSpace(user.Email)) throw new InvalidOperationException("Email is required.");

            var existing = _userRepository.GetById(user.Id)
                ?? throw new InvalidOperationException("User not found.");

            if (!string.Equals(existing.UserName, user.UserName, StringComparison.OrdinalIgnoreCase)
                && _userRepository.ExistsUsername(user.UserName))
                throw new InvalidOperationException("Username already exists.");

            if (!string.Equals(existing.Email, user.Email, StringComparison.OrdinalIgnoreCase)
                && _userRepository.ExistsEmail(user.Email))
                throw new InvalidOperationException("Email already exists.");

            _userRepository.Update(user);
        }

        public void DeleteUser(int id)
            => _userRepository.Delete(id);

        public User? GetByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            return _userRepository.GetByUsername(username.Trim());
        }
    }
}

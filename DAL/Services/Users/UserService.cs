using DAL.Models;
using DAL.Repositories.Users;
using DAL.Security;
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

        public void CreateUser(User user, string plainPassword)
        {
            ValidateForCreate(user, plainPassword);

            if (_userRepository.ExistsUsername(user.UserName))
                throw new InvalidOperationException("Username already exists.");

            if (_userRepository.ExistsEmail(user.Email))
                throw new InvalidOperationException("Email already exists.");

            var salt = PasswordHashProvider.GetSalt();
            var hash = PasswordHashProvider.GetHash(plainPassword, salt);

            user.PasswordSalt = salt;
            user.PasswordHash = hash;

            if (string.IsNullOrWhiteSpace(user.Role))
                user.Role = "User";

            if (user.CreatedAt == default)
                user.CreatedAt = DateTime.UtcNow;

            user.IsActive = true;

            _userRepository.Add(user);
        }

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

        public void Register(User user, string plainPassword)
            => CreateUser(user, plainPassword);

        public User Login(string usernameOrEmail, string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail))
                throw new InvalidOperationException("Username/Email is required.");

            if (string.IsNullOrWhiteSpace(plainPassword))
                throw new InvalidOperationException("Password is required.");

            User? user = usernameOrEmail.Contains("@")
                ? _userRepository.GetByEmail(usernameOrEmail)
                : _userRepository.GetByUsername(usernameOrEmail);

            if (user == null)
                throw new InvalidOperationException("Invalid credentials.");

            if (!user.IsActive)
                throw new InvalidOperationException("User is not active.");

            var computedHash = PasswordHashProvider.GetHash(plainPassword, user.PasswordSalt);

            if (!string.Equals(computedHash, user.PasswordHash, StringComparison.Ordinal))
                throw new InvalidOperationException("Invalid credentials.");

            user.LastLoginAt = DateTime.UtcNow;
            _userRepository.Update(user);

            return user;
        }

        private static void ValidateForCreate(User user, string plainPassword)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(user.UserName))
                throw new InvalidOperationException("UserName is required.");

            if (string.IsNullOrWhiteSpace(user.Email))
                throw new InvalidOperationException("Email is required.");

            if (string.IsNullOrWhiteSpace(plainPassword))
                throw new InvalidOperationException("Password is required.");

            if (plainPassword.Length < 6)
                throw new InvalidOperationException("Password must be at least 6 characters long.");
        }
    }
}

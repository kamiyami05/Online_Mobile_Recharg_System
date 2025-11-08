using System;
using System.Collections.Generic;
using System.Linq;
using sem3.Models.Entities;

namespace sem3.Models.Repositories
{
    public class UserRepository : IDisposable
    {
        private readonly OnlineRechargeDBEntities _context;

        public UserRepository()
        {
            _context = new OnlineRechargeDBEntities();
        }

        public List<User> GetAll()
        {
            return _context.Users.ToList();
        }

        public User GetById(int id)
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
            var existing = _context.Users.Find(user.UserID);
            if (existing == null) return;

            existing.FullName = user.FullName;
            existing.Email = user.Email;
            existing.MobileNumber = user.MobileNumber;
            existing.Address = user.Address;
            existing.Role = user.Role;

            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

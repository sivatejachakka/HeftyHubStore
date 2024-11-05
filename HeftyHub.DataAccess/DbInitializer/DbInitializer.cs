using HeftyHub.DataAccess.Data;
using HeftyHub.Models;
using HeftyHub.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeftyHub.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public void Initialize()
        {
            // push migrations if they are not applied
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex) {

            }

            // create roles if they are not created
            if (!_roleManager.RoleExistsAsync(Constants.ROLE_USER_CUSTOMER).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(Constants.ROLE_USER_CUSTOMER)).GetAwaiter().GetResult();
            }
            if (!_roleManager.RoleExistsAsync(Constants.ROLE_USER_EMPLOYEE).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(Constants.ROLE_USER_EMPLOYEE)).GetAwaiter().GetResult();
            }
            if (!_roleManager.RoleExistsAsync(Constants.ROLE_USER_ADMIN).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(Constants.ROLE_USER_ADMIN)).GetAwaiter().GetResult();

                // if roles are created, then we will create admin user as well
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "sivateja1213@gmail.com",
                    Email = "sivateja1213@gmail.com",
                    Name = "Siva Teja",
                    PhoneNumber = "1234567890",
                    StreetAddress = "Street Lion 123",
                    City = "Milwaukee",
                    State = "Wisconsin",
                    PostalCode = "382117",
                }, "HeftyHub@123").GetAwaiter().GetResult();

                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@heftyhub.com",
                    Email = "admin@heftyhub.com",
                    Name = "Siva Teja",
                    PhoneNumber = "1234567890",
                    StreetAddress = "Street Lion 123",
                    City = "Milwaukee",
                    State = "Wisconsin",
                    PostalCode = "351817"
                }, "HeftyHub@123").GetAwaiter().GetResult();

                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@heftyhub.com");
                _userManager.AddToRoleAsync(user, Constants.ROLE_USER_ADMIN).GetAwaiter().GetResult();
                user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "sivateja1213@gmail.com");
                _userManager.AddToRoleAsync(user, Constants.ROLE_USER_ADMIN).GetAwaiter().GetResult();
            }
            if (!_roleManager.RoleExistsAsync(Constants.ROLE_USER_COMPANY).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(Constants.ROLE_USER_COMPANY)).GetAwaiter().GetResult();
            }

            return;
        }
    }
}

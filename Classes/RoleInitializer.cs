﻿using AlcantaraNew.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlcantaraNew.Classes
{
    public class RoleInitializer
    {
        public static async Task InitializeAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)//, IConfiguration configuratio)
        {
            try
            {
                string adminEmail = "AdminEmail";
                string password = "AdminPassword";
                if (await roleManager.FindByNameAsync("Admin") == null) await roleManager.CreateAsync(new IdentityRole("Admin"));
                if (await roleManager.FindByNameAsync("High") == null) await roleManager.CreateAsync(new IdentityRole("High"));
                if (await roleManager.FindByNameAsync("Middle") == null) await roleManager.CreateAsync(new IdentityRole("Middle "));
                if (await roleManager.FindByNameAsync("Low") == null) await roleManager.CreateAsync(new IdentityRole("Low"));
                if (await userManager.FindByNameAsync(adminEmail) == null)
                {
                    User admin = new User { Email = adminEmail, UserName = adminEmail, FirstName = "Admin", LastName = "Admin" };
                    IdentityResult result = await userManager.CreateAsync(admin, password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(admin, "Admin");
                    }
                }
            }
            catch (Exception)
            {

            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AngularProjectAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AngularProjectAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<User> UserManager;
        public AccountController(UserManager<User> UserManager)
        {
            this.UserManager = UserManager;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody]RegistrationModel registrationModel)
        {

            var user = new User()
            {
                UserName = registrationModel.UserName,
                Email = registrationModel.Email,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            var result = await UserManager.CreateAsync(user, registrationModel.Password);
            if (result.Succeeded)
            {
                await UserManager.AddToRoleAsync(user, "Normal User");
                return Ok(new { UserName = user.UserName, Email = user.Email });
            }
            return BadRequest(new JsonResult("Nooo"));
        }
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody]LoginModel loginModel)
        {
            var user = await UserManager.FindByNameAsync(loginModel.UserName);
            if(user!=null && await UserManager.CheckPasswordAsync(user, loginModel.Password))
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub,user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                };
                var signingkey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MySuperSecurityKey"));
                var token= new JwtSecurityToken
                (

                   audience:"http://oec.com",
                   issuer: "http://oec.com",
                   expires:DateTime.Now.AddDays(30),
                   claims:claims,
                   signingCredentials:new Microsoft.IdentityModel.Tokens.SigningCredentials(signingkey,SecurityAlgorithms.HmacSha256)
                );
                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration=token.ValidTo});
            }
            return Unauthorized();
        }


    }
}
﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineChat.Data;
using OnlineChat.Models;
using OnlineChat.Services;
using OnlineChat.ViewModels;

namespace OnlineChat.Controllers
{
    /// <summary>
    /// Controller for managing user accounts.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ChatDbContext _dbContext;
        private readonly PasswordService _passwordService;

        public AccountController(ChatDbContext dbContext, PasswordService passwordService)
        {
            _dbContext = dbContext;
            _passwordService = passwordService;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="model">The registration model containing user's email and password.</param>
        /// <returns>ActionResult with status 200 if successful, 400 if model is invalid, or 409 if user already exists.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if user with the same email already exists
            if (_dbContext.Users != null && await _dbContext.Users.AnyAsync(u => u.Email == model.Email))
            {
                return Conflict("User with this email already exists");
            }

            // Check if password matches confirm password
            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest("Password and confirm password do not match");
            }

            // Hash the password and save the user to the database
            if (model.Password == null)
            {
                return BadRequest("Password is required");
            }
            (string hashedPassword, string salt) = _passwordService.HashPassword(model.Password);
            User user = new()
            {
                Email = model.Email,
                Username = model.Username,
                PasswordHash = hashedPassword,
                Salt = salt
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return Ok("User registered successfully");
        }

        /// <summary>
        /// Authenticates a user.
        /// </summary>
        /// <param name="model">The login model containing user's email and password.</param>
        /// <returns>ActionResult with status 200 if successful, 400 if model is invalid, 404 if user not found, or 401 if invalid credentials.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Find user by email or username
            User? user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.EmailOrUsername || u.Username == model.EmailOrUsername);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Check password
            if (!_passwordService.VerifyPassword(model.Password, user.PasswordHash, user.Salt))
            {
                return Unauthorized("Invalid email/username or password");
            }

            // Successful authentication
            return Ok("User logged in successfully");
        }
    }
}
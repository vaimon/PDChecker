using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PDChecker.Models;
using PDChecker.Services;
using PDChecker.utils;

namespace PDChecker.Controllers;

[ApiController]
public class AuthorizationController : Controller
{
    private PdDbContext _context;
    private PasswordHasher<User> _passwordHasher = new();

    public AuthorizationController(PdDbContext context)
    {
        _context = context;
    }
    
    [HttpPost("/login")]
    public IActionResult Login(AuthInfo authInfo)
    {
        var identity = GetIdentity(authInfo.Login, authInfo.Password);
        if (identity == null)
        {
            return BadRequest(new {errorText = "Invalid username or password."});
        }

        var now = DateTime.UtcNow;
        // создаем JWT-токен
        var jwt = new JwtSecurityToken(
            issuer: AuthOptions.ISSUER,
            audience: AuthOptions.AUDIENCE,
            notBefore: now,
            claims: identity.Claims,
            expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(),
                SecurityAlgorithms.HmacSha256));
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        var response = new
        {
            access_token = encodedJwt,
            username = identity.Name,
        };

        return Json(response);
    }

    [HttpGet("/user/{login}")]
    public IActionResult UserInfo(string login)
    {
        var user = _context.Users.FirstOrDefault(u => u.Login == login);
        if (user == null)
        {
            return BadRequest(new {errorText = "User with this login does not exist."});
        }
        
        var response = new
        {
            username = user.Login,
            role = user.Role,
            name = user.Name
        };

        return Json(response);
    }
    
    [HttpPost("/register")]
    public IActionResult Register(NewUser newUser)
    {
        if (_context.Users.Any(u => u.Login == newUser.Login))
        {
            return BadRequest(new {errorText = "User with this login already exists."});
        }
        var user = new User(newUser.Role, newUser.Name, newUser.Login);
        user.Password = _passwordHasher.HashPassword(user, newUser.Password);
        _context.Users.Add(user);
        _context.SaveChanges();
        ClaimsIdentity? identity;
        try
        {
            identity = GetIdentity(newUser.Login, newUser.Password);
        }
        catch (UserNotFoundException e)
        {
            Console.WriteLine(e);
            return BadRequest(new {errorText = "User not found."}); 
        }
        
        if (identity == null)
        {
            return BadRequest(new {errorText = "Invalid password."});
        }

        var now = DateTime.UtcNow;
        // создаем JWT-токен
        var jwt = new JwtSecurityToken(
            issuer: AuthOptions.ISSUER,
            audience: AuthOptions.AUDIENCE,
            notBefore: now,
            claims: identity.Claims,
            expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(),
                SecurityAlgorithms.HmacSha256));
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        var response = new
        {
            access_token = encodedJwt,
            username = identity.Name,
        };

        return Json(response);
    }
    
    private ClaimsIdentity? GetIdentity(string username, string password)
    {
        User? user = _context.Users.FirstOrDefault(u => u.Login == username);
        if (user == null)
        {
            throw new UserNotFoundException();
        }
        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return null;
        }
        var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role)
            };
            ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;
    }
}
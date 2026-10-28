using BankingProject.Data;
using BankingProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;


namespace BankingProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]    
    public class AuthController : ControllerBase
    {
        private readonly DbHelper _dbHelper;
        private readonly PasswordHasher<string> _passwordHasher = new();
        private readonly IConfiguration _configuration;

        public AuthController(DbHelper dbHelper, IConfiguration configuration)
        {
            _dbHelper = dbHelper;
            _configuration = configuration;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public IActionResult Register(RegisterRequest request)
        {
            try
            {
                using SqlConnection con = _dbHelper.GetConnection();
                con.Open();

                // Check whether email already exists
                string checkQuery =
                    "SELECT COUNT(*) FROM Users WHERE Email = @Email";

                using (SqlCommand checkCmd =
                    new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.Add(
                        "@Email", SqlDbType.NVarChar, 100)
                        .Value = request.Email.Trim();

                    int count = (int)checkCmd.ExecuteScalar()!;

                    if (count > 0)
                        return Conflict("Email is already registered.");
                }

                // Hash the password before saving it
                string hashedPassword =
                    _passwordHasher.HashPassword(
                        request.Username, request.Password);

                string insertQuery = @"
                    INSERT INTO Users
                    (Username, Email, E_Password, User_Role)
                    VALUES
                    (@Username, @Email, @Password, @Role)";

                using SqlCommand cmd =
                    new SqlCommand(insertQuery, con);

                cmd.Parameters.Add(
                    "@Username", SqlDbType.NVarChar, 50)
                    .Value = request.Username.Trim();

                cmd.Parameters.Add(
                    "@Email", SqlDbType.NVarChar, 100)
                    .Value = request.Email.Trim();

                cmd.Parameters.Add(
                    "@Password", SqlDbType.NVarChar, 100)
                    .Value = hashedPassword;

                cmd.Parameters.Add(
                    "@Role", SqlDbType.NVarChar, 40)
                    .Value = "Customer";

                cmd.ExecuteNonQuery();

                return Ok("Registration successful.");
            }
            catch (SqlException)
            {
                return StatusCode(
                    500, "Database operation failed.");
            }
        }

        // POST: api/Auth/login

        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            try
            {
                using SqlConnection con = _dbHelper.GetConnection();
                con.Open();

                string query = @"
            SELECT UserId, Username, Email,
                   E_Password, User_Role
            FROM Users
            WHERE Email = @Email";

                using SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.Add(
                    "@Email", SqlDbType.NVarChar, 100)
                    .Value = request.Email.Trim();

                using SqlDataReader reader = cmd.ExecuteReader();

                if (!reader.Read())
                    return Unauthorized("Invalid email or password.");

                int userId = reader.GetInt32(0);
                string username = reader.GetString(1);
                string email = reader.GetString(2);
                string passwordHash = reader.GetString(3);
                string role = reader.GetString(4);

                reader.Close();

                var result = _passwordHasher.VerifyHashedPassword(
                    username,
                    passwordHash,
                    request.Password);

                if (result == PasswordVerificationResult.Failed)
                    return Unauthorized("Invalid email or password.");

                // Generate JWT token
                var claims = new[]
                {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

                var jwtKey = _configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException(
                        "JWT key is missing.");

                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey));

                var credentials = new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256);

                var expiryMinutes = int.Parse(
                    _configuration["Jwt:ExpiryMinutes"] ?? "30");

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                    signingCredentials: credentials);

                string tokenString =
                    new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new
                {
                    Message = "Login successful.",
                    UserId = userId,
                    Username = username,
                    Email = email,
                    Role = role,
                    Token = tokenString
                });
            }
            catch (SqlException)
            {
                return StatusCode(500, "Database operation failed.");
            }
        }

    }

}



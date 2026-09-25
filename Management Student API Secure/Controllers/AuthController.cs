using Business_Layer;
using Management_Student_API_Secure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;

namespace Management_Student_API_Secure.Controllers
{
   // This controller is responsible for authentication-related actions,
     // such as logging in and issuing JWT tokens.
        [ApiController]
        [Route("api/[controller]")]
        public class AuthController : ControllerBase
        {
        // 1. تعريف الحقول
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        // 2. مُنشئ واحد يجمع كل الحقن (Dependency Injection)
        public AuthController(ILogger<AuthController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        private static string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
      
        // This endpoint handles user login.
        // It verifies credentials and returns a JWT token if login succeeds.
        [HttpPost("login")]
        [EnableRateLimiting("AuthLimiter")]
        public IActionResult Login([FromBody] LoginRequest request)
            {
                // Step 1: Find the student by email from the in-memory data store.
                // Email acts as the unique login identifier.
                var user = clsUser.GetAllUsers()
                    .FirstOrDefault(u => u.Email == request.Email);

            // 1. استخراج الـ IP
            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            // If no student is found with the given email,
            // return 401 Unauthorized without revealing which field was wrong.
            if (user == null)
            {

                // 2. تسجيل الحدث في ملفات الـ Logger (كما هو لديك)
                _logger.LogWarning("Failed login attempt (email not found). Email={Email}, IP={IP}", request.Email, ip);

                // 3. تجهيز التفاصيل على شكل JSON
                string detailsJson = $"{{\"reason\": \"Email not found\", \"attemptedEmail\": \"{request.Email}\"}}";

                // 4. حفظ الحدث في قاعدة البيانات (جدول AuditLogs)
                // نمرر null للـ UserId لأن المستخدم غير موجود
                Business_Layer.clsAuditLog.LogEvent(
                    userId: null,
                    action: "LOGIN_FAILED",
                    entityName: "Auth",
                    entityId: null,
                    ipAddress: ip,
                    details: detailsJson
                );
                return Unauthorized("Invalid credentials");
            }
                   


                // Step 2: Verify the provided password against the stored hash.
                // BCrypt handles hashing and salt internally.
                bool isValidPassword =
                    BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);


                // If the password does not match the stored hash,
                // return 401 Unauthorized.
                if (!isValidPassword)
            {  // 2. تسجيل الحدث في ملفات الـ Logger (كما هو لديك)
                _logger.LogWarning("Failed login attempt (password not found). Password={Email}, IP={IP}", request.Password, ip);

                // 3. تجهيز التفاصيل على شكل JSON
                string detailsJson = $"{{\"reason\": \"Password not found\", \"attemptedPassword\": \"{request.Password}\"}}";

                // 4. حفظ الحدث في قاعدة البيانات (جدول AuditLogs)
                // نمرر null للـ UserId لأن المستخدم غير موجود
                Business_Layer.clsAuditLog.LogEvent(
                    userId: null,
                    action: "LOGIN_FAILED",
                    entityName: "Auth",
                    entityId: null,
                    ipAddress: ip,
                    details: detailsJson
                );

                return Unauthorized("Invalid credentials");
            }
                    


                // Step 3: Create claims that represent the authenticated user's identity.
                // These claims will be embedded inside the JWT.
                var claims = new[]
                {
                // Unique identifier for the student
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),


                // Student email address
                new Claim(ClaimTypes.Email, user.Email),


                // Role (Student or Admin) used later for authorization
                new Claim(ClaimTypes.Role, user.Role),

                // الاسم الكامل
    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
    // حالة تفعيل الحساب
    new Claim("IsActive", user.IsActive.ToString().ToLower())

            };


            // قراءة القيم من appsettings.json مباشرة
            var secretKey = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var duration = double.Parse(_configuration["Jwt:DurationInMinutes"] ?? "30");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Step 6: Create the JWT token.
            // The token includes issuer, audience, claims, expiration, and signature.
            var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(duration),
                    signingCredentials: creds
                );


            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            // Create refresh token (random)
            var refreshToken = GenerateRefreshToken();

            // Store refresh token securely (hash + expiry + not revoked)
            user.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            user.RefreshTokenRevokedAt = null;

            clsUser.UpdateRefreshToken(user.Id, user.RefreshTokenHash,user.RefreshTokenExpiresAt.Value);

            return Ok(new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        [HttpPost("refresh")]
        [EnableRateLimiting("AuthLimiter")]
        public IActionResult Refresh([FromBody] RefreshRequest request)
        {
            var user = clsUser.GetAllUsers()
                .FirstOrDefault(u => u.Email == request.Email);

            if (user == null)
                return Unauthorized("Invalid refresh request");

            if (user.RefreshTokenRevokedAt != null)
                return Unauthorized("Refresh token is revoked");

            if (user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
                return Unauthorized("Refresh token expired");

            bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, user.RefreshTokenHash);
            if (!refreshValid)
                return Unauthorized("Invalid refresh token");

            // Issue NEW access token (same claims & signing settings as login)
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role),
              // الاسم الكامل
    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
    // حالة تفعيل الحساب
    new Claim("IsActive", user.IsActive.ToString().ToLower())
    };

            // قراءة القيم من appsettings.json مباشرة
            var secretKey = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var duration = double.Parse(_configuration["Jwt:DurationInMinutes"] ?? "30");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Step 6: Create the JWT token.
            // The token includes issuer, audience, claims, expiration, and signature.
            var jwt = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(duration),
                    signingCredentials: creds
                );

            var newAccessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

            // Rotation: replace refresh token
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            user.RefreshTokenRevokedAt = null;


            clsUser.UpdateRefreshToken(user.Id, user.RefreshTokenHash, user.RefreshTokenExpiresAt.Value);

            return Ok(new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] LogoutRequest request)
        {
            var user = clsUser.GetAllUsers()
                .FirstOrDefault(u => u.Email == request.Email);

            if (user == null)
                return Ok(); // Do not reveal if user exists

            bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, user.RefreshTokenHash);
            if (!refreshValid)
                return Ok();

            clsUser.UpdateRefreshTokenRevoked(user.Id);
            return Ok("Logged out successfully");
        }
    }
    
}

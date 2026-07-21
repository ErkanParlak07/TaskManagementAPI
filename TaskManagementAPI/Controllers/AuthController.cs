using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using TaskManagementAPI.Dtos;

namespace TaskManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        // Veritabanı (AppDbContext) ve Ayarlar (IConfiguration - appsettings.json için) çağrılıyor
        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // --- 1. KAYIT OL (REGISTER) UÇ NOKTASI ---
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            // Kullanıcı adı daha önce alınmış mı kontrol et
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Bu kullanıcı adı zaten alınmış.");
            }

            // Şifreyi BCrypt ile geri döndürülemez şekilde şifrele (Hash)
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Kayıt işlemi başarılı.");
        }

        // --- 2. GİRİŞ YAP (LOGIN) UÇ NOKTASI ---
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            // Veritabanında bu kullanıcı adını ara
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            // 1. DURUM: Kullanıcı veritabanında hiç yoksa
            if (user == null)
            {
                _context.LoginLogs.Add(new LoginLog { Username = request.Username, AttemptDate = DateTime.Now, IsSuccess = false, ErrorMessage = "Kullanıcı Yok" });
                await _context.SaveChangesAsync();
                return BadRequest("Kullanıcı adı veya şifre hatalı.");
            }

            // 2. DURUM: Kullanıcı var ama BCrypt şifresi uyuşmuyorsa
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _context.LoginLogs.Add(new LoginLog { Username = request.Username, AttemptDate = DateTime.Now, IsSuccess = false, ErrorMessage = "Şifre Hatalı" });
                await _context.SaveChangesAsync();
                return BadRequest("Kullanıcı adı veya şifre hatalı.");
            }

            // 3. DURUM: Kullanıcı Soft Delete (Pasif) yemişse
            if (user.IsDeleted)
            {
                _context.LoginLogs.Add(new LoginLog { Username = request.Username, AttemptDate = DateTime.Now, IsSuccess = false, ErrorMessage = "Hesap Pasif" });
                await _context.SaveChangesAsync();
                return BadRequest("Bu hesap sistem yöneticisi tarafından askıya alınmış veya silinmiştir.");
            }

            // 4. DURUM: Her şey doğruysa (Başarılı Giriş)
            _context.LoginLogs.Add(new LoginLog { Username = request.Username, AttemptDate = DateTime.Now, IsSuccess = true, ErrorMessage = "Başarılı" });
            await _context.SaveChangesAsync();

            // Her şey doğruysa kullanıcıya özel dijital anahtar (Token) üret
            string token = CreateToken(user);

            return Ok(new { Token = token });
        }

        // --- 3. JWT ÜRETME (TOKEN GENERATION) METODU ---
        private string CreateToken(User user)
        {
            // Token'ın içine gömeceğimiz kimlik bilgileri (Kimliği ve Kullanıcı Adı)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            // appsettings.json'dan gizli anahtarımızı çekiyoruz
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("Jwt:Key").Value!));
                
            // Şifreleme algoritmasını belirliyoruz
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Token'ın ne zaman süresinin dolacağını ve içeriğini oluşturuyoruz (Örn: 1 Gün)
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            // Token'ı metin formatına çevirip geri döndürüyoruz
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            
            return jwt;
        }
    }
}
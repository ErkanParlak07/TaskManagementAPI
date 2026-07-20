using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // İŞTE SİHİRLİ KİLİT BURADA: Bu sınıfın içindeki hiçbir şeye 'Admin' rolü olmayanlar giremez!
    [Authorize(Roles = "Admin")] 
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Tüm kullanıcıları getirir (Pasif veya Aktif filtrelemesini JS yapacak)
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // 2. Kullanıcı aktifse pasif, pasifse aktif yapar
        [HttpPut("toggle-status/{id}")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            // Durumu tersine çevirir
            user.IsDeleted = !user.IsDeleted; 
            
            await _context.SaveChangesAsync();
            return Ok();
        }
        // 3. Kullanıcı verilerini günceller
        [HttpPut("update-user/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            // Gelen yeni verileri, veritabanındaki kullanıcıya aktarıyoruz
            user.Username = request.Username;
            user.Email = request.Email;
            user.Role = request.Role;
            // Eğer admin formda şifre kısmına bir şey yazdıysa şifreyi güncelle!
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }
            await _context.SaveChangesAsync();
            return Ok(new { message = "Kullanıcı başarıyla güncellendi." });
        }
        // 4. Belirli bir kullanıcının görevlerini getirir
        [HttpGet("user-tasks/{id}")]
        public async Task<IActionResult> GetUserTasks(int id)
        {
            var user = await _context.Users
                                     .Include(u => u.Tasks) 
                                     .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            //  Sonsuz döngüyü engellemek için sadece gereken verileri seçiyoruz
            var taskList = user.Tasks.Select(t => new 
            {
                id = t.Id,
                title = t.Title, // DİKKAT: C# modelinde görev adı Title değil de Name ise burayı t.Name yap
                isCompleted = t.IsCompleted,
                statusName = t.Status.ToString()
            }).ToList();

            return Ok(taskList);
        }
       // 5. Belirli bir kullanıcıya admin tarafından görev atama
        [HttpPost("user-tasks/{userId}")]
        public async Task<IActionResult> AssignTaskToUser(int userId, [FromBody] AdminAssignTaskDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            var newTask = new TaskItem 
            {
                Title = dto.Title,
                Description = dto.Description ?? string.Empty, // Detay boş bırakılırsa hata vermesin
                Priority = (TaskPriority)dto.Priority, // Öncelik Enum'a çevriliyor
                UserId = userId,
                CreatedDate = DateTime.UtcNow,
                IsCompleted = false
            };

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Görev başarıyla atandı." });
        }

        // Güncellenmiş DTO (Artık detay ve öncelik de alıyor)
        public class AdminAssignTaskDto
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public int Priority { get; set; } // 0: Düşük, 1: Orta, 2: Yüksek
        }
}}
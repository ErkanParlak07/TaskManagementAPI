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
                Description = t.Description, 
                Priority = (int)t.Priority,
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
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; } // Bu eksik olabilir
            public int Priority { get; set; }       
            public bool IsCompleted { get; set; }
        }
        [HttpGet("logs")]
[Authorize(Roles = "Admin")] // Sadece adminler görebilsin
public async Task<IActionResult> GetSecurityLogs()
{
    // Son 10 başarılı giriş
    var recentLogins = await _context.LoginLogs
        .Where(l => l.IsSuccess)
        .OrderByDescending(l => l.AttemptDate)
        .Take(10)
        .ToListAsync();

    // Son 10 hatalı deneme
    var failedLogins = await _context.LoginLogs
        .Where(l => !l.IsSuccess)
        .OrderByDescending(l => l.AttemptDate)
        .Take(10)
        .ToListAsync();

    return Ok(new { recentLogins, failedLogins });
}
// 1. GÖREV GÜNCELLEME METODU (PUT)
[HttpPut("tasks/{id}")]
public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskItem model) 
{
    // Görevi veritabanında bul
    var task = await _context.Tasks.FindAsync(id);
    if (task == null)
    {
        return NotFound("Görev bulunamadı.");
    }

    // Yeni değerleri üzerine yaz
    task.Title = model.Title;
    task.Description = model.Description;
    task.Priority = model.Priority;
    task.IsCompleted = model.IsCompleted;
    task.Status = model.Status;

    // Veritabanını güncelle
    _context.Tasks.Update(task);
    await _context.SaveChangesAsync();

    return Ok(new { message = "Görev başarıyla güncellendi." });
}
// 2. GÖREV SİLME METODU (DELETE)
[HttpDelete("tasks/{id}")]
public async Task<IActionResult> DeleteTask(int id)
{
    // Görevi veritabanında bul
    var task = await _context.Tasks.FindAsync(id);
    if (task == null)
    {
        return NotFound("Silinecek görev bulunamadı.");
    }

    // Görevi veritabanından tamamen sil (Eğer isDeleted kullanıyorsan soft-delete de yapabilirsin)
    _context.Tasks.Remove(task);
    await _context.SaveChangesAsync();

    return Ok(new { message = "Görev başarıyla silindi." });
}
}}
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementAPI.Dtos;
using TaskManagementAPI.Models;
using TaskManagementAPI.Services;
using System.Security.Claims;
using TaskManagementAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace TaskManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
       private readonly AppDbContext _context;
        private readonly ITaskService _taskService;

        // İki bağımlılığı da TEK BİR yapıcı metot (Constructor) içinde aynı anda alıyoruz
        public TasksController(AppDbContext context, ITaskService taskService)
        {
            _context = context;
            _taskService = taskService;
        }

       [HttpGet]
public async Task<IActionResult> GetTasks()
{
    // 1. Sisteme giriş yapmış olan kullanıcının ID'sini Token'dan okuyoruz
    // Token'dan ID okuma satırının son hali (Sarı uyarıyı yok eder)
var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // 2. Veritabanından SADECE bu kullanıcıya ait olan görevleri çekiyoruz!
    var tasks = await _context.Tasks.Where(t => t.UserId == userId).ToListAsync();
    
    return Ok(tasks);
}
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponseDto>> GetById(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound(new { Message = $"{id} numaralı görev bulunamadı." });
            }
            return Ok(task);
        }

        [HttpPost]
public async Task<IActionResult> CreateTask(CreateTaskDto taskDto) // DTO adın neyse ona göre düzelt
{
    // 1. Yine Token'dan kullanıcının ID'sini yakalıyoruz
    // Token'dan ID okuma satırının son hali (Sarı uyarıyı yok eder)
var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    var newTask = new TaskItem // Kendi modelinin adıyla (Örn: TaskItem) değiştir
    {
        Title = taskDto.Title,
        Description = taskDto.Description,
        Priority = taskDto.Priority,
        Status = 0,
        UserId = userId // İŞTE HATAYI ÇÖZEN SİHİRLİ SATIR BURASI! Görevi kullanıcıya bağlıyoruz.
    };

    _context.Tasks.Add(newTask);
    await _context.SaveChangesAsync();

    return Ok(newTask);
}

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto updateTaskDto)
        {
            var updatedTask = await _taskService.UpdateTaskAsync(id, updateTaskDto);
            if (updatedTask == null)
            {
                return NotFound(new { Message = $"{id} numaralı görev bulunamadı, güncelleme başarısız." });
            }
            return Ok(updatedTask);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _taskService.DeleteTaskAsync(id);
            if (!result)
            {
                return NotFound(new { Message = $"{id} numaralı görev bulunamadı." });
            }
            return NoContent();
        }
    }
}
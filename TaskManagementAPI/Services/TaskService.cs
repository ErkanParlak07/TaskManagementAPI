using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Dtos;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Services
{
    public class TaskService : ITaskService
    {
        // Veritabanı bağlantımızı tutacağımız değişken
        private readonly AppDbContext _context;

        // Constructor Injection: .NET bu sınıfı her çağırdığında arka planda veritabanı köprümüzü (AppDbContext) buraya otomatik olarak enjekte edecek.
        public TaskService(AppDbContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME VE FİLTRELEME
        public async Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync(Models.TaskStatus? status, TaskPriority? priority)
        {
            // Veritabanındaki 'Tasks' tablosuna bir sorgu hazırlamaya başlıyoruz (Henüz SQL'e gitmedi)
            var query = _context.Tasks.AsQueryable();

            // Eğer kullanıcı bir durum (status) filtresi gönderdiyse sorguya WHERE şartı ekle
            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            // Eğer kullanıcı bir öncelik (priority) filtresi gönderdiyse sorguya WHERE şartı ekle
            if (priority.HasValue)
            {
                query = query.Where(t => t.Priority == priority.Value);
            }

            // ToListAsync() komutu geldiği an, yukarıda hazırladığımız sorgu gerçek bir SQL SELECT sorgusuna dönüşüp veritabanına gider.
            var tasks = await query.ToListAsync();

            // Veritabanı modelimizi doğrudan dışarı açmıyor, kullanıcının göreceği DTO (TaskResponseDto) formatına dönüştürüp yolluyoruz.
            return tasks.Select(t => MapToResponseDto(t));
        }

        // 2. TEKİL DETAY GETİRME
        public async Task<TaskResponseDto?> GetTaskByIdAsync(int id)
        {
            // Primary Key (Id) üzerinden hızlıca veri arama işlemi
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return null;

            return MapToResponseDto(task);
        }

        // 3. YENİ GÖREV EKLEME
        public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto createTaskDto)
        {
            // Kullanıcıdan gelen DTO'yu, veritabanı için anlamlı olan TaskItem modeline çeviriyoruz
            var taskItem = new TaskItem
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                Priority = createTaskDto.Priority,
                Status = Models.TaskStatus.Todo, // Yeni başlayan görev otomatik olarak Todo'dur.
                CreatedDate = DateTime.UtcNow
            };

            // Entity Framework'e "Bunu eklenecekler listesine al" diyoruz
            _context.Tasks.Add(taskItem);
            
            // SaveChangesAsync() çağrıldığında gerçek "INSERT INTO" SQL sorgusu çalışır
            await _context.SaveChangesAsync();

            return MapToResponseDto(taskItem);
        }

        // 4. GÜNCELLEME
        public async Task<TaskResponseDto?> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return null;

            // Bulunan kaydın değerlerini, kullanıcının gönderdiği yeni DTO değerleriyle eziyoruz
            task.Title = updateTaskDto.Title;
            task.Description = updateTaskDto.Description;
            task.Status = updateTaskDto.Status;
            task.Priority = updateTaskDto.Priority;

            // "UPDATE" SQL sorgusunu çalıştır
            await _context.SaveChangesAsync();
            return MapToResponseDto(task);
        }

        // 5. SİLME
        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return false;

            _context.Tasks.Remove(task);
            
            // "DELETE FROM" SQL sorgusunu çalıştır
            await _context.SaveChangesAsync();
            return true;
        }

        // YARDIMCI METOT: Veritabanı modelini DTO'ya dönüştürür
        private static TaskResponseDto MapToResponseDto(TaskItem task)
        {
            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(), // Enum olan "0" değerini "Todo" metnine çevirir
                Priority = task.Priority.ToString(),
                CreatedDate = task.CreatedDate
            };
        }
    }
}
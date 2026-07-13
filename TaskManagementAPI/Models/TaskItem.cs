using System;

namespace TaskManagementAPI.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // Yukarıda oluşturduğumuz Enum'ları burada kullanıyoruz
        public TaskStatus Status { get; set; } = TaskStatus.Todo;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
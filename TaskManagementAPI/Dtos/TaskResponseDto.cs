using System;

namespace TaskManagementAPI.Dtos
{
    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // Kullanıcı sayı değil, metin görsün diye bunları string olarak ayarlıyoruz
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; }
    }
}
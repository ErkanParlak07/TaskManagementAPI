using System.ComponentModel.DataAnnotations;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Dtos
{
    public class CreateTaskDto
    {
        // [Required]: Bu alanın boş bırakılamayacağını belirtir. API otomatik hata döner.
        [Required(ErrorMessage = "Görev başlığı boş bırakılamaz.")]
        // [StringLength]: Karakter sınırını belirler.
        [StringLength(100, ErrorMessage = "Baslik en fazla 100 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Görev açıklaması boş bırakılamaz.")]
        public string Description { get; set; } = string.Empty;

        // Kullanıcı görev eklerken önceliğini belirleyebilir (Varsayılan olarak Medium atanır)
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    }
}
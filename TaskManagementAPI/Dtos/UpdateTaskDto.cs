using System.ComponentModel.DataAnnotations;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Dtos
{
    public class UpdateTaskDto
    {
        [Required(ErrorMessage = "Görev başlığı boş bırakılamaz.")]
        [StringLength(100, ErrorMessage = "Başlık en fazla 100 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Görev açıklaması boş bırakılamaz.")]
        public string Description { get; set; } = string.Empty;

        // Güncelleme işleminde hem Durum hem de Öncelik değiştirilebilir
        public Models.TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
    }
}
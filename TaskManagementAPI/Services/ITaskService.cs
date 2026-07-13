using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagementAPI.Dtos;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Services
{
    // Arayüzler (Interfaces) "I" harfi ile başlar. 
    // Bu dosya bir sözleşmedir. Servisimizin hangi işlevleri yerine getirmek ZORUNDA olduğunu listeler.
    public interface ITaskService
    {
        // Tüm görevleri getir (Filtreleme parametreleri alabilir)
        Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync(Models.TaskStatus? status, TaskPriority? priority);
        
        // Tek bir görevi ID'sine göre getir
        Task<TaskResponseDto?> GetTaskByIdAsync(int id);
        
        // Yeni görev oluştur
        Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto createTaskDto);
        
        // Görevi güncelle
        Task<TaskResponseDto?> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto);
        
        // Görevi sil (Başarılıysa true, bulamazsa false döner)
        Task<bool> DeleteTaskAsync(int id);
    }
}
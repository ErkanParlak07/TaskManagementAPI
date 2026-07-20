using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Data
{
    // DbContext, Entity Framework'ün kalbidir. Veritabanı ile uygulamamız arasındaki ana köprüdür.
    public class AppDbContext : DbContext
    {
        // Constructor (Yapıcı Metot): Dışarıdan veritabanı ayarlarını (hangi veritabanına bağlanılacağı vb.) alır ve temel sınıfa (base) iletir.
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet<...>, veritabanındaki bir Tabloyu temsil eder. 
        // Burada Entity Framework'e diyoruz ki: "TaskItem modelimi al, veritabanında 'Tasks' adında bir tabloya dönüştür."
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
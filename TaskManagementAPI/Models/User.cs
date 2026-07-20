namespace TaskManagementAPI.Models // Kendi projenin namespace'ine göre ayarla
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        // Şifreyi asla düz metin (örn: "123456") tutmayız, şifrelenmiş (Hash) halini tutacağız
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public bool IsDeleted { get; set; } = false;

        // İlişki (Relation): Bir kullanıcının birden fazla görevi olabilir
       
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>(); 
    }
}
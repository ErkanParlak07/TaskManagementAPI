using System;

namespace TaskManagementAPI.Models // Kendi namespace'in ile aynı olduğuna emin ol
{
    public class LoginLog
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public DateTime AttemptDate { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } // "Başarılı", "Şifre Hatalı", "Kullanıcı Yok" vb.
    }
}
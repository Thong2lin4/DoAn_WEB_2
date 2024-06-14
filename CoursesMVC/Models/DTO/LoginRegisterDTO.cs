using System.ComponentModel.DataAnnotations;

namespace CoursesMVC.Models.DTO
{
    public class LoginRegisterDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string[] Roles { get; set; }
    }
}

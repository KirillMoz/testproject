using System.ComponentModel.DataAnnotations;

namespace testproject.ViewModels
{
    public class UserViewModel
    {
        [Display(Name = "ID пользователя")]
        public string? Id { get; set; }

        [Display(Name = "Имя пользователя")]
        public string? UserName { get; set; }

        [Display(Name = "Email")]
        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name = "Имя")]
        public string? FirstName { get; set; }

        [Display(Name = "Фамилия")]
        public string? LastName { get; set; }

        [Display(Name = "Номер телефона")]
        [Phone]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Дата создания")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Роли")]
        public List<string> Roles { get; set; } = new List<string>();
    }
}

using System.ComponentModel.DataAnnotations;

namespace testproject.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Имя обязательно")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилия обязательна")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        [StringLength(100, ErrorMessage = "Пароль должен быть от {2} до {1} символов", MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите день")]
        [Display(Name = "День")]
        public int? BirthDay { get; set; }

        [Required(ErrorMessage = "Выберите месяц")]
        [Display(Name = "Месяц")]
        public int? BirthMonth { get; set; }

        [Required(ErrorMessage = "Выберите год")]
        [Display(Name = "Год")]
        public int? BirthYear { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace testproject.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Имя обязательно")]
        [Display(Name = "Имя")]
        [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна")]
        [Display(Name = "Фамилия")]
        [StringLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный email адрес")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Никнейм обязателен")]
        [Display(Name = "Никнейм")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Никнейм должен быть от 3 до 20 символов")]
        public string? Login { get; set; }

        [Required(ErrorMessage = "Год рождения обязателен")]
        [Display(Name = "Год")]
        [Range(1900, 2024, ErrorMessage = "Год должен быть от 1900 до 2024")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Месяц обязателен")]
        [Display(Name = "Месяц")]
        [Range(1, 12, ErrorMessage = "Месяц должен быть от 1 до 12")]
        public int Month { get; set; }

        [Required(ErrorMessage = "День обязателен")]
        [Display(Name = "День")]
        [Range(1, 31, ErrorMessage = "День должен быть от 1 до 31")]
        public int Date { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Пароль должен быть от 5 до 100 символов")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Подтверждение пароля обязательно")]
        [DataType(DataType.Password)]
        [Display(Name = "Подтвердить пароль")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string? PasswordConfirm { get; set; }

        // Свойство для объединенной даты рождения (не обязательно, можно вычислять)
        public DateTime? BirthDate => ValidateDate() ? new DateTime(Year, Month, Date) : null;

        private bool ValidateDate()
        {
            try
            {
                var date = new DateTime(Year, Month, Date);
                return date <= DateTime.Now && date.Year >= 1900;
            }
            catch
            {
                return false;
            }
        }
    }
}

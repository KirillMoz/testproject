using System.ComponentModel.DataAnnotations;

namespace testproject.ViewModels
{
    public class EditProfileViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        [Display(Name = "Имя")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилия обязательна")]
        [Display(Name = "Фамилия")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "О себе")]
        [StringLength(500)]
        public string? Bio { get; set; }

        [Display(Name = "Город")]
        [StringLength(100)]
        public string? Location { get; set; }

        [Display(Name = "Сайт")]
        [Url(ErrorMessage = "Введите корректный URL")]
        [StringLength(200)]
        public string? Website { get; set; }

        [Display(Name = "Аватар")]
        public IFormFile? AvatarFile { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
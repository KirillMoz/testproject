using Microsoft.AspNetCore.Identity;

namespace testproject.Models
{
    public class User: IdentityUser
    {
        [PersonalData]
        public string? FirstName { get; set; }

        [PersonalData]
        public string? LastName { get; set; }

        [PersonalData]
        public DateTime? BirthDate { get; set; }

        // Уже есть в IdentityUser: Email, UserName (используем как Login)
        // UserName будем использовать для Login (никнейма)

        // Дополнительные свойства если нужны
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
    }
}

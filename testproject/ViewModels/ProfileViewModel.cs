using System.ComponentModel.DataAnnotations;
using testproject.Models;

namespace testproject.ViewModels
{
    public class ProfileViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Дата рождения")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Display(Name = "О себе")]
        public string? Bio { get; set; }

        [Display(Name = "Город")]
        public string? Location { get; set; }

        [Display(Name = "Сайт")]
        [Url]
        public string? Website { get; set; }

        [Display(Name = "Аватар")]
        public string? AvatarUrl { get; set; }

        public bool IsCurrentUser { get; set; }
        public bool IsFriend { get; set; }
        public FriendRequestStatus? FriendRequestStatus { get; set; }

        // Статистика
        public int FriendsCount { get; set; }
        public int PostsCount { get; set; }

        // Посты пользователя
        public List<PostViewModel> Posts { get; set; } = new List<PostViewModel>();
    }

    public class PostViewModel
    {
        public int Id { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorAvatar { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int LikesCount { get; set; }
        public bool CanEdit { get; set; }
    }
}
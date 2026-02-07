using System;
using System.ComponentModel.DataAnnotations;

namespace testproject.Models
{
    /// <summary>
    /// Модель поста на стене пользователя
    /// Пост может быть как на своей стене, так и на стене друга
    /// </summary>
    public class Post
    {
        public int Id { get; set; }

        public int AuthorId { get; set; } // ID автора поста
        public int? WallOwnerId { get; set; } // ID владельца стены (null если на своей стене)

        [Required]
        [StringLength(5000)] // Ограничение длины поста
        public string Content { get; set; } = string.Empty; // Текст поста

        public DateTime CreatedDate { get; set; } // Дата создания
        public int LikesCount { get; set; } = 0; // Количество лайков

        // Навигационные свойства
        public virtual User Author { get; set; } // Автор поста
        public virtual User? WallOwner { get; set; } // Владелец стены (может быть null)
    }
}
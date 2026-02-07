namespace testproject.ViewModels
{
    public class MessageViewModel
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string? SenderAvatar { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentDate { get; set; }
        public bool IsRead { get; set; }
        public bool IsCurrentUserSender { get; set; }
    }

    public class ChatViewModel
    {
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string? OtherUserAvatar { get; set; }
        public bool IsOnline { get; set; }
        public List<MessageViewModel> Messages { get; set; } = new List<MessageViewModel>();
        public string NewMessage { get; set; } = string.Empty;
    }

    public class DialogViewModel
    {
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string? OtherUserAvatar { get; set; }
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastMessageDate { get; set; }
        public int UnreadCount { get; set; }
        public bool IsOnline { get; set; }
    }
}
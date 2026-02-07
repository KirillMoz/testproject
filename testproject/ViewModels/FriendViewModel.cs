namespace testproject.ViewModels
{
    public class FriendViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Location { get; set; }
        public DateTime? LastOnline { get; set; }
        public bool IsOnline { get; set; }
    }

    public class FriendRequestViewModel
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string? SenderAvatar { get; set; }
        public DateTime SentDate { get; set; }
    }

    public class SearchResultsViewModel
    {
        public string Query { get; set; } = string.Empty;
        public List<FriendViewModel> Users { get; set; } = new List<FriendViewModel>();
    }
}
using PortalBeritaApp.Models.Enums;

namespace PortalBeritaApp.ViewModels.Admin
{
    public class AdminUserListViewModel
    {
        public List<AdminUserItemViewModel> Users { get; set; } = new();
        public string? RoleFilter { get; set; }
        public string? SearchQuery { get; set; }
        public List<string> AllRoles { get; set; } = new();
    }

    public class AdminUserItemViewModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string CurrentRole { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ArticlesCount { get; set; }
        public int CommentsCount { get; set; }
    }

    public class AdminCommentListViewModel
    {
        public List<AdminCommentItemViewModel> Comments { get; set; } = new();
        public CommentStatus? StatusFilter { get; set; }
        public string? SearchQuery { get; set; }
    }

    public class AdminCommentItemViewModel
    {
        public Guid Id { get; set; }
        public string ArticleTitle { get; set; } = string.Empty;
        public string ArticleSlug { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public CommentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

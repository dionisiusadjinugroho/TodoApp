using Newtonsoft.Json;

namespace TodoBlazorApp.Models
{
    public partial class User
    {
        public int UserId { get; set; }

        public string Name { get; set; } = null!;

        public string Username { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string? CreatedDate { get; set; }

        public string? UpdatedDate { get; set; }
    }
}

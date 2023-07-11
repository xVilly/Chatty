using System.ComponentModel.DataAnnotations;

namespace Chatty.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Permission { get; set; } = 0;
    }
}

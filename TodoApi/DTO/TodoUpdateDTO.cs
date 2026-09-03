using System.ComponentModel.DataAnnotations;

namespace TodoApi.DTO
{
    public class TodoUpdateDTO
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public bool IsCompleted { get; set; }
    }
}

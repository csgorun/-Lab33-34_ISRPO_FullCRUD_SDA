using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
namespace NotesApp.Models;

public class Note
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;  

    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; 

    public bool IsPinned { get; set; } = false;
    public bool IsArchived { get; set; } = false;

    [Range(1, 5)]
    public int Priority { get; set; } = 3; 

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!; 
}
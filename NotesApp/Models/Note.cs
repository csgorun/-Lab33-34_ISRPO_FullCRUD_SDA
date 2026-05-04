using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
namespace NotesApp.Models;

public class Note
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null;
    public DateTime CrreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdateAt { get; set; } = DateTime.UtcNow;
    public bool IsPinned { get; set; } = false;
    public bool IsArchived { get; set; } = false;
}
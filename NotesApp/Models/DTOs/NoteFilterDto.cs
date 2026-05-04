using System.ComponentModel.DataAnnotations;

namespace NotesApp.Models.DTOs;

public class NoteFilterDto
{
    public bool? Archived { get; set; }
    public int? CategoryId { get; set; }
    public bool? IsPinned { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public bool Descending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
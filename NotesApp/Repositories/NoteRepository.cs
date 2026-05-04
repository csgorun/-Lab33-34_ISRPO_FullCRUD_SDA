using Microsoft.EntityFrameworkCore;
using NotesApp.Data;
using NotesApp.Models;
using NotesApp.Models.DTOs;

namespace NotesApp.Repositories;

public class NoteRepository : INoteRepository
{
    private readonly AppDbContext _db;

    public NoteRepository(AppDbContext db)
    {
        _db = db;
    }

    // 4.1. GetAllAsync: динамическая фильтрация, сортировка, пагинация и проекция в DTO
    public async Task<IEnumerable<NoteResponseDto>> GetAllAsync(NoteFilterDto filter)
    {
        var query = _db.Notes
            .Include(n => n.Category)
            .Where(n => !n.IsArchived) // По умолчанию скрываем архивные
            .AsQueryable();

        if (filter.Archived.HasValue)
            query = query.Where(n => n.IsArchived == filter.Archived);

        if (filter.CategoryId.HasValue)
            query = query.Where(n => n.CategoryId == filter.CategoryId.Value);

        if (filter.IsPinned.HasValue)
            query = query.Where(n => n.IsPinned == filter.IsPinned.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(n => 
                n.Title.ToLower().Contains(search) || 
                n.Content.ToLower().Contains(search));
        }

        query = filter.SortBy.ToLower() switch
        {
            "title" => filter.Descending ? query.OrderByDescending(n => n.Title) : query.OrderBy(n => n.Title),
            "priority" => filter.Descending ? query.OrderByDescending(n => n.Priority) : query.OrderBy(n => n.Priority),
            "updatedat" => filter.Descending ? query.OrderByDescending(n => n.UpdatedAt) : query.OrderBy(n => n.UpdatedAt),
            _ => filter.Descending ? query.OrderByDescending(n => n.CreatedAt) : query.OrderBy(n => n.CreatedAt),
        };

        // Закреплённые всегда сверху, архивные всегда внизу
        query = query.OrderByDescending(n => n.IsPinned)
                     .ThenBy(n => n.IsArchived);

        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 50);

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NoteResponseDto
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt,
                IsPinned = n.IsPinned,
                IsArchived = n.IsArchived,
                Priority = n.Priority,
                CategoryId = n.CategoryId,
                CategoryName = n.Category != null ? n.Category.Name : null,
                CategoryColor = n.Category != null ? n.Category.Color : null
            })
            .ToListAsync();
    }

    // 4.2. GetByIdAsync: получение одной заметки с данными категории
    public async Task<NoteResponseDto?> GetByIdAsync(int id)
    {
        return await _db.Notes
            .Include(n => n.Category)
            .Where(n => n.Id == id)
            .Select(n => new NoteResponseDto
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt,
                IsPinned = n.IsPinned,
                IsArchived = n.IsArchived,
                Priority = n.Priority,
                CategoryId = n.CategoryId,
                CategoryName = n.Category != null ? n.Category.Name : null,
                CategoryColor = n.Category != null ? n.Category.Color : null
            })
            .FirstOrDefaultAsync();
    }

    // 4.3. CreateAsync: создание заметки
    public async Task<Note> CreateAsync(Note note)
    {
        _db.Notes.Add(note);
        await _db.SaveChangesAsync();
        return note;
    }

    // 4.4. UpdateAsync: обновление с автоматическим UpdatedAt
    public async Task<Note> UpdateAsync(Note note)
    {
        note.UpdatedAt = DateTime.UtcNow;
        _db.Notes.Update(note);
        await _db.SaveChangesAsync();
        return note;
    }

    // 4.5. DeleteAsync: удаление
    public async Task DeleteAsync(Note note)
    {
        _db.Notes.Remove(note);
        await _db.SaveChangesAsync();
    }

    // 4.6. FindAsync: быстрая проверка по первичному ключу (для контроллера PUT/PATCH)
    public async Task<Note?> FindAsync(int id)
    {
        return await _db.Notes.FindAsync(id);
    }

    // 4.7. GetCountByCategoryAsync: подсчёт заметок в категории
    public async Task<int> GetCountByCategoryAsync(int categoryId)
    {
        return await _db.Notes.CountAsync(n => n.CategoryId == categoryId && !n.IsArchived);
    }
}
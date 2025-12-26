using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models.Domain;

namespace TodoApi.Repositories
{
    public class TodoRepository : ITodoRepository
    {
        private readonly ApplicationDbContext _context;
        public TodoRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<TodoItem>> GetAllForUserAsync(int userId)
        {
            return await _context.TodoItems
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }
        public async Task<TodoItem?> GetByIdAsync(int id)
        {
            return await _context.TodoItems.FindAsync(id);
        }
        public async Task AddAsync(TodoItem item)
        {
            await _context.TodoItems.AddAsync(item);
        }

        public Task UpdateAsync(TodoItem item)
        {
            _context.TodoItems.Update(item);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(TodoItem item)
        {
            _context.TodoItems.Remove(item);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

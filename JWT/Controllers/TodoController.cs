using Edu_plat.Model;
using JWT;
using JWT.DATA;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Edu_plat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public TodoController(ApplicationDbContext context, UserManager<ApplicationUser> usermangaer)
        {
            _context = context;
            _userManager = usermangaer;
        }

        [HttpGet("GetAll/{userId}")]

        public async Task<IActionResult> GetAllItems(string userId)
        {
            var checkuser=User.FindFirstValue(userId);
            var UserChecked= await _userManager.FindByIdAsync(userId);
            if (UserChecked == null)
            {
                return NotFound(new { success=false ,message= "User not found" });
            }

            if (_context.TodoItems != null)
            {
                var todoItems = await _context.TodoItems.Where(td => td.ApplicationUserId == userId).ToListAsync();
                return Ok(todoItems);
            }

            return BadRequest(new {success=false,message="No items are here."});
        }

        [HttpPost("Add-new-todo/{userId}")]
        public async Task<IActionResult> AddItem(string userId, [FromBody] TodoItems itemFromUser)
        {
            var checkuser = User.FindFirstValue(userId);
            var UserChecked = await _userManager.FindByIdAsync(userId);
            if (UserChecked == null)
            {
                return NotFound(new {success=false,message= "User not found" });
            }


            if (itemFromUser == null)
            {
                return BadRequest(new { success=false,message= "Item couldn't be null " });
            }

            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { success=false ,message= "User not found" });
                }

                var todoItem = new TodoItems
                {
                    Title = itemFromUser.Title,
                    Description = itemFromUser.Description,
                    CreationDate = itemFromUser.CreationDate,
                    isDone = itemFromUser.isDone,
                    ApplicationUserId = userId
                };

                _context.TodoItems.Add(todoItem);
                await _context.SaveChangesAsync();
                return Ok(new {success=true ,message="Item added to the User Successfully"});
            }

            return BadRequest(new { success = false, message = "Item couldn't be added." });
        }

        [HttpDelete("Delete/{todoId}/{userId}")]
        public async Task<IActionResult> DeleteItem(int todoId, string userId)
        {

            var checkuser = User.FindFirstValue(userId);
            var UserChecked = await _userManager.FindByIdAsync(userId);
            if (UserChecked == null)
            {
                return NotFound(new {success=false ,message=""});
            }

            
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return BadRequest("User was not found.");
                }

                var todoItem = await _context.TodoItems
                    .FirstOrDefaultAsync(td => td.Id == todoId && td.ApplicationUserId == userId);

                if (todoItem == null)
                {
                    return NotFound("Todo item not found.");
                }

                _context.TodoItems.Remove(todoItem);
                await _context.SaveChangesAsync();

                return Ok("Item deleted successfully.");
            }

            return BadRequest("Item was not deleted.");
        }

        [HttpGet("sort-by-date/{userId}")]
        public async Task<IActionResult> SortedItems(string userId)
        {

            var checkuser = User.FindFirstValue(userId);
            var UserChecked = await _userManager.FindByIdAsync(userId);
            if (UserChecked == null)
            {
                return NotFound("User not found");
            }

            if (ModelState.IsValid)
            {
                var user = await _context.Users.Include(u => u.todoItems)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return BadRequest("User not found or wrong todoItem id.");
                }

                var sortedItems = user.todoItems.OrderByDescending(td => td.CreationDate).ToList();

                return Ok(sortedItems);
            }
            return BadRequest("No items found.");
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] TodoItems updatedItem)
        {
            if (updatedItem == null)
            {
                return BadRequest("Item cannot be null.");
            }

            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound("Item not found.");
            }

            todoItem.Title = updatedItem.Title;
            todoItem.Description = updatedItem.Description;
            todoItem.CreationDate = updatedItem.CreationDate;
            todoItem.isDone = updatedItem.isDone;

            _context.TodoItems.Update(todoItem);
            await _context.SaveChangesAsync();

            return Ok("Item updated successfully.");
        }
    }
}

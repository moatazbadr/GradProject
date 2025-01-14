
using Edu_plat.DTO.To_doDto;
using Edu_plat.Model;
using JWT;
using JWT.DATA;
using Microsoft.AspNetCore.Authorization;
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

        public TodoController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        #region Getting all items
        [Authorize(Roles = "Student")]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllItems()
        {
            var userId = User.FindFirstValue("AppicationUserId");
            var user = await GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            var todoItems = await _context.TodoItems
                .Where(td => td.ApplicationUserId == userId)
                .ToListAsync();
            List<ToDoDto> toDo = new List<ToDoDto>();
            foreach (var todoItem in todoItems)
            {
                ToDoDto tododto = new ToDoDto()
                {
                    Id = todoItem.Id,
                    title = todoItem.Title,
                    IsDone = todoItem.isDone,
                    CreationDate = todoItem.CreationDate,
                    Description = todoItem.Description,
                }
                ;
                toDo.Add(tododto);
            }


            //if (todoItems == null || todoItems.Count == 0)
            //{
            //    return NotFound(new { success = false, message = "No items are here." });
            //}

            return Ok(toDo);
        }
        #endregion

        #region Add item
        [Authorize(Roles = "Student")]
        [HttpPost("Add-new-todo")]
        public async Task<IActionResult> AddItem([FromBody] ToDoDto itemFromUser)
        {
            var userId = User.FindFirstValue("AppicationUserId");
            var user = await GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            if (itemFromUser == null)
            {
                return BadRequest(new { success = false, message = "Item couldn't be null" });
            }

            if (ModelState.IsValid)
            {
                var todoItem = new TodoItems
                {
                    Title = itemFromUser.title,
                    Description = itemFromUser.Description,
                    CreationDate = itemFromUser.CreationDate,
                    isDone = false,
                    ApplicationUserId = userId
                };

                _context.TodoItems.Add(todoItem);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, ItemId = todoItem.Id });
            }

            return BadRequest(new { success = false, message = "Item couldn't be added." });
        }
        #endregion

        // check validated id 
        #region delete item
        [Authorize(Roles = "Student")]
        [HttpDelete("Delete/{todoId}")]
        public async Task<IActionResult> DeleteItem(int todoId)
        {
            var userId = User.FindFirstValue("AppicationUserId");
            var user = await GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            if (ModelState.IsValid)
            {
                var todoItem = await _context.TodoItems
                    .FirstOrDefaultAsync(td => td.Id == todoId && td.ApplicationUserId == userId);

                if (todoItem == null)
                {
                    return NotFound(new { success = false, message = "Item was not found" });
                }

                _context.TodoItems.Remove(todoItem);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Item deleted successfully." });
            }

            return BadRequest(new { success = false, message = "Item was not deleted." });
        }
        #endregion

        #region stored
        [Authorize(Roles = "Student")]
        [HttpGet("sort-by-date")]
        public async Task<IActionResult> SortedItems()
        {
            var userId = User.FindFirstValue("AppicationUserId");
            var user = await GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            if (ModelState.IsValid)
            {
                var sortedItems = await _context.TodoItems
                    .Where(td => td.ApplicationUserId == userId)
                    .OrderByDescending(td => td.CreationDate)
                    .ToListAsync();

                if (sortedItems == null || sortedItems.Count == 0)
                {
                    return BadRequest(new { success = false, message = "No items found." });
                }

                return Ok(sortedItems);
            }

            return BadRequest(new { success = false, message = "No items found." });
        }
        #endregion

        #region update 

        [Authorize(Roles = "Student")]
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] ToDoConfirm toDoConfirm)
        {
            var userId = User.FindFirstValue("AppicationUserId");
            var user = await GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            if (toDoConfirm == null)
            {
                return BadRequest(new { success = false, message = "Item cannot be null." });
            }

            var todoItem = await _context.TodoItems
                .FirstOrDefaultAsync(td => td.Id == id && td.ApplicationUserId == userId);

            if (todoItem == null)
            {
                return NotFound(new { success = false, message = "Item was not found" });
            }


            todoItem.isDone = toDoConfirm.IsDone;

            _context.TodoItems.Update(todoItem);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Item updated successfully." });
        } 
        #endregion
    }
}

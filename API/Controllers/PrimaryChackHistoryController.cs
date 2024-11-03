using API.Dtos;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize(Roles = "Admin, User")]
[ApiController]
[Route("api/[controller]")]
public class PrimeChackHistoryController(UserManager<AppUser> userManager, AppDbContext context, IServiceProvider serviceProvider) : ControllerBase
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly AppDbContext _context = context;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public static int TasksInProgressCount { get; private set; } = 0;
    private const int MaxActiveTasks = 5;
    private const int MaxNumber = 1500;


    // POST: api/primecheckhistory/create
    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> CreatePrimeCheckRequest([FromBody] PrimeCheckRequestDto requestDto)
    {
        // Find user and validate existence
        var user = await _userManager.FindByIdAsync(requestDto.UserId!);
        if (user == null)
            return NotFound("User not found.");

        // Validate number range
        if (requestDto.Number > MaxNumber)
        {
            return BadRequest(new { 
                message = $"The number must not exceed {MaxNumber}.",
                providedNumber = requestDto.Number,
                maxAllowedNumber = MaxNumber
            });
        }

        // Check active tasks count
        var activeTasksCount = await _context.PrimeCheckHistory
            .CountAsync(x => x.UserId == requestDto.UserId 
                && x.Progress != 100  
                && x.Progress != -1);

        if (activeTasksCount >= MaxActiveTasks)
        {
            return BadRequest(new { 
                message = $"You have reached the maximum limit of {MaxActiveTasks} active tasks.",
                currentActiveTasks = activeTasksCount,
                maxAllowedTasks = MaxActiveTasks
            });
        }

        // Create new PrimeCheckHistory record
        var primeCheckHistory = new PrimeCheckHistory
        {
            UserId = requestDto.UserId,
            Number = requestDto.Number,
            IsPrime = false,
            RequestDateTime = DateTime.UtcNow,
            Progress = 0
        };
        _context.PrimeCheckHistory.Add(primeCheckHistory);
        await _context.SaveChangesAsync();

        // Create cancellation token
        var cancellationToken = new Models.CancellationToken
        {
            PrimeCheckHistoryId = primeCheckHistory.Id,
            IsCanceled = false
        };

        await _context.CancellationToken.AddAsync(cancellationToken);
        await _context.SaveChangesAsync();

        // Start the task processing
        await StartPrimeCheckRequest(primeCheckHistory.Id);

        return Ok(new { 
            message = "Task started successfully", 
            taskId = primeCheckHistory.Id,
            activeTasksCount = activeTasksCount + 1,
            remainingTaskSlots = MaxActiveTasks - (activeTasksCount + 1),
            number = requestDto.Number
        });
    }

    // POST: api/primecheckhistory/start/{id}
    [Authorize]
    [HttpPost("start/{id}")]
    public async Task<IActionResult> StartPrimeCheckRequest(int id)
    {
        var task = await _context.PrimeCheckHistory.FindAsync(id);
        if (task == null)
            return NotFound("Task not found.");

        if (task.Progress == 100)
            return BadRequest("Task is already completed.");

        // Increase the static task count
        TasksInProgressCount++;

        // Execute task in a separate thread
        _ = System.Threading.Tasks.Task.Run(async () =>
        {
            AppDbContext scopedContext;
            try
            {
                using var scope = _serviceProvider.CreateScope();
                scopedContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                await PrimeCheckService.IsPrime(id, scopedContext);
                TasksInProgressCount--;
            }
            catch (OperationCanceledException)
            {
                using var scope = _serviceProvider.CreateScope();
                scopedContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                var cancellationToken = await scopedContext.CancellationToken
                    .FirstOrDefaultAsync(x => x.PrimeCheckHistoryId == id);
                if (cancellationToken != null)
                {
                    cancellationToken.IsCanceled = true;
                    scopedContext.CancellationToken.Update(cancellationToken);
                    await scopedContext.SaveChangesAsync();
                }
                TasksInProgressCount--;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing prime check task: {ex.Message}");
                
                using var scope = _serviceProvider.CreateScope();
                scopedContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                var taskToUpdate = await scopedContext.PrimeCheckHistory.FindAsync(id);
                if (taskToUpdate != null)
                {
                    taskToUpdate.Progress = -2; 
                    await scopedContext.SaveChangesAsync();
                }
                
                TasksInProgressCount--;
            }
        });

        return Ok(new { message = "Task processing started", taskId = id });
    }




    [Authorize]
    [HttpPost("cancel-request/{id}")]
    public async Task<IActionResult> CancelRequest(int id)
    {
        try
        {
            var cancellationToken = await _context.CancellationToken
                .FirstOrDefaultAsync(r => r.PrimeCheckHistoryId == id);

            if (cancellationToken == null)
                return NotFound("Cancellation token not found for this request.");

            var task = await _context.PrimeCheckHistory
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound("Prime check task not found.");

            if (task.Progress == 100)
                return BadRequest("Cannot cancel completed task.");

            if (task.Progress == -1)
                return BadRequest("Task is already cancelled.");

            cancellationToken.IsCanceled = true;
            await _context.SaveChangesAsync();

            task.Progress = -1;
            _context.PrimeCheckHistory.Update(task);
            await _context.SaveChangesAsync();

            TasksInProgressCount--;

            return Ok(new { 
                message = "Request cancelled successfully",
                taskId = id,
                status = "Cancelled"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                message = "An error occurred while cancelling the request",
                error = ex.Message
            });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all-requests")]
    public async Task<IActionResult> GetAllRequests()
    {
        var requests = await _context.PrimeCheckHistory.ToListAsync();
        return Ok(requests);
    }


    [Authorize]
    [HttpGet("requests-by-user-id{id}")]
    public async Task<IActionResult> GetRequestsByUserId(string id)
    {
        var requests = await _context.PrimeCheckHistory
            .Where(r => r.UserId == id)
            .ToListAsync();

        if (requests == null || !requests.Any())
        {
            return NotFound();
        }

        return Ok(requests);
    }

    [Authorize]
    [HttpGet("get-prime-chack-request{id}")]
    public async Task<IActionResult> GetRequest(int id)
    {
        var request = await _context.PrimeCheckHistory
            .FirstOrDefaultAsync(r => r.Id == id);
        if (request == null)
        {
            return NotFound("Request not found.");
        }

        var currentUserId = _userManager.GetUserId(User);
        if (request.UserId != currentUserId)
        {
            return Forbid("You are not authorized to access this request.");
        }

        return Ok(request);
    }

    // GET: api/primechackhistory/running-tasks-count
    [Authorize]
    [HttpGet("running-tasks-count")]
    public IActionResult GetRunningTasksCount()
    {
        return Ok(new { count = TasksInProgressCount });
    }

}


using API.Dtos;
using Core.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Core.Data;
//using PrimeCheckService.Services;
using Microsoft.EntityFrameworkCore;
using Core.Repositories;

namespace API.Controllers;

[Authorize(Roles = "Admin, User")]
[ApiController]
[Route("api/[controller]")]
public class PrimeChackHistoryController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    //private readonly AppDbContext _context;
    private static IServiceProvider _serviceProvider;

    private readonly PrimeCheckHistoryRepository _primeCheckHistoryRepository;
    private readonly CancelationTokenRepository _cancelationTokenRepository;

    public volatile static int TasksInProgressCount = 0;
    public volatile static bool ProcessLoopInUse = false;
    private const int MaxActiveTasksForUser = 5;
    private const int MaxActiveTasksForServer = 2;
    private const int MaxNumber = 1500;

    public PrimeChackHistoryController(UserManager<AppUser> userManager,
                                       PrimeCheckHistoryRepository primeCheckHistoryRepository,
                                       CancelationTokenRepository cancelationTokenRepository,
                                       IServiceProvider serviceProvider)
    {
        if (_serviceProvider == null)
            _serviceProvider = serviceProvider;

        //_serviceProvider = serviceProvider;
        _userManager = userManager;
        //_context = context;
        _primeCheckHistoryRepository = primeCheckHistoryRepository;
        _cancelationTokenRepository = cancelationTokenRepository;
    }


    // POST: api/primecheckhistory/create
    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> CreatePrimeCheckRequest([FromBody] PrimeCheckRequestDto requestDto)
    {
        var user = await _userManager.FindByIdAsync(requestDto.UserId!);
        if (user == null)
            return NotFound("User not found.");

        if (requestDto.Number > MaxNumber)
        {
           return BadRequest(new
           {
               message = $"The number must not exceed {MaxNumber}.",
               providedNumber = requestDto.Number,
               maxAllowedNumber = MaxNumber
           });
        }

        var activeTasksCount = await _primeCheckHistoryRepository.GetActiveTaskCountAsync(requestDto.UserId!);

        if (activeTasksCount >= MaxActiveTasksForUser)
        {
            return BadRequest(new
            {
                message = $"You have reached the maximum limit of {MaxActiveTasksForUser} active tasks.",
                currentActiveTasks = activeTasksCount,
                maxAllowedTasks = MaxActiveTasksForUser
            });
        }

        var primeCheckHistory = new PrimeCheckHistory
        {
            UserId = requestDto.UserId,
            Number = requestDto.Number,
            IsPrime = false,
            RequestDateTime = DateTime.UtcNow,
            Progress = -3//TasksInProgressCount < MaxActiveTasksForServer ? 0 : -3
        };
        _primeCheckHistoryRepository.Add(primeCheckHistory);

        var cancellationToken = new Core.Models.CancellationToken
        {
            PrimeCheckHistoryId = primeCheckHistory.Id,
            IsCanceled = false
        };

        _cancelationTokenRepository.Add(cancellationToken);
        
        
        return Ok(new
        {
            message = TasksInProgressCount < MaxActiveTasksForServer ? "Task started successfully" : "Task added to queue",
            taskId = primeCheckHistory.Id,
            activeTasksCount = activeTasksCount + 1,
            remainingTaskSlots = MaxActiveTasksForServer - (activeTasksCount + 1),
            number = requestDto.Number
        });
    }

    //POST: api/primecheckhistory/start/{id}
    [Authorize]
    [HttpPost("start/{id}")]
    public async Task<IActionResult> StartPrimeCheckRequest(int id)
    {
        var task = _primeCheckHistoryRepository.GetById(id);
        //await _context.PrimeCheckHistory.FindAsync(id);
        if (task == null)
            return NotFound("Task not found.");

        if (task.Progress == 100)
            return BadRequest("Task is already completed.");

        TasksInProgressCount++;
        var dbContext = _serviceProvider.GetService<AppDbContext>();
        if (dbContext == null)
            return StatusCode(500, "Database context is not available.");
        
        PrimeCheckService.IsPrime(id, dbContext).GetAwaiter().GetResult();

        return Ok(new { message = "Task processing started", taskId = id });
    }


    [Authorize]
    [HttpGet("queue")]
    public async Task<IActionResult> Queue()
    {
        //var tasks = await _context.PrimeCheckHistory
        //    .Where(x => x.Progress == -3)
        //    .ToListAsync();
        return Ok(_primeCheckHistoryRepository.GetAll(-3));
    }

    [Authorize]
    [HttpGet("queue-size")]
    public int QueueSize()
    {
        //int tasks = _context.PrimeCheckHistory
        //    .Count(x => x.Progress == -3);
        return _primeCheckHistoryRepository.GetAll(-3).Count;
    }

    [Authorize]
    [HttpPost("cancel-request/{id}")]
    public async Task<IActionResult> CancelRequest(int id)
    {
        try
        {
            var cancellationToken = _cancelationTokenRepository.GetByPrimeCheckHistoryId(id);

            if (cancellationToken == null)
                return NotFound("Cancellation token not found for this request.");

            var task = _primeCheckHistoryRepository.GetById(id);

            if (task == null)
                return NotFound("Prime check task not found.");

            if (task.Progress == 100)
                return BadRequest("Cannot cancel completed task.");

            if (task.Progress == -1)
                return BadRequest("Task is already cancelled.");

            cancellationToken.IsCanceled = true;

            task.Progress = -1;

            TasksInProgressCount--;

            _primeCheckHistoryRepository.Update(task);
            _cancelationTokenRepository.Update(cancellationToken);

            return Ok(new
            {
                message = "Request cancelled successfully",
                taskId = id,
                status = "Cancelled"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while cancelling the request",
                error = ex.Message
            });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all-requests")]
    public async Task<IActionResult> GetAllRequests()
    {
        var requests = _primeCheckHistoryRepository.GetAll();//await _context.PrimeCheckHistory.ToListAsync();
        return Ok(requests);
    }


    [Authorize]
    [HttpGet("requests-by-user-id{id}")]
    public async Task<IActionResult> GetRequestsByUserId(string id)
    {
        var requests = _primeCheckHistoryRepository.GetByUserId(id);

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
        var request = _primeCheckHistoryRepository.GetById(id);

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


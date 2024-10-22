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

    [HttpPost("create")]
    public async Task<IActionResult> CreatePrimeCheckRequest([FromBody] PrimeCheckRequestDto requestDto)
    {
        var user = await _userManager.FindByIdAsync(requestDto.UserId!);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var primeCheckHistory = new PrimeCheckHistory
        {
            UserId = requestDto.UserId,
            Number = requestDto.Number,
            IsPrime = false,
            RequestDateTime = DateTime.UtcNow
        };
        _context.PrimeCheckHistory.Add(primeCheckHistory);
        await _context.SaveChangesAsync();

         var cancellationToken = new Models.CancellationToken
        {
            PrimeCheckHistoryId = primeCheckHistory.Id,
            IsCanceled = false
        };

        await _context.CancellationToken.AddAsync(cancellationToken);
        await _context.SaveChangesAsync();

        _ = System.Threading.Tasks.Task.Run(async () =>
        {
            AppDbContext scopedContext;
            try
            {
                using var scope = _serviceProvider.CreateScope();
                scopedContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await PrimeCheckService.IsPrime(primeCheckHistory.Id, scopedContext);
                
            }
            catch (OperationCanceledException)
            {
                using var scope = _serviceProvider.CreateScope();
                scopedContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                cancellationToken.IsCanceled = true;
                scopedContext.CancellationToken.Update(cancellationToken);
                await scopedContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //cancelled

                Console.WriteLine($"\n\n\n\n\n\n\n\n\n\nError processing prime check task: {ex.Message}\n\n\n\n\n\n\n\n\n\n");
            }
        });

        return Ok(new { message = "Task started successfully", taskId = primeCheckHistory.Id });
    }


    [HttpGet("all-requests")]
    public async Task<IActionResult> GetAllRequests()
    {
        var requests = await _context.PrimeCheckHistory.ToListAsync();
        return Ok(requests);
    }

    [HttpGet("get-prime-chack-request{id}")]
    public async Task<IActionResult> GetRequest(int id)
    {
        var request = await _context.PrimeCheckHistory
            .FirstOrDefaultAsync(r => r.Id == id);
        
        if (request == null)
        {
            return NotFound("Request not found.");
        }

        return Ok(request);
    }

    [HttpGet("get-prime-chack-request-progress{id}")]
    public async Task<IActionResult> GetRequestProgress(int id)
    {
        var request = await _context.PrimeCheckHistory
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
        {
            return NotFound("Request not found.");
        }

        var progress = new
        {
            RequestId = request.Id,
            Status = request.IsPrime ? "Completed" : "In Progress",
            RequestDate = request.RequestDateTime
        };

        return Ok(progress);
    }

}


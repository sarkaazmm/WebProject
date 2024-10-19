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
public class PrimeChackHistoryController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly PrimeCheckService _primeCheckService;
    private readonly AppDbContext _context;

    public PrimeChackHistoryController(UserManager<AppUser> userManager, PrimeCheckService primeCheckService, AppDbContext context)
    {
        _context = context;
        _userManager = userManager;
        _primeCheckService = primeCheckService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreatePrimeCheckRequest([FromBody] PrimeCheckRequestDto requestDto)
    {
        var user = await _userManager.FindByIdAsync(requestDto.UserId!);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        bool isPrime = _primeCheckService.IsPrime(requestDto.Number, requestDto.UserId!);
        var primeCheckHistory = new PrimeCheckHistory
        {
            UserId = requestDto.UserId,
            Number = requestDto.Number,
            IsPrime = isPrime,
            RequestDateTime = DateTime.UtcNow
        };
        _context.PrimeCheckHistory.Add(primeCheckHistory);
        await _context.SaveChangesAsync();
        
        return Ok(primeCheckHistory);
    }

    [HttpGet("all-requests")]
    public async Task<IActionResult> GetAllRequests()
    {
        var requests = await _context.PrimeCheckHistory.ToListAsync();
        return Ok(requests);
    }

}


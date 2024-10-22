using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class CancellationToken
{
    [Key]
    public int Id { get; set; }
    public int PrimeCheckHistoryId { get; set; }
    public bool IsCanceled { get; set; }
}

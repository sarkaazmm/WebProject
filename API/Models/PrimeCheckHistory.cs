using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

[Table("PrimeCheckHistory")]
public class PrimeCheckHistory
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public int Number { get; set; }
    public bool IsPrime { get; set; }
    public DateTime RequestDateTime { get; set; }
    public int Progress { get; set; }

}

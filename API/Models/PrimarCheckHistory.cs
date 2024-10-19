using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

[Table("PrimarCheckHistory")]
public class PrimarCheckHistory
{
    public int Id { get; set; }
    private double Number { get; set; }
    private bool IsPrimary { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.Concurrency.Tests;

public class TestModel
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TestModelId { get; set; } = 1;

    public string TestModelString { get; set; } = string.Empty;

    [ConcurrencyCheck]
    public Guid Version { get; set; }
}
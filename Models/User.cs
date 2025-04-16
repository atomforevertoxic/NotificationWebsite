using System.ComponentModel.DataAnnotations;

public class User
{

    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string? Name { get; set; }

    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [Range(18, 100)]
    public int Age { get; set; }


    public DateTime Timestamp { get; set; }
}
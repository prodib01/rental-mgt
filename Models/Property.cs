using System.ComponentModel.DataAnnotations;

public class Property
{ 
    public int Id { get; set; }

    [Required]
    public string Address { get; set; }

    [Required]
    public string Type { get; set; }

    [Required]
    [DataType(DataType.Currency)]
    public decimal Rent { get; set; }

    public string Description { get; set; }

}
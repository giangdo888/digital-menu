namespace DigitalMenuApi.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public class UpdateDishIngredientsRequest
{
    [Required]
    public required List<DishIngredientItem> Ingredients { get; set; }
}

public class DishIngredientItem
{
    [Required]
    public required int AfcdItemId { get; set; }

    [Required]
    [Range(0.1, 10000, ErrorMessage = "Amount must be between 0.1 and 10000 grams")]
    public required decimal AmountInGrams { get; set; }
}

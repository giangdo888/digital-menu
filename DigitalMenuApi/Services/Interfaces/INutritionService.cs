namespace DigitalMenuApi.Services.Interfaces;

public interface INutritionService
{
    /// <summary>
    /// Calculate BMI from height and weight
    /// </summary>
    decimal CalculateBmi(decimal heightCm, decimal weightKg);

    // <summary>
    /// Calculate weight from BMI and height
    /// </summary>
    decimal CalculateWeightFromBmi(decimal bmi, decimal heightCm);

    /// <summary>
    /// Get BMI category based on BMI value
    /// </summary>
    string GetBmiCategory(decimal bmi);

    /// <summary>
    /// Calculate Basal Metabolic Rate using Mifflin-St Jeor equation
    /// </summary>
    decimal CalculateBmr(string gender, decimal weightKg, decimal heightCm, int age);

    /// <summary>
    /// Calculate Total Daily Energy Expenditure (TDEE)
    /// Assumes sedentary lifestyle (BMR * 1.2) as base
    /// </summary>
    decimal CalculateTdee(decimal bmr);

    /// <summary>
    /// Calculate daily calorie target based on TDEE and dietary goal
    /// </summary>
    decimal CalculateDailyCaloriesTarget(decimal tdee, string dietaryGoal);

    /// <summary>
    /// Calculate macronutrient targets based on calories and goal
    /// </summary>
    (decimal proteinG, decimal carbsG, decimal fatG) CalculateMacros(decimal dailyCalories, string dietaryGoal);

    /// <summary>
    /// Calculate age from date of birth
    /// </summary>
    int CalculateAge(DateOnly dateOfBirth);
}

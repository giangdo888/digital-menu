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
    /// Calculate Total Daily Energy Expenditure (TDEE) based on BMR and Activity Level
    /// </summary>
    decimal CalculateTdee(decimal bmr, string activityLevel);

    /// <summary>
    /// Calculate daily calorie target based on TDEE, gender, and weekly weight goal
    /// </summary>
    decimal CalculateDailyCaloriesTarget(decimal tdee, string gender, decimal targetWeightChangeKgPerWeek);

    /// <summary>
    /// Calculate macronutrient targets based on calories, weight, and targeted weight change
    /// </summary>
    (decimal proteinG, decimal carbsG, decimal fatG) CalculateMacros(decimal dailyCalories, decimal weightKg, decimal targetWeightChangeKgPerWeek);

    /// <summary>
    /// Calculate age from date of birth
    /// </summary>
    int CalculateAge(DateOnly dateOfBirth);
}

namespace DigitalMenuApi.Services.Implementations;

using DigitalMenuApi.Services.Interfaces;

public class NutritionService : INutritionService
{
    /// <summary>
    /// BMI = weight(kg) / height(m)^2
    /// </summary>
    public decimal CalculateBmi(decimal heightCm, decimal weightKg)
    {
        if (heightCm <= 0) return 0;
        var heightM = heightCm / 100;
        return Math.Round(weightKg / (heightM * heightM), 1);
    }

    /// <summary>
    /// Calculate weight from BMI and height
    /// </summary>
    public decimal CalculateWeightFromBmi(decimal bmi, decimal heightCm)
    {
        if (heightCm <= 0) return 0m;
        var heightM = heightCm / 100;
        return Math.Round(bmi * (heightM * heightM), 1);
    }

    /// <summary>
    /// WHO BMI Categories
    /// </summary>
    public string GetBmiCategory(decimal bmi)
    {
        return bmi switch
        {
            < 18.5m => "Underweight",
            < 25m => "Normal",
            < 30m => "Overweight",
            _ => "Obese"
        };
    }

    /// <summary>
    /// Mifflin-St Jeor Equation (most accurate for most people)
    /// Men:   BMR = (10 × weight in kg) + (6.25 × height in cm) – (5 × age) + 5
    /// Women: BMR = (10 × weight in kg) + (6.25 × height in cm) – (5 × age) – 161
    /// </summary>
    public decimal CalculateBmr(string gender, decimal weightKg, decimal heightCm, int age)
    {
        var bmr = (10 * weightKg) + (6.25m * heightCm) - (5 * age);

        if (gender.ToLower() == "male")
            bmr += 5;
        else
            bmr -= 161;

        return Math.Round(bmr, 0);
    }

    /// <summary>
    /// TDEE = BMR × Activity Factor
    /// Using 1.2 (sedentary) as default - can be enhanced later with activity level
    /// </summary>
    public decimal CalculateTdee(decimal bmr)
    {
        return Math.Round(bmr * 1.2m, 0);
    }

    /// <summary>
    /// Adjust calories based on goal:
    /// - Lose: TDEE - 500 (0.5kg/week loss)
    /// - Maintain: TDEE
    /// - Gain: TDEE + 300 (lean gain)
    /// </summary>
    public decimal CalculateDailyCaloriesTarget(decimal tdee, string dietaryGoal)
    {
        return dietaryGoal.ToLower() switch
        {
            "lose" => Math.Max(1200, tdee - 500),  // Minimum 1200 calories for safety
            "gain" => tdee + 300,
            _ => tdee  // maintain
        };
    }

    /// <summary>
    /// Calculate macros based on goal:
    /// - Protein: 1.6-2.2g per kg body weight (using calories as proxy)
    /// - Fat: 25-30% of calories
    /// - Carbs: remainder
    ///
    /// Simplified approach using calorie percentages:
    /// - Lose:     40% protein, 30% carbs, 30% fat
    /// - Maintain: 30% protein, 40% carbs, 30% fat
    /// - Gain:     30% protein, 45% carbs, 25% fat
    /// </summary>
    public (decimal proteinG, decimal carbsG, decimal fatG) CalculateMacros(decimal dailyCalories, string dietaryGoal)
    {
        decimal proteinPct, carbsPct, fatPct;

        switch (dietaryGoal.ToLower())
        {
            case "lose":
                proteinPct = 0.40m;
                carbsPct = 0.30m;
                fatPct = 0.30m;
                break;
            case "gain":
                proteinPct = 0.30m;
                carbsPct = 0.45m;
                fatPct = 0.25m;
                break;
            default: // maintain
                proteinPct = 0.30m;
                carbsPct = 0.40m;
                fatPct = 0.30m;
                break;
        }

        // Protein: 4 cal/g, Carbs: 4 cal/g, Fat: 9 cal/g
        var proteinG = Math.Round((dailyCalories * proteinPct) / 4, 0);
        var carbsG = Math.Round((dailyCalories * carbsPct) / 4, 0);
        var fatG = Math.Round((dailyCalories * fatPct) / 9, 0);

        return (proteinG, carbsG, fatG);
    }

    /// <summary>
    /// Calculate age from date of birth
    /// </summary>
    public int CalculateAge(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dateOfBirth.Year;

        if (dateOfBirth > today.AddYears(-age))
            age--;

        return age;
    }
}

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
    /// </summary>
    public decimal CalculateTdee(decimal bmr, string activityLevel)
    {
        var activityMultiplier = activityLevel.ToLower() switch
        {
            "lightly_active" => 1.375m,
            "moderately_active" => 1.55m,
            "very_active" => 1.725m,
            "extra_active" => 1.9m,
            _ => 1.2m // sedentary default
        };

        return Math.Round(bmr * activityMultiplier, 0);
    }

    /// <summary>
    /// Adjust calories based on goal:
    /// - Lose: TDEE - 500 (approx. 0.5kg/week loss)
    /// - Maintain: TDEE
    /// - Gain: TDEE + 300 (lean gain)
    /// References: 
    /// - Hall, K. D. (2008). What is the required energy deficit per unit weight loss? International Journal of Obesity.
    /// - Redman, L. M., et al. (2009). Metabolic and Behavioral Compensations in Response to Caloric Restriction.
    /// </summary>
    public decimal CalculateDailyCaloriesTarget(decimal tdee, string dietaryGoal)
    {
        return dietaryGoal.ToLower() switch
        {
            "lose" => Math.Max(1200, tdee - 500),  // Minimum 1200 calories for safety (ACSM guidelines)
            "gain" => tdee + 300,
            _ => tdee  // maintain
        };
    }

    /// <summary>
    /// Calculate macros based on goal using calorie percentages.
    /// References:
    /// - Protein (1.6-2.2g/kg proxy): Morton, R. W., et al. (2018). A systematic review... protein supplementation on resistance training-induced gains. British Journal of Sports Medicine.
    /// - AMDR (Acceptable Macronutrient Distribution Ranges): Institute of Medicine (2005). Dietary Reference Intakes for Energy, Carbohydrate, Fiber, Fat, Fatty Acids, Cholesterol, Protein, and Amino Acids.
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

        // Energy density: Protein 4 kcal/g, Carbs 4 kcal/g, Fat 9 kcal/g (Atwater system)
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

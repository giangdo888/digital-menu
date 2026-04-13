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
    /// Calculate daily calorie target based on TDEE, gender, and weekly weight goal.
    /// Formula: TDEE + ((Goal_{kg/week} * 7700) / 7)
    /// Safety floors: 1500 for men, 1200 for women.
    /// </summary>
    public decimal CalculateDailyCaloriesTarget(decimal tdee, string gender, decimal targetWeightChangeKgPerWeek)
    {
        var calorieAdjustment = (targetWeightChangeKgPerWeek * 7700m) / 7m;
        var targetCalories = tdee + calorieAdjustment;

        var safetyFloor = gender.ToLower() == "male" ? 1500m : 1200m;
        return Math.Max(safetyFloor, Math.Round(targetCalories, 0));
    }

    /// <summary>
    /// Calculate macros based on goal using calorie percentages. Checks protein threshold.
    /// References:
    /// - Protein (1.6-2.2g/kg proxy): Morton, R. W., et al. (2018). A systematic review... protein supplementation on resistance training-induced gains. British Journal of Sports Medicine.
    /// - AMDR (Acceptable Macronutrient Distribution Ranges): Institute of Medicine (2005). Dietary Reference Intakes for Energy, Carbohydrate, Fiber, Fat, Fatty Acids, Cholesterol, Protein, and Amino Acids.
    /// </summary>
    public (decimal proteinG, decimal carbsG, decimal fatG) CalculateMacros(decimal dailyCalories, decimal weightKg, decimal targetWeightChangeKgPerWeek)
    {
        decimal proteinPct, carbsPct, fatPct;

        if (targetWeightChangeKgPerWeek < 0) // lose
        {
            proteinPct = 0.30m;
            carbsPct = 0.40m;
            fatPct = 0.30m;
        }
        else if (targetWeightChangeKgPerWeek > 0) // gain
        {
            proteinPct = 0.25m;
            carbsPct = 0.55m;
            fatPct = 0.20m;
        }
        else // maintain
        {
            proteinPct = 0.25m;
            carbsPct = 0.50m;
            fatPct = 0.25m;
        }

        // Energy density: Protein 4 kcal/g, Carbs 4 kcal/g, Fat 9 kcal/g (Atwater system)
        var proteinG = Math.Round((dailyCalories * proteinPct) / 4, 0);
        var carbsG = Math.Round((dailyCalories * carbsPct) / 4, 0);
        var fatG = Math.Round((dailyCalories * fatPct) / 9, 0);

        // Apply protein threshold (1.6 g/kg minimum for preservation)
        var minProteinG = Math.Round(1.6m * weightKg, 0);
        if (proteinG < minProteinG)
        {
            proteinG = minProteinG;
            // Recalculate remaining calories for carbs and fat
            var remainingCalories = dailyCalories - (proteinG * 4);
            
            // Distribute remaining calories to carbs and fat based on their original ratio
            var carbsToFatRatio = carbsPct / (carbsPct + fatPct);
            carbsG = Math.Round((remainingCalories * carbsToFatRatio) / 4, 0);
            fatG = Math.Round((remainingCalories * (1 - carbsToFatRatio)) / 9, 0);
        }

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

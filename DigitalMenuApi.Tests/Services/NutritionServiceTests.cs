using DigitalMenuApi.Services.Implementations;
using FluentAssertions;
using System;
using Xunit;

namespace DigitalMenuApi.Tests.Services;

public class NutritionServiceTests
{
    private readonly NutritionService _sut; // System Under Test

    public NutritionServiceTests()
    {
        _sut = new NutritionService();
    }

    // This test ensures our Body Mass Index (BMI) calculator works perfectly.
    // We pass in different heights and weights, and make sure the math matches
    // the standard formula (weight / height squared) rounded to one decimal point.
    // It also checks edge cases like what happens if height is accidentally 0.
    [Theory]
    [InlineData(180, 80, 24.7)]
    [InlineData(160, 50, 19.5)]
    [InlineData(0, 80, 0)]  // Edge case: 0 height
    [InlineData(-10, 80, 0)] // Edge case: negative height
    public void CalculateBmi_ShouldReturnCorrectMathRoundedToOneDecimal(decimal heightCm, decimal weightKg, decimal expectedBmi)
    {
        // Act
        var result = _sut.CalculateBmi(heightCm, weightKg);

        // Assert
        result.Should().Be(expectedBmi);
    }

    // Sometimes we need to work backwards! If we know someone's height and their target BMI,
    // this test checks that our math correctly figures out exactly how much they need to weigh
    // to hit that BMI target.
    [Theory]
    [InlineData(24.7, 180, 80.0)]
    [InlineData(19.5, 160, 49.9)]
    [InlineData(24.7, 0, 0)]
    public void CalculateWeightFromBmi_ShouldReturnCorrectWeight(decimal bmi, decimal heightCm, decimal expectedWeight)
    {
        // Act
        var result = _sut.CalculateWeightFromBmi(bmi, heightCm);

        // Assert
        result.Should().Be(expectedWeight);
    }

    // After calculating a BMI, we need to tell the user what category they fall into.
    // This test feeds in borderline numbers (like 24.9 vs 25.0) to make sure they get
    // classified correctly according to standard World Health Organization categories.
    [Theory]
    [InlineData(18.4, "Underweight")]
    [InlineData(18.5, "Normal")]
    [InlineData(24.9, "Normal")]
    [InlineData(25.0, "Overweight")]
    [InlineData(29.9, "Overweight")]
    [InlineData(30.0, "Obese")]
    public void GetBmiCategory_ShouldReturnCorrectClassification(decimal bmi, string expectedCategory)
    {
        // Act
        var result = _sut.GetBmiCategory(bmi);

        // Assert
        result.Should().Be(expectedCategory);
    }

    // BMR (Basal Metabolic Rate) is how many calories you burn just staying alive in bed all day.
    // This test ensures we're correctly using the Mifflin-St Jeor equation. We test both men and
    // women because the formula changes slightly based on gender, and we make sure it doesn't care
    // if "male" is typed in uppercase or lowercase.
    [Theory]
    [InlineData("male", 80, 180, 30, 1780)]
    [InlineData("Male", 80, 180, 30, 1780)] // Case insensitivity
    [InlineData("female", 65, 160, 25, 1364)]
    [InlineData("Female", 65, 160, 25, 1364)]
    public void CalculateBmr_ShouldUseCorrectMifflinStJeorEquation(string gender, decimal weightKg, decimal heightCm, int age, decimal expectedBmr)
    {
        // Act
        var result = _sut.CalculateBmr(gender, weightKg, heightCm, age);

        // Assert
        result.Should().Be(expectedBmr);
    }

    // TDEE (Total Daily Energy Expenditure) is your BMR multiplied by your activity level.
    // Right now, we assume everyone is "sedentary" (multiplier of 1.2). This test just makes
    // sure that math works out before we build on top of it.
    [Fact]
    public void CalculateTdee_ShouldMultiplyBySedentaryFactor()
    {
        // Arrange
        var bmr = 1780m;
        var expectedTdee = Math.Round(bmr * 1.2m, 0);

        // Act
        var result = _sut.CalculateTdee(bmr, "sedentary");

        // Assert
        result.Should().Be(expectedTdee);
    }

    // This is the big one! Depending on if the user wants to lose, maintain, or gain weight,
    // we take their daily calorie burn and adjust it.
    // Crucially, this test also ensures we NEVER recommend someone eat less than 1200 calories a day,
    // because that's fundamentally unsafe.
    [Theory]
    [InlineData(2000, "male", -0.5, 1500)]     // 2000 - 550 = 1450, hits floor 1500
    [InlineData(2000, "female", 0, 2000)]      // maintain = 2000
    [InlineData(2000, "male", 0.25, 2275)]     // 2000 + 275 = 2275
    [InlineData(1500, "female", -0.5, 1200)]   // Boundary check: Should catch floor 1200 for women
    [InlineData(1200, "male", -0.5, 1500)]     // Boundary check: Men floor 1500
    public void CalculateDailyCaloriesTarget_ShouldAdjustCorrectlyBasedOnGoal(decimal tdee, string gender, decimal weeklyWeightGoal, decimal expectedCalories)
    {
        // Act
        var result = _sut.CalculateDailyCaloriesTarget(tdee, gender, weeklyWeightGoal);

        // Assert
        result.Should().Be(expectedCalories);
    }

    [Theory]
    [InlineData(2000, 80, -0.5, 150, 200, 67)]     // Lose: 30% P = 150. Min P = 1.6*80 = 128. Since 150 > 128, use 150. Carbs 40% = 200, Fat 30% = 67.
    [InlineData(2000, 80, 0, 128, 248, 55)]        // Maintain: 25% P = 125. Min P = 128. Clamped to 128. Remaining cals = 1488. Carbs ratio = 2/3 -> 992/4 = 248. Fat ratio = 1/3 -> 496/9 = 55.
    [InlineData(2000, 80, 0.25, 128, 273, 44)]     // Gain: 25% P = 125. Min P = 128. Clamped to 128. Remaining cals = 1488. Carbs 55%, Fat 20%. Ratio C=55/75, F=20/75. C=1091.2/4=273, F=396.8/9=44.
    public void CalculateMacros_ShouldReturnCorrectGramDistributionForGoal(decimal dailyCalories, decimal weightKg, decimal weeklyWeightGoal, decimal expectedProtein, decimal expectedCarbs, decimal expectedFat)
    {
        // Act
        var result = _sut.CalculateMacros(dailyCalories, weightKg, weeklyWeightGoal);

        // Assert
        result.proteinG.Should().Be(expectedProtein);
        result.carbsG.Should().Be(expectedCarbs);
        result.fatG.Should().Be(expectedFat);
    }

    // The age calculator needs to be smart! If someone was born 30 years ago, but their
    // birthday hasn't happened yet *this* year, they are still technically 29.
    // This test simulates a birthday that happened yesterday, so they should be fully 30.
    [Fact]
    public void CalculateAge_ShouldReturnCorrectAge_WhenBirthdayHasPassed()
    {
        // Arrange
        // We use UTC now minus exactly 30 years to ensure the birthday has passed today
        var dob = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30).AddDays(-1));

        // Act
        var result = _sut.CalculateAge(dob);

        // Assert
        result.Should().Be(30);
    }

    // Following up on the previous test: this simulates someone whose birthday is *tomorrow*.
    // Even though it's the year they turn 30, the math should recognize they are technically
    // still 29 right now.
    [Fact]
    public void CalculateAge_ShouldReturnCorrectAge_WhenBirthdayHasNotPassed()
    {
        // Arrange
        // Add 1 day to the 30-year subtraction so the birthday hasn't happened yet this year
        var dob = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30).AddDays(1));

        // Act
        var result = _sut.CalculateAge(dob);

        // Assert
        result.Should().Be(29);
    }
}

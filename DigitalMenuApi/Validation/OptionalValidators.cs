namespace DigitalMenuApi.Validation;

using System.ComponentModel.DataAnnotations;

public class OptionalEmailAttribute : ValidationAttribute
{
    private readonly EmailAddressAttribute _inner = new();

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var str = value as string;
        if (string.IsNullOrWhiteSpace(str)) return ValidationResult.Success;
        return _inner.IsValid(str) ? ValidationResult.Success : new ValidationResult(ErrorMessage ?? "Invalid email address");
    }
}

public class OptionalPhoneAttribute : ValidationAttribute
{
    private readonly PhoneAttribute _inner = new();

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var str = value as string;
        if (string.IsNullOrWhiteSpace(str)) return ValidationResult.Success;
        return _inner.IsValid(str) ? ValidationResult.Success : new ValidationResult(ErrorMessage ?? "Invalid phone number");
    }
}

public class OptionalUrlAttribute : ValidationAttribute
{
    private readonly UrlAttribute _inner = new();

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var str = value as string;
        if (string.IsNullOrWhiteSpace(str)) return ValidationResult.Success;
        return _inner.IsValid(str) ? ValidationResult.Success : new ValidationResult(ErrorMessage ?? "Invalid URL");
    }
}

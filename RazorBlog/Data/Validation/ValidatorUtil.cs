using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RazorBlog.Data.Validation;

public static class ValidatorUtil
{
    public static bool TryValidateProperty(
        object? value, 
        string propertyName, 
        object sourceObject,
        ICollection<ValidationResult>? results = null)
    {
        return Validator.TryValidateProperty(
            value,
            new ValidationContext(sourceObject)
            {
                MemberName = propertyName,
            },
            results);
    }
}

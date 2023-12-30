using System;
using System.ComponentModel.DataAnnotations;

namespace RazorBlog.Data.Validation;

public class DateRangeAttribute(
    bool allowsPast, 
    bool allowsFuture) : ValidationAttribute
{
    private readonly bool _allowsPast = allowsPast;
    private readonly bool _allowsFuture = allowsFuture;

    public override bool IsValid(object? value)
    { 
        if (value is not DateTime dateTime) 
        {  
            return false; 
        }

        var utcDateTime = dateTime.ToUniversalTime();
        var now = DateTime.UtcNow;

        return (_allowsPast || utcDateTime >= now) && (_allowsFuture || utcDateTime <= now);
    }
}

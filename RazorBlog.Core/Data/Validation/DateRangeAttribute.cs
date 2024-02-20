using System;
using System.ComponentModel.DataAnnotations;

namespace RazorBlog.Core.Data.Validation;

public class DateRangeAttribute : ValidationAttribute
{
    private readonly bool _allowsPast;
    private readonly bool _allowsFuture;

    public DateRangeAttribute(bool allowsPast,
        bool allowsFuture)
    {
        _allowsPast = allowsPast;
        _allowsFuture = allowsFuture;
    }

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

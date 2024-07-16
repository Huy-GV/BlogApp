using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleForum.Core.Data.Validation;

public class DateRangeAttribute : ValidationAttribute
{
    private readonly bool _allowsPast;
    private readonly bool _allowsFuture;
    private readonly bool _required;

    public DateRangeAttribute(
        bool allowsPast,
        bool allowsFuture,
        bool required)
    {
        _allowsPast = allowsPast;
        _allowsFuture = allowsFuture;
        _required = required;
    }

    public override bool IsValid(object? value)
    {
        if (value is not DateTime dateTime)
        {
            return !_required;
        }

        var utcDateTime = dateTime.ToUniversalTime();
        var now = DateTime.UtcNow;

        return (_allowsPast || utcDateTime >= now) && (_allowsFuture || utcDateTime <= now);
    }
}

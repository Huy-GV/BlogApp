using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace SimpleForum.Core.Data.Validation;

public class FileTypeAttribute : ValidationAttribute
{
    private readonly string[] _allowedFileTypes;

    public override bool IsValid(object? value)
    {
        return value is null ||
               value is IFormFile file && _allowedFileTypes.Contains(Path.GetExtension(file.FileName).TrimStart('.'));
    }

    public FileTypeAttribute(params string[] allowedTypes)
    {
        if (allowedTypes.Any(x => string.IsNullOrWhiteSpace(x) || x == string.Empty))
        {
            throw new ArgumentException("File types must not be null or empty");
        }

        _allowedFileTypes = allowedTypes
            .Select(x => x.TrimStart('.', ' ').ToLowerInvariant())
            .ToArray();
    }
}

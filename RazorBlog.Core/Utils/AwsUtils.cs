using System.Diagnostics.CodeAnalysis;
using Amazon.Runtime.Internal.Util;

namespace RazorBlog.Core.Utils;

public static class AwsUtils
{
    public static bool TryConvertToS3Uri(string uri, [NotNullWhen(true)] out S3Uri? s3Uri)
    {
        s3Uri = null;
        try
        {
            s3Uri = new S3Uri(uri);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

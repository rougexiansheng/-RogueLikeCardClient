using System;

public static class Int32Extension
{
    /// <summary>
    /// 擴充string可以安全轉成Int
    /// </summary>
    public static int ToInt(this string text)
    {
        Int32.TryParse(text, out var result);
        return result;
    }
}

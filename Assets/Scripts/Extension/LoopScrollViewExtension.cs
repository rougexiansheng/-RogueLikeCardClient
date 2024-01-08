using UnityEngine;

public static class LoopScrollViewExtension
{
    public static int TransformIndexNumber(this int realIndex, int scrollRectCount)
    {
        int result = Mathf.Abs(realIndex);
        result %= scrollRectCount;
        if (realIndex < 0 && result != 0)
            result = scrollRectCount - result;
        return result;
    }
}

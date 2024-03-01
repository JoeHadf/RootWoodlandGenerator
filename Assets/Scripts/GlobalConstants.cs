using Unity.VisualScripting;

public static class GlobalConstants
{
    public const float clearingRadius = 1.0f;
    public const float clearingOutlineWidth = 0.2f;

    public const float pathWidth = 0.4f;
    public const float minPathLength = 1.5f;
    
    public static float xRange { get; private set; }
    public static float yRange { get; private set; }

    public static void SetScreenRange(float x, float y)
    {
        xRange = x - clearingRadius;
        yRange = y - clearingRadius;
    }
}

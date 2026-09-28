using System;

public static class HealthBarMetrics
{
    public static bool ShouldShow(float currentHealth, float maxHealth, bool isSelected)
    {
        return maxHealth > 0f && currentHealth > 0f && (isSelected || currentHealth < maxHealth);
    }

    public static float CalculateFill(float currentHealth, float maxHealth)
    {
        return maxHealth > 0f ? Math.Clamp(currentHealth / maxHealth, 0f, 1f) : 0f;
    }

    public static float CalculateWidth(float targetSize, float maxHealth, float widthPerWorldUnit, float healthWidthScale, float minWidth, float maxWidth)
    {
        float sizeWidth = Math.Max(0f, targetSize) * widthPerWorldUnit;
        float healthWidth = (float)Math.Sqrt(Math.Max(0f, maxHealth)) * healthWidthScale;
        return Math.Clamp(sizeWidth + healthWidth, minWidth, maxWidth);
    }
}

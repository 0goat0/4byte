using System;

public interface IHealthSource
{
    float CurrentHealth { get; }
    float MaxHealth { get; }

    event Action<float, float> OnHealthChanged;
}

using System;

public interface IHealthProvider {
    float Health { get; set; }
    float MaxHealth { get; set; }
    event Action<float> HealthUpdated;

    bool IsAlive => Health > 0f;
}

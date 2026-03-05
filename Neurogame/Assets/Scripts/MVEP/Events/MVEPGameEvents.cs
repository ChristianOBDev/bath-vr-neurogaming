using System;

public static class MVEPGameEvents
{
  public static Action OnPowerUpCollected;
  public static Action OnObstacleHit;

  public static Action<float> OnSpeedChanged;
  public static Action<int> OnScoreUpdated;
}

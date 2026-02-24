using System;
using UnityEngine;

public enum Phase
{
  NoControl,
  HalfControl,
  FullControl
}

public class MVEPGameSettings : Singleton<MVEPGameSettings>
{
  [Header("Configs")]
  public Phase CurrentPhase;
  public LaneConfiguration laneConfig;
  public ChunkConfiguration chunkConfig;
  public MVEPConfiguration mvepConfig;
}

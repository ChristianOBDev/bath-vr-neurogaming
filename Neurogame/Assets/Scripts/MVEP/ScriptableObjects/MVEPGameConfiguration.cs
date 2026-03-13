using UnityEngine;

[CreateAssetMenu(fileName = "MVEPGameConfig", menuName = "MVEP/MVEPGameConfig", order = 1)]
public class MVEPGameConfiguration : ScriptableObject
{
  [SerializeField] private int trials;
  public int Trials => trials;

  [SerializeField] private float scoreMax;
  public float ScoreMax => scoreMax;

  [SerializeField] private int warmUpChunks;
  public int WarmUpChunks => warmUpChunks;
}

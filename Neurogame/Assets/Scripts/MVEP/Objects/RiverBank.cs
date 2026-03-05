using UnityEngine;

public class RiverBank : MonoBehaviour
{
  [Header("Anchors")]
  [SerializeField] private MeshFilter treeMeshFilter;
  [SerializeField] private MeshRenderer treeRenderer;

  [SerializeField] private MeshFilter[] mushroomMeshFilters;
  [SerializeField] private MeshRenderer[] mushroomRenderers;

  // [Header("Options")]
  // [SerializeField] private TreeData[] treeOptions;
  // [SerializeField] private MushroomData[] mushroomOptions;

  public void Randomize()
  {
    RandomizeTree();
    // RandomizeMushrooms();
  }

  private void RandomizeTree()
  {
    // var data = treeOptions[Random.Range(0, treeOptions.Length)];

    // treeMeshFilter.sharedMesh = data.Mesh;
    // treeRenderer.sharedMaterial = data.Material;
  }

  private void RandomizeMushrooms()
  {
    // for (int i = 0; i < mushroomMeshFilters.Length; i++)
    // {
    //   var data = mushroomOptions[Random.Range(0, mushroomOptions.Length)];

    //   mushroomMeshFilters[i].sharedMesh = data.Mesh;
    //   mushroomRenderers[i].sharedMaterial = data.Material;
    // }
  }
}
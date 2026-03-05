using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI scoreText;

    void Update()
    {
        if (GameManager.Instance == null) return;

        scoreText.text = GameManager.Instance.CurrentScore.ToString();
    }
}
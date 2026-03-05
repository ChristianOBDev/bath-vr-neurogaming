using System.Collections;
using UnityEngine;
using TMPro;

public class ScorePopup : MonoBehaviour
{
    [Header("Motion")]
    public float riseSpeed = 1.5f;
    public float lifetime = 1.2f;

    [Header("Fade")]
    public float fadeDelay = 0.5f;

    [Header("Combo Colors")]
    public Color comboColor1 = Color.white;
    public Color comboColor2 = new Color(1f, 0.95f, 0.2f);
    public Color comboColor3 = new Color(1f, 0.55f, 0.1f);
    public Color comboColor4 = new Color(1f, 0.2f, 0.2f);

    private TextMeshPro tmp;

    void Awake()
    {
        tmp = GetComponent<TextMeshPro>();
    }

    public void Init(int points, int comboMultiplier)
    {
        tmp.text = points.ToString();
        tmp.color = GetComboColor(comboMultiplier);
        StartCoroutine(Animate());
    }

    private Color GetComboColor(int multiplier)
    {
        return multiplier switch
        {
            1 => comboColor1,
            2 => comboColor2,
            3 => comboColor3,
            _ => comboColor4,
        };
    }

    private IEnumerator Animate()
    {
        float elapsed = 0f;
        Color startColor = tmp.color;

        while (elapsed < lifetime)
        {
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;

            if (elapsed >= fadeDelay)
            {
                float fadeProgress = (elapsed - fadeDelay) / (lifetime - fadeDelay);
                tmp.color = new Color(startColor.r, startColor.g, startColor.b, 1f - fadeProgress);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
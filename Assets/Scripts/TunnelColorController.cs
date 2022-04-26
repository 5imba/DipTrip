using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TunnelColorController : MonoBehaviour
{
    [SerializeField] private Material tunnelMat;
    [SerializeField] private Color[] tunnelColors;
    [SerializeField] private float changeColorSpeed = 0.1f;
    [SerializeField] private float fogColorDarkness = 0.2f;
    [SerializeField] private bool randomColor = false;

    private Color startColor, targetColor;
    private float colorTime = 0f;
    private int prevColorIndex = -1;
    private int colorIndex = 0;
    private bool isChanging = true;

    // Start is called before the first frame update
    void Start()
    {
        startColor = randomColor ? tunnelColors[GetRandomIndex()] : tunnelColors[colorIndex];
        targetColor = randomColor ? tunnelColors[GetRandomIndex()] : tunnelColors[colorIndex + 1];
    }

    // Update is called once per frame
    void Update()
    {
        if (isChanging)
        {
            colorTime += Time.deltaTime * changeColorSpeed;

            if (colorTime >= 1f)
            {
                colorIndex++;
                if (colorIndex == tunnelColors.Length)
                    colorIndex = 0;

                startColor = targetColor;
                targetColor = randomColor ? tunnelColors[GetRandomIndex()] : tunnelColors[colorIndex];

                colorTime = 0f;
            }

            Color newColor = Color.Lerp(startColor, targetColor, colorTime);
            tunnelMat.color = newColor;
            RenderSettings.fogColor = ColorMultiply(newColor, fogColorDarkness);
        }
    }

    public bool IsChanging
    {
        set
        {
            isChanging = value;
        }
    }

    private int GetRandomIndex()
    {
        int counter = 0;

        while (true)
        {
            if (counter > 20)
                return 0;

            int index = Random.Range(0, tunnelColors.Length);
            if (index != prevColorIndex)
            {
                return index;
            }

            counter++;
        }
    }

    private Color ColorMultiply(Color color, float amount)
    {
        return new Color(color.r * amount, color.g * amount, color.b * amount, color.a);
    }
}

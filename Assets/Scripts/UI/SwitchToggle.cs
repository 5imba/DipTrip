using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SwitchToggle : MonoBehaviour
{
    [SerializeField] private RectTransform handleRectTransform;
    [SerializeField] private Color backgroundActiveColor = new Color(0.2392157f, 0.972549f, 0.3960784f, 1f);
    [SerializeField] private Color handleActiveColor = Color.white;
    [SerializeField] private float switchDuration = 0.1f;

    Toggle toggle;
    Vector2 handlePos;
    Image backgroundImage, handleImage; 
    Color backgroundDefaultColor, handleDefaultColor;

    private void Start()
    {
        toggle = GetComponent<Toggle>();
        handlePos = handleRectTransform.anchoredPosition;

        backgroundImage = handleRectTransform.parent.GetComponent<Image>();
        handleImage = handleRectTransform.GetComponent<Image>();

        backgroundDefaultColor = backgroundImage.color;
        handleDefaultColor = handleImage.color;

        toggle.onValueChanged.AddListener(OnSwitch);

        if (toggle.isOn) OnSwitch(true);
    }

    void OnSwitch(bool isOn)
    {
        if (isOn)
        {
            StartCoroutine(SwitchColor(handlePos * -1, backgroundActiveColor, handleActiveColor));
        }
        else
        {
            StartCoroutine(SwitchColor(handlePos, backgroundDefaultColor, handleDefaultColor));
        }
    }

    IEnumerator SwitchColor(Vector2 newHandlePos, Color newBackgroundColor, Color newHandleColor)
    {
        Vector2 startHandlePos = handleRectTransform.anchoredPosition;
        Color startBackgroundColor = backgroundImage.color;
        Color startHandleColor = handleImage.color;

        float time = 0.0f;
        while (time < switchDuration)
        {
            time += Time.deltaTime;
            float t = time / switchDuration;

            handleRectTransform.anchoredPosition = Vector2.Lerp(startHandlePos, newHandlePos, t);
            backgroundImage.color = Color.Lerp(startBackgroundColor, newBackgroundColor, t);
            handleImage.color = Color.Lerp(startHandleColor, newHandleColor, t);

            yield return null;
        }
    }

    private void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnSwitch);
    }
}

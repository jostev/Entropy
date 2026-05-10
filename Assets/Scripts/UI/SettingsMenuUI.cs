using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenuUI: MonoBehaviour
{
    public Slider sensitivitySlider;
    public TMP_Text sensitivityText;

    const string SensitivityKey = "MouseSensitivity";

    void Start()
    {
        float savedSensitivity = PlayerPrefs.GetFloat(SensitivityKey, 1f);

        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = savedSensitivity;
            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        }

        UpdateText(savedSensitivity);
    }

    public void SetSensitivity(float value)
    {
        PlayerPrefs.SetFloat(SensitivityKey, value);
        PlayerPrefs.Save();

        UpdateText(value);
    }

    void UpdateText(float value)
    {
        if (sensitivityText != null)
            sensitivityText.text = value.ToString("0.00");
    }
}

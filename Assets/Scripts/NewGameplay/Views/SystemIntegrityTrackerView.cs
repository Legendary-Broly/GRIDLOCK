using UnityEngine;
using TMPro;
using NewGameplay.Interfaces;

public class SystemIntegrityTrackerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI integrityText;
    [SerializeField] private TMP_FontAsset firaCodeFont;
    [SerializeField] private int barWidth = 20;
    [SerializeField] private RectTransform textTransform;
    [SerializeField] private float maxShakeIntensity = 1.5f;

    private ISystemIntegrityService integrityService;
    private Vector3 initialPosition;

    private void Start()
    {
        if (firaCodeFont == null)
        {
            firaCodeFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/FiraCode-Regular SDF");
        }

        if (firaCodeFont != null)
        {
            integrityText.font = firaCodeFont;
        }

        if (textTransform == null)
        {
            textTransform = integrityText.rectTransform;
        }

        initialPosition = textTransform.anchoredPosition;
    }

    public void Initialize(ISystemIntegrityService service)
    {
        integrityService = service;
        integrityService.OnIntegrityChanged += OnIntegrityChanged;
        OnIntegrityChanged(integrityService.CurrentIntegrity);
    }

    private void OnIntegrityChanged(float value)
    {
        integrityText.text = GenerateAsciiBar(value);
        integrityText.color = GetInterpolatedColor(value);
        ApplyShake(value);
    }

    private string GenerateAsciiBar(float current)
    {
        float percent = Mathf.Clamp01(current / 100f);
        int filled = Mathf.RoundToInt(barWidth * percent);
        int empty = barWidth - filled;

        string bar = new string('█', filled) + new string('░', empty);
        return $"SYSTEM INTEGRITY: [{bar}] {Mathf.RoundToInt(current)}%";
    }

    private Color GetInterpolatedColor(float value)
    {
        Color cyan = new Color32(0x00, 0xCF, 0xFD, 255);  // #00CFFD
        Color yellow = Color.yellow;
        Color red = Color.red;

        if (value >= 50f)
        {
            float t = Mathf.InverseLerp(100f, 50f, value);
            return Color.Lerp(cyan, yellow, t);
        }
        else
        {
            float t = Mathf.InverseLerp(50f, 10f, value);
            return Color.Lerp(yellow, red, t);
        }
    }

    private void ApplyShake(float value)
    {
        float integrityPercent = Mathf.Clamp01(value / 100f);
        float intensity = Mathf.Lerp(maxShakeIntensity, 0f, integrityPercent);

        Vector2 offset = Random.insideUnitCircle * intensity;
        textTransform.anchoredPosition = initialPosition + new Vector3(offset.x, offset.y, 0f);
    }

    private void OnDisable()
    {
        if (textTransform != null)
        {
            textTransform.anchoredPosition = initialPosition;
        }
    }
}

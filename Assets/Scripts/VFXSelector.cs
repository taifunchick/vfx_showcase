using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class VFXSelector : MonoBehaviour
{
    [Header("Камеры и зоны")]
    public Transform[] cameraPositions;      // 5 позиций
    public GameObject[] effectZones;         // 5 зон
    public GameObject[] sliderGroups;        // 5 панелей со слайдерами
    public TextMeshProUGUI effectNameText;

    [Header("Скорость камеры")]
    public float cameraMoveSpeed = 5f;

    // Размер
    private float[] effectSizes;
    public Slider[] sizeSliders;
    public TextMeshProUGUI[] sizeValueTexts;

    // Цвет (Hue)
    private float[] effectHues;
    public Slider[] colorSliders;
    public TextMeshProUGUI[] colorValueTexts;

    // Скорость (для Combined и любых других эффектов с ParticleSystem)
    private float[] effectSpeeds;
    public Slider[] speedSliders;
    public TextMeshProUGUI[] speedValueTexts;

    // Для Mesh: Dissolve и Transparency (индекс эффекта Mesh = 2)
    private float[] effectDissolve;
    private float[] effectTransparency;
    public Slider[] dissolveSliders;
    public TextMeshProUGUI[] dissolveValueTexts;
    public Slider[] transparencySliders;
    public TextMeshProUGUI[] transparencyValueTexts;

    private int currentIndex = 0;
    private bool isMoving = false;
    private Dictionary<int, Material> effectMaterials;
    private Dictionary<int, ParticleSystem> effectParticleSystems; // для управления скоростью и цветом

    void Start()
    {
        int n = effectZones.Length;
        effectSizes = new float[n];
        effectHues = new float[n];
        effectSpeeds = new float[n];
        effectDissolve = new float[n];
        effectTransparency = new float[n];
        effectMaterials = new Dictionary<int, Material>();
        effectParticleSystems = new Dictionary<int, ParticleSystem>();

        for (int i = 0; i < n; i++)
        {
            effectSizes[i] = 1f;
            effectHues[i] = 0f;
            effectSpeeds[i] = 1f;
            effectDissolve[i] = 0f;
            effectTransparency[i] = 1f;
        }

        // Настройка слайдеров размера
        for (int i = 0; i < sizeSliders.Length; i++)
        {
            if (sizeSliders[i] == null) continue;
            int idx = i;
            sizeSliders[i].minValue = 0.2f;
            sizeSliders[i].maxValue = 3f;
            sizeSliders[i].onValueChanged.RemoveAllListeners();
            sizeSliders[i].onValueChanged.AddListener(v => OnSizeChanged(v, idx));
            sizeSliders[i].value = effectSizes[i];
            if (sizeValueTexts[i]) sizeValueTexts[i].text = $"Size: {effectSizes[i]:F2}";
        }

        // Настройка слайдеров цвета (Hue)
        for (int i = 0; i < colorSliders.Length; i++)
        {
            if (colorSliders[i] == null) continue;
            int idx = i;
            colorSliders[i].minValue = 0f;
            colorSliders[i].maxValue = 1f;
            colorSliders[i].onValueChanged.RemoveAllListeners();
            colorSliders[i].onValueChanged.AddListener(v => OnColorChanged(v, idx));
            colorSliders[i].value = effectHues[i];
            if (colorValueTexts[i]) colorValueTexts[i].text = $"Color: {HueToName(effectHues[i])}";
        }

        // Настройка слайдеров скорости
        for (int i = 0; i < speedSliders.Length; i++)
        {
            if (speedSliders[i] == null) continue;
            int idx = i;
            speedSliders[i].minValue = 0f;
            speedSliders[i].maxValue = 2f;
            speedSliders[i].onValueChanged.RemoveAllListeners();
            speedSliders[i].onValueChanged.AddListener(v => OnSpeedChanged(v, idx));
            speedSliders[i].value = effectSpeeds[i];
            if (speedValueTexts[i]) speedValueTexts[i].text = $"Speed: {effectSpeeds[i]:F2}";
        }

        // Настройка dissolve и transparency (только для Mesh – индекс 2, но массив на 5)
        for (int i = 0; i < dissolveSliders.Length; i++)
        {
            if (dissolveSliders[i] != null)
            {
                int idx = i;
                dissolveSliders[i].minValue = 0f;
                dissolveSliders[i].maxValue = 1f;
                dissolveSliders[i].onValueChanged.RemoveAllListeners();
                dissolveSliders[i].onValueChanged.AddListener(v => OnDissolveChanged(v, idx));
                dissolveSliders[i].value = effectDissolve[i];
                if (dissolveValueTexts[i]) dissolveValueTexts[i].text = $"Dissolve: {effectDissolve[i]:F2}";
            }
            if (transparencySliders[i] != null)
            {
                int idx = i;
                transparencySliders[i].minValue = 0f;
                transparencySliders[i].maxValue = 1f;
                transparencySliders[i].onValueChanged.RemoveAllListeners();
                transparencySliders[i].onValueChanged.AddListener(v => OnTransparencyChanged(v, idx));
                transparencySliders[i].value = effectTransparency[i];
                if (transparencyValueTexts[i]) transparencyValueTexts[i].text = $"Alpha: {effectTransparency[i]:F2}";
            }
        }

        // Поиск материалов и ParticleSystem
        for (int i = 0; i < n; i++)
        {
            // ParticleSystem для скорости и цвета
            var ps = effectZones[i].GetComponentInChildren<ParticleSystem>();
            if (ps != null) effectParticleSystems[i] = ps;

            // Поиск материала: сначала MeshRenderer, потом ParticleSystemRenderer
            Material mat = null;
            var meshRend = effectZones[i].GetComponentInChildren<MeshRenderer>();
            if (meshRend != null)
            {
                mat = meshRend.material;
                Debug.Log($"Зона {i} ({effectZones[i].name}) нашла MeshRenderer материал: {mat.name}");
            }
            else
            {
                var psRend = effectZones[i].GetComponentInChildren<ParticleSystemRenderer>();
                if (psRend != null)
                {
                    mat = psRend.material;
                    Debug.Log($"Зона {i} ({effectZones[i].name}) нашла ParticleSystemRenderer материал: {mat.name}");
                }
                else
                {
                    Debug.LogWarning($"Зона {i} ({effectZones[i].name}) не имеет ни MeshRenderer, ни ParticleSystemRenderer");
                }
            }
            if (mat != null) effectMaterials[i] = mat;
        }

        // Начальное состояние UI
        effectNameText.text = GetEffectName(0);
        for (int i = 0; i < sliderGroups.Length; i++)
            sliderGroups[i].SetActive(i == 0);

        Camera.main.transform.position = cameraPositions[0].position;
        Camera.main.transform.rotation = cameraPositions[0].rotation;

        ApplyAllParameters(0);
    }

    // Применяет все параметры (размер, цвет, скорость, dissolve, прозрачность) к указанному эффекту
    void ApplyAllParameters(int index)
    {
        // Размер
        effectZones[index].transform.localScale = Vector3.one * effectSizes[index];

        // Цвет (через специальный метод, учитывающий ParticleSystem и _TintColor)
        Color col = Color.HSVToRGB(effectHues[index], 1f, 1f);
        ApplyColorToEffect(index, col);

        // Скорость симуляции
        if (effectParticleSystems.ContainsKey(index) && effectParticleSystems[index] != null)
        {
            var main = effectParticleSystems[index].main;
            main.simulationSpeed = effectSpeeds[index];
        }

        // Dissolve и прозрачность (если материал поддерживает)
        if (effectMaterials.ContainsKey(index) && effectMaterials[index] != null)
        {
            effectMaterials[index].SetFloat("_DissolveAmount", effectDissolve[index]);
            effectMaterials[index].SetFloat("_DissolveThreshold", effectDissolve[index]);
            Color c = effectMaterials[index].color;
            c.a = effectTransparency[index];
            effectMaterials[index].color = c;
        }
    }

    // Универсальный метод изменения цвета для любых эффектов (частицы + меши)
    private void ApplyColorToEffect(int index, Color color)
    {
        // 1. Если есть ParticleSystem, меняем startColor (работает всегда)
        if (effectParticleSystems.ContainsKey(index) && effectParticleSystems[index] != null)
        {
            var main = effectParticleSystems[index].main;
            main.startColor = color;
        }
        
        // 2. Если есть материал, пробуем стандартный Color, а также _TintColor (для шейдера Mobile/Particles/Additive)
        if (effectMaterials.ContainsKey(index) && effectMaterials[index] != null)
        {
            var mat = effectMaterials[index];
            mat.color = color;
            if (mat.HasProperty("_TintColor"))
                mat.SetColor("_TintColor", color);
            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", color);
        }
    }

    // ----- Обработчики слайдеров -----
    public void OnSizeChanged(float val, int idx)
    {
        effectSizes[idx] = val;
        if (sizeValueTexts[idx]) sizeValueTexts[idx].text = $"Size: {val:F2}";
        if (currentIndex == idx) effectZones[idx].transform.localScale = Vector3.one * val;
    }

    public void OnColorChanged(float hue, int idx)
    {
        effectHues[idx] = hue;
        string name = HueToName(hue);
        if (colorValueTexts[idx]) colorValueTexts[idx].text = $"Color: {name}";
        if (currentIndex == idx)
        {
            Color col = Color.HSVToRGB(hue, 1f, 1f);
            ApplyColorToEffect(idx, col);
        }
    }

    public void OnSpeedChanged(float val, int idx)
    {
        effectSpeeds[idx] = val;
        if (speedValueTexts[idx]) speedValueTexts[idx].text = $"Speed: {val:F2}";
        if (currentIndex == idx && effectParticleSystems.ContainsKey(idx) && effectParticleSystems[idx] != null)
        {
            var main = effectParticleSystems[idx].main;
            main.simulationSpeed = val;
        }
    }

    public void OnDissolveChanged(float val, int idx)
    {
        effectDissolve[idx] = val;
        if (dissolveValueTexts[idx]) dissolveValueTexts[idx].text = $"Dissolve: {val:F2}";
        if (currentIndex == idx && effectMaterials.ContainsKey(idx) && effectMaterials[idx] != null)
        {
            effectMaterials[idx].SetFloat("_DissolveAmount", val);
            effectMaterials[idx].SetFloat("_DissolveThreshold", val);
        }
    }

    public void OnTransparencyChanged(float val, int idx)
    {
        effectTransparency[idx] = val;
        if (transparencyValueTexts[idx]) transparencyValueTexts[idx].text = $"Alpha: {val:F2}";
        if (currentIndex == idx && effectMaterials.ContainsKey(idx) && effectMaterials[idx] != null)
        {
            Color c = effectMaterials[idx].color;
            c.a = val;
            effectMaterials[idx].color = c;
        }
    }

    // ----- Переключение эффекта -----
    public void SelectEffect(int index)
    {
        if (isMoving || index == currentIndex) return;
        currentIndex = index;
        effectNameText.text = GetEffectName(index);

        for (int i = 0; i < sliderGroups.Length; i++)
            sliderGroups[i].SetActive(i == index);

        if (sizeSliders[currentIndex] != null) sizeSliders[currentIndex].value = effectSizes[currentIndex];
        if (colorSliders[currentIndex] != null) colorSliders[currentIndex].value = effectHues[currentIndex];
        if (speedSliders[currentIndex] != null) speedSliders[currentIndex].value = effectSpeeds[currentIndex];
        if (dissolveSliders[currentIndex] != null) dissolveSliders[currentIndex].value = effectDissolve[currentIndex];
        if (transparencySliders[currentIndex] != null) transparencySliders[currentIndex].value = effectTransparency[currentIndex];

        StartCoroutine(MoveCameraSmoothly(cameraPositions[index]));
    }

    IEnumerator MoveCameraSmoothly(Transform target)
    {
        isMoving = true;
        Camera cam = Camera.main;
        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;
        float distance = Vector3.Distance(startPos, target.position);
        float duration = Mathf.Clamp(distance / cameraMoveSpeed, 0.5f, 2f);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t);
            cam.transform.position = Vector3.Lerp(startPos, target.position, t);
            cam.transform.rotation = Quaternion.Slerp(startRot, target.rotation, t);
            yield return null;
        }
        cam.transform.position = target.position;
        cam.transform.rotation = target.rotation;
        isMoving = false;
    }

    private string GetEffectName(int idx) => new[] { "Particle", "Flipbook", "Mesh", "Shader", "Combined" }[idx];

    private string HueToName(float hue)
    {
        if (hue < 0.05f) return "Red";
        if (hue < 0.15f) return "Orange";
        if (hue < 0.35f) return "Yellow";
        if (hue < 0.5f) return "Green";
        if (hue < 0.7f) return "Cyan";
        if (hue < 0.85f) return "Blue";
        return "White";
    }
}
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class VFXSelector : MonoBehaviour
{
    [Header("Камеры и зоны")]
    public Transform[] cameraPositions;
    public GameObject[] effectZones;
    public GameObject[] sliderGroups;
    public TextMeshProUGUI effectNameText;

    [Header("Скорость камеры")]
    public float cameraMoveSpeed = 5f;

    private float[] effectSizes;
    public Slider[] sizeSliders;
    public TextMeshProUGUI[] sizeValueTexts;

    private float[] effectHues;
    public Slider[] colorSliders;
    public TextMeshProUGUI[] colorValueTexts;

    private float[] effectSpeeds;
    public Slider[] speedSliders;
    public TextMeshProUGUI[] speedValueTexts;

    private float[] effectDissolve;
    private float[] effectTransparency;
    public Slider[] dissolveSliders;
    public TextMeshProUGUI[] dissolveValueTexts;
    public Slider[] transparencySliders;
    public TextMeshProUGUI[] transparencyValueTexts;

    private int currentIndex = 0;
    private bool isMoving = false;
    private Dictionary<int, Material> effectMaterials;
    private Dictionary<int, ParticleSystem> effectParticleSystems;

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

        for (int i = 0; i < n; i++)
        {
            var ps = effectZones[i].GetComponentInChildren<ParticleSystem>();
            if (ps != null) effectParticleSystems[i] = ps;

            Material mat = null;
            var meshRend = effectZones[i].GetComponentInChildren<MeshRenderer>();
            if (meshRend != null)
                mat = meshRend.material;
            else
            {
                var psRend = effectZones[i].GetComponentInChildren<ParticleSystemRenderer>();
                if (psRend != null) mat = psRend.material;
            }
            if (mat != null) effectMaterials[i] = mat;
        }

        effectNameText.text = GetEffectName(0);
        for (int i = 0; i < sliderGroups.Length; i++)
            sliderGroups[i].SetActive(i == 0);

        Camera.main.transform.position = cameraPositions[0].position;
        Camera.main.transform.rotation = cameraPositions[0].rotation;

        ApplyAllParameters(0);
    }

    void ApplyAllParameters(int index)
    {
        effectZones[index].transform.localScale = Vector3.one * effectSizes[index];

        Color col = Color.HSVToRGB(effectHues[index], 1f, 1f);
        ApplyColorToEffect(index, col);

        if (effectParticleSystems.ContainsKey(index) && effectParticleSystems[index] != null)
        {
            var main = effectParticleSystems[index].main;
            main.simulationSpeed = effectSpeeds[index];
        }

        ApplyDissolveToEffect(index, effectDissolve[index]);
        ApplyTransparencyToEffect(index, effectTransparency[index]);
    }

    private void ApplyColorToEffect(int index, Color color)
    {
        if (effectParticleSystems.ContainsKey(index) && effectParticleSystems[index] != null)
        {
            var main = effectParticleSystems[index].main;
            main.startColor = color;
        }
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

    private void ApplyDissolveToEffect(int index, float value)
    {
        if (effectMaterials.ContainsKey(index) && effectMaterials[index] != null)
        {
            var mat = effectMaterials[index];
            if (mat.HasProperty("_DissolveAmount"))
                mat.SetFloat("_DissolveAmount", value);
            else if (mat.HasProperty("_DissolveThreshold"))
                mat.SetFloat("_DissolveThreshold", value);
            else if (mat.HasProperty("_SliceAmount"))
                mat.SetFloat("_SliceAmount", value);
            else if (mat.HasProperty("Dissolve"))
                mat.SetFloat("Dissolve", value);
            else
            {
                if (!dissolveWarningShown.ContainsKey(index))
                {
                    Debug.LogWarning($"Материал {mat.name} не имеет параметра dissolve (пробовал: _DissolveAmount, _DissolveThreshold, _SliceAmount, Dissolve)");
                    dissolveWarningShown[index] = true;
                }
            }
        }
    }
    private Dictionary<int, bool> dissolveWarningShown = new Dictionary<int, bool>();

    private void ApplyTransparencyToEffect(int index, float value)
    {
        if (effectMaterials.ContainsKey(index) && effectMaterials[index] != null)
        {
            var mat = effectMaterials[index];
            if (mat.HasProperty("_Transparency"))
                mat.SetFloat("_Transparency", value);
            else if (mat.HasProperty("_Alpha"))
                mat.SetFloat("_Alpha", value);
            else if (mat.HasProperty("Transparency"))
                mat.SetFloat("Transparency", value);
            else if (mat.HasProperty("alpha"))
                mat.SetFloat("alpha", value);
            else
            {
                Color c = mat.color;
                c.a = value;
                mat.color = c;
            }
        }
    }

    public void OnSizeChanged(float val, int idx)
    {
        effectSizes[idx] = val;
        if (sizeValueTexts[idx]) sizeValueTexts[idx].text = $"Size: {val:F2}";
        if (currentIndex == idx) effectZones[idx].transform.localScale = Vector3.one * val;
    }

    public void OnColorChanged(float hue, int idx)
    {
        effectHues[idx] = hue;
        if (colorValueTexts[idx]) colorValueTexts[idx].text = $"Color: {HueToName(hue)}";
        if (currentIndex == idx)
            ApplyColorToEffect(idx, Color.HSVToRGB(hue, 1f, 1f));
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
        if (currentIndex == idx)
            ApplyDissolveToEffect(idx, val);
    }

    public void OnTransparencyChanged(float val, int idx)
    {
        effectTransparency[idx] = val;
        if (transparencyValueTexts[idx]) transparencyValueTexts[idx].text = $"Alpha: {val:F2}";
        if (currentIndex == idx)
            ApplyTransparencyToEffect(idx, val);
    }

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
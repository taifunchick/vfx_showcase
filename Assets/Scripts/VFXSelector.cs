using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class VFXSelector : MonoBehaviour
{
    public Transform[] cameraPositions;
    public GameObject[] effectZones;
    public GameObject[] sliderGroups;
    public TextMeshProUGUI effectNameText;

    public float cameraMoveSpeed = 5f;

    private bool isMoving = false;
    private float[] effectSizes;
    private float[] effectRotations; // Массив для хранения значений вращения
    
    public Slider[] sizeSliders;
    public TextMeshProUGUI[] sliderValueTexts;
    
    // Новые массивы для слайдера вращения
    public Slider[] rotationSliders;
    public TextMeshProUGUI[] rotationValueTexts;

    private int currentIndex = 0;

    void Start()
    {
        effectSizes = new float[] { 1f, 1f, 1f, 1f };
        effectRotations = new float[] { 0f, 0f, 0f, 0f }; // Инициализация вращения
        
        // Настройка слайдеров размера
        for (int i = 0; i < sizeSliders.Length; i++)
        {
            if (sizeSliders[i] != null)
            {
                int capturedIndex = i;
                sizeSliders[i].onValueChanged.RemoveAllListeners();
                sizeSliders[i].onValueChanged.AddListener((value) => OnSizeSliderChanged(value, capturedIndex));
                sizeSliders[i].value = effectSizes[i];
                
                if (sliderValueTexts[i] != null)
                    sliderValueTexts[i].text = "Size: " + effectSizes[i].ToString("F2");
            }
        }
        
        // Настройка слайдеров вращения
        for (int i = 0; i < rotationSliders.Length; i++)
        {
            if (rotationSliders[i] != null)
            {
                int capturedIndex = i;
                rotationSliders[i].onValueChanged.RemoveAllListeners();
                rotationSliders[i].onValueChanged.AddListener((value) => OnRotationSliderChanged(value, capturedIndex));
                rotationSliders[i].value = effectRotations[i];
                
                if (rotationValueTexts[i] != null)
                    rotationValueTexts[i].text = "Rotation: " + effectRotations[i].ToString("F0") + "°";
            }
        }
        
        effectNameText.text = GetEffectName(0);
        
        for (int i = 0; i < sliderGroups.Length; i++)
            sliderGroups[i].SetActive(i == 0);
        
        Camera.main.transform.position = cameraPositions[0].position;
        Camera.main.transform.rotation = cameraPositions[0].rotation;
        
        effectZones[0].transform.localScale = Vector3.one * effectSizes[0];
        effectZones[0].transform.rotation = Quaternion.Euler(0, effectRotations[0], 0);
    }

    public void SelectEffect(int index)
    {
        if (isMoving) return;
        if (index == currentIndex) return;

        currentIndex = index;
        effectNameText.text = GetEffectName(index);
        
        for (int i = 0; i < sliderGroups.Length; i++)
            sliderGroups[i].SetActive(i == index);
        
        if (sizeSliders[currentIndex] != null)
            sizeSliders[currentIndex].value = effectSizes[currentIndex];
        
        if (rotationSliders[currentIndex] != null)
            rotationSliders[currentIndex].value = effectRotations[currentIndex];
        
        StartCoroutine(MoveCameraSmoothly(cameraPositions[index]));
    }

    IEnumerator MoveCameraSmoothly(Transform target)
    {
        isMoving = true;
        Camera cam = Camera.main;
        
        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;
        
        float distance = Vector3.Distance(startPos, target.position);
        float duration = distance / cameraMoveSpeed;
        duration = Mathf.Clamp(duration, 0.5f, 2f); 
        
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

    public void OnSizeSliderChanged(float value, int effectIndex)
    {
        effectSizes[effectIndex] = value;
        
        if (sliderValueTexts[effectIndex] != null)
            sliderValueTexts[effectIndex].text = "Size: " + value.ToString("F2");
        
        if (currentIndex == effectIndex)
        {
            effectZones[effectIndex].transform.localScale = Vector3.one * value;
        }
    }

    // Новый метод для слайдера вращения
    public void OnRotationSliderChanged(float value, int effectIndex)
    {
        effectRotations[effectIndex] = value;
        
        if (rotationValueTexts[effectIndex] != null)
            rotationValueTexts[effectIndex].text = "Rotation: " + value.ToString("F0") + "°";
        
        if (currentIndex == effectIndex)
        {
            effectZones[effectIndex].transform.rotation = Quaternion.Euler(0, value, 0);
        }
    }

    private string GetEffectName(int idx)
    {
        return new[] { "Particle", "Flipbook", "Mesh", "Shader" }[idx];
    }
}
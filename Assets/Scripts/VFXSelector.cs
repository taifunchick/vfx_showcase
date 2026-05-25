using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VFXSelector : MonoBehaviour
{
    public Transform[] cameraPositions;
    public GameObject[] effectZones;
    public GameObject[] sliderGroups;
    public TextMeshProUGUI effectNameText;
    
    private float[] effectSizes;
    
    public Slider[] sizeSliders;

    private int currentIndex = 0;

    void Start()
    {
        sizeSliders = new Slider[sliderGroups.Length];
        for (int i = 0; i < sliderGroups.Length; i++)
        {
            sizeSliders[i] = sliderGroups[i].GetComponentInChildren<Slider>();
            if (sizeSliders[i] != null)
            {
                int capturedIndex = i;
                sizeSliders[i].onValueChanged.AddListener((value) => OnSizeSliderChanged(value, capturedIndex));
            }
        }

        effectSizes = new float[] { 1f, 1f, 1f, 1f };
        
        for (int i = 0; i < sizeSliders.Length; i++)
        {
            sizeSliders[i].value = effectSizes[i];
        }
        
        effectNameText.text = GetEffectName(0);
        
        for (int i = 0; i < sliderGroups.Length; i++)
            sliderGroups[i].SetActive(i == 0);
        
        Camera.main.transform.position = cameraPositions[0].position;
        Camera.main.transform.rotation = cameraPositions[0].rotation;
        
        effectZones[0].transform.localScale = Vector3.one * effectSizes[0];
    }

    public void SelectEffect(int index)
    {
        currentIndex = index;
        Camera.main.transform.position = cameraPositions[index].position;
        Camera.main.transform.rotation = cameraPositions[index].rotation;

        effectNameText.text = GetEffectName(index);
        
        for (int i = 0; i < sliderGroups.Length; i++)
            sliderGroups[i].SetActive(i == index);
        
        sizeSliders[currentIndex].value = effectSizes[currentIndex];
    }

    public void OnSizeSliderChanged(float value, int effectIndex)
    {
        effectSizes[effectIndex] = value;
        
        if (currentIndex == effectIndex)
        {
            effectZones[effectIndex].transform.localScale = Vector3.one * value;
        }
    }

    private string GetEffectName(int idx)
    {
        return new[] { "Particle", "Flipbook", "Mesh", "Shader" }[idx];
    }
}
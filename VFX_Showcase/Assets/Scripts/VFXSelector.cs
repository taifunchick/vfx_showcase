using UnityEngine;
using TMPro;

public class VFXSelector : MonoBehaviour
{
    public Transform[] cameraPositions;
    public GameObject[] effectZones;
    public GameObject[] sliderGroups;
    public TextMeshProUGUI effectNameText;

    private int currentIndex = 0;

    public void SelectEffect(int index)
    {
        currentIndex = index;
        Camera.main.transform.position = cameraPositions[index].position;
        Camera.main.transform.rotation = cameraPositions[index].rotation;

        effectNameText.text = GetEffectName(index);
        
        for (int i = 0; i < sliderGroups.Length; i++)
            sliderGroups[i].SetActive(i == index);
    }

    public void OnSizeSliderChanged(float value)
    {
        effectZones[currentIndex].transform.localScale = Vector3.one * value;
    }

    private string GetEffectName(int idx)
    {
        return new[] { "Particle", "Flipbook", "Mesh", "Shader" }[idx];
    }
}
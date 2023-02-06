using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    public CrosshairData data;

    public void generateCrosshair(GameObject parent)
    {
        GameObject container = new GameObject("CrosshairContainer");
        container.layer = LayerMask.NameToLayer("UI");
        RectTransform trans = container.AddComponent<RectTransform>();
        trans.SetParent(parent.transform);
        float overallSize = (data.length + data.spacing) * 2;
        trans.sizeDelta = new Vector2(overallSize, overallSize);
        trans.anchoredPosition = new Vector2(0f, 0f);

        GenerateCrosshairComponent(new Vector2(0, 1), container);
        GenerateCrosshairComponent(new Vector2(0, -1), container);
        GenerateCrosshairComponent(new Vector2(1, 0), container);
        GenerateCrosshairComponent(new Vector2(-1, 0), container);
    }

    private void GenerateCrosshairComponent(Vector2 position, GameObject parent)
    {
        GameObject container = new GameObject("CrosshairComponent");
        container.layer = LayerMask.NameToLayer("UI");
        RectTransform trans = container.AddComponent<RectTransform>();
        

        trans.SetParent(parent.transform);
        trans.anchoredPosition = new Vector2(0f, 0f);
        

        float centerDistance = data.length / 2 + data.spacing;

        if (position[0] != 0)
        {
            trans.sizeDelta = new Vector2(data.length, data.width);
            trans.localPosition = new Vector2(position[0] * centerDistance, 0);
        }
        else
        {
            trans.sizeDelta = new Vector2(data.width, data.length);
            trans.localPosition = new Vector2(0, position[1] * centerDistance);
        }

        Image image = container.AddComponent<Image>();
        image.color = data.color;
    }

    public void ShowCrosshair()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    public void HideCrosshair()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }
}

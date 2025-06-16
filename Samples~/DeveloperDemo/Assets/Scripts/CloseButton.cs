using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class CloseButton : MonoBehaviour, IPointerClickHandler
{
    public AssetViewer assetViewer;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            assetViewer.Hide();
        }
    }
}

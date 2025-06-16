using System;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DefaultNamespace
{
    public class ItemButton: MonoBehaviour, IPointerClickHandler
    {
        public string id;
        public AssetViewer viewer;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                viewer.Show(id);
            }
        }
    }
}
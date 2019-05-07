using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    public class ItemData : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler {
        public Item item;
        public int count;
        public int slot;

        private Inventory inv;
        private Tooltip tooltip;
        private Vector2 offset;

        public void Start()
        {
            // обьявляем инвентарь
            inv = GameObject.Find("Inventory").GetComponent<Inventory>();
            tooltip = inv.GetComponent<Tooltip>();
        }

        // начало перетаскивания, 
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (item != null) {
                // что это такие ?? (офсет)
                offset = eventData.position -  (Vector2)this.transform.position;
                this.transform.SetParent(this.transform.parent.parent);
                this.transform.position = eventData.position - offset;
                GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (item != null)
            {
                this.transform.position = eventData.position - offset;
            }
        }

        // поле завершения перетаскивания итема присваем ему положение
        public void OnEndDrag(PointerEventData eventData)
        {
            this.transform.SetParent(inv.slots[slot].transform);
            this.transform.position = inv.slots[slot].transform.position;
            GetComponent<CanvasGroup>().blocksRaycasts = true;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            tooltip.Activate(item);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tooltip.Deactivate();
        }
    }
}
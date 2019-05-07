using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    public class Slot : MonoBehaviour, IDropHandler {

        private Inventory inv;
        [SerializeField]
        private int id;
        [SerializeField]
        private GameObject item;

        // Use this for initialization
        void Start()
        {
            inv = GameObject.Find("Inventory").GetComponent<Inventory>();
        }

        public void OnDrop(PointerEventData eventData)
        {
            ItemData dropedItem = eventData.pointerDrag.GetComponent<ItemData>();
            if (inv.items[id].ID == -1)
            {
                inv.items[dropedItem.slot] = new Item();
                inv.items[id] = dropedItem.item;
                dropedItem.slot = id;
            }
            else if(dropedItem.slot != id)
            {
                Transform item = this.transform.GetChild(0);
                item.GetComponent<ItemData>().slot = dropedItem.slot;
                item.transform.SetParent(inv.slots[dropedItem.slot].transform);
                item.transform.position = inv.slots[dropedItem.slot].transform.position;

                dropedItem.slot = id;
                dropedItem.transform.SetParent(this.transform);
                dropedItem.transform.position = this.transform.position;

                inv.items[dropedItem.slot] = item.GetComponent<ItemData>().item;
                inv.items[id] = dropedItem.item;
            }
        }   
    }
}
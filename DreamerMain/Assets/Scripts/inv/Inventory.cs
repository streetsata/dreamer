using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class Inventory : MonoBehaviour {

        ItemDataBase dataBase;
        public GameObject inventoryItem;

        public List<Item> items = new List<Item>();
        public List<GameObject> slots;

        private void Start()
        {
            dataBase = GetComponent<ItemDataBase>();
            //// цикл для инициализации списка итемов
            for (int i = 0; i < slots.Count; i++)
            {
                items.Add(new Item());
            }

            AddItem(1);
            AddItem(0);
        }

        // Добавление нового итема в поле слота 
        public void AddItem(int id)
        {
            id = Random.Range(0, 3);   
            Item itemToAdd = dataBase.FetchItemById(id);
            //проверяем если итем есть в иневентаре то создаем его, в противном случае только добавляем каунт
            if (itemToAdd.Stackable & CheckIfItemIsInInventory(itemToAdd))
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    if (items[i].ID == id)
                    {
                        ItemData data = slots[i].transform.GetChild(0).GetComponent<ItemData>();
                        data.count++;
                        data.transform.GetChild(0).GetComponent<Text>().text = data.count.ToString();
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    if (items[i].ID == -1)
                    {
                        items[i] = itemToAdd;
                        GameObject itemObj = Instantiate(inventoryItem);
                        itemObj.GetComponent<ItemData>().item = itemToAdd;
                        itemObj.GetComponent<ItemData>().count = 1;
                        itemObj.GetComponent<ItemData>().slot = i;
                        itemObj.transform.SetParent(slots[i].transform);
                        itemObj.transform.position = slots[i].transform.position;
                        itemObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0,0);
                        itemObj.GetComponent<Image>().sprite = itemToAdd.Sprite;
                        itemObj.transform.localScale = new Vector3(1f, 1f, 1f);
                        itemObj.name = itemToAdd.Title;
                        break;
                    }
                }
            }
        }

        // поверка наличия предмета в инвентаре
        public bool CheckIfItemIsInInventory (Item item){
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ID==item.ID) {
                    return true;
                }
            }
            return false;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using Assets.Scripts;
using System.IO;

namespace Assets.Scripts
{
    // клас для дазы предметов
    public class ItemDataBase : MonoBehaviour
    {
        private List<Item> database = new List<Item>();
        private JsonData itemData;

        private void Start()
        {
            // определение файла с данными о предметах
            itemData = JsonMapper.ToObject(File.ReadAllText(Application.dataPath + "/StreamingAssets/Items.json"));
            // вызов метода для получения предметов
            ConctructItemDataBase();

           Debug.Log(FetchItemById(0).Description);
        }

        // поиск в листе итем по полю id
        public Item FetchItemById(int id)
        {
            for (int i = 0; i < database.Count; i++)
                if (database[i].ID == id)
                {
                    return database[i];
                }
            return null;  
        }

        // метод для сбора данных из файла типо базы данных
        void ConctructItemDataBase()
        {
            for (int i = 0; i < itemData.Count; i++)
            {
                // добавление каждого поля из файла в нужные переменные в конструкторе
                database.Add(new Item((int)itemData[i]["id"], itemData[i]["title"].ToString(), (int)itemData[i]["value"], 
                    (int)itemData[i]["stats"]["power"], (int)itemData[i]["stats"]["defense"], (int)itemData[i]["stats"]["vitality"], itemData[i]["desctiption"].ToString(),
                    (bool)itemData[i]["stackable"], (int)itemData[i]["rarity"], itemData[i]["slug"].ToString()));
            }
        }
    }
}
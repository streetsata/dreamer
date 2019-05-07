using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    // класс для предметов инвентаря
   public class Item
    {
        // поля класса
        public int ID { get; set; }
        public string Title { get; set; }
        public int Value { get; set; }
        public int Power { get; set; }
        public int Defense { get; set; }
        public int Vitality { get; set; }
        public string Description { get; set; }
        public bool Stackable { get; set; }
        public int Rarity { get; set; }
        public string Slug { get; set; }
        public Sprite Sprite { get; set; }

        //Конструкторы
        public Item(int id, string title, int value, int power, int defense, int vitality, string description, bool stackable,int rarity, string slug) {
            this.ID = id;
            this.Title = title;
            this.Value = value;
            this.Power = power;
            this.Defense = defense;
            this.Vitality = vitality;
            this.Description = description;
            this.Stackable = stackable;
            this.Rarity = rarity;
            this.Slug = slug;
            this.Sprite = Resources.Load<Sprite>("Sprites/" + slug);
        }

        public Item(int id, string title, int value)
        {
            this.ID = id;
            this.Title = title;
            this.Value = value;
        }

        // пустая ячейка инвнтаря
        public Item() {
            this.ID = -1;
        }
    }
}
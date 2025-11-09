#region

//文件创建者：Egg
//创建时间：11-09 01:01

#endregion

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


namespace KOJC.Core
{
    [Serializable, HideReferenceObjectPicker]
    public class HexCell
    {
        [SerializeField] private HexCoord     coord;
        [SerializeField] private List<string> tags = new();
        [SerializeField] private HexItem      item;
        [SerializeField] private GameObject   obj;

        public HexCell(HexCoord coordinate,GameObject o)
        {
            coord = coordinate;
            obj   = o;
        }

        #region 属性访问器

        public HexCoord     Coord   => coord;
        public List<string> Tags    => tags;
        public HexItem      Item    => item;
        public bool         HasItem => item != null;

        public GameObject Obj => obj;

        #endregion

        #region Tag管理方法

        public bool HasTag(string tag)
        {
            return tags.Contains(tag);
        }

        public void AddTag(string tag)
        {
            if (!tags.Contains(tag))
                tags.Add(tag);
        }

        public void RemoveTag(string tag)
        {
            tags.Remove(tag);
        }

        public bool HasAnyTag(params string[] checkTags)
        {
            foreach (string tag in checkTags)
            {
                if (tags.Contains(tag))
                    return true;
            }

            return false;
        }

        public bool HasAllTags(params string[] checkTags)
        {
            foreach (string tag in checkTags)
            {
                if (!tags.Contains(tag))
                    return false;
            }

            return true;
        }

        #endregion

        #region Item管理方法

        public bool PlaceItem(HexItem newItem)
        {
            if (item != null)
                return false; // 格子已有物品

            item = newItem;
            return true;
        }

        public HexItem RemoveItem()
        {
            HexItem removedItem = item;
            item = null;
            return removedItem;
        }

        #endregion

        public override string ToString()
        {
            return
                $"Cell {coord} - Tags: {string.Join(", ", tags)} - Item: {(HasItem ? item.ItemId : "None")}";
        }
    }
}
#region

//文件创建者：Egg
//创建时间：11-09 01:00

#endregion

using System;
using System.Collections.Generic;
using UnityEngine;

namespace KOJC.Core
{
    [Serializable]
    public class HexItem
    {
        [SerializeField] private string       itemId;
        [SerializeField] private List<string> tags = new();
        
        [SerializeField] private string itemType = "Default";

        public HexItem(string id)
        {
            itemId = id;
        }

        #region 属性访问器

        public string       ItemId   => itemId;
        public List<string> Tags     => tags;
        public string       ItemType => itemType;

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
    }
}
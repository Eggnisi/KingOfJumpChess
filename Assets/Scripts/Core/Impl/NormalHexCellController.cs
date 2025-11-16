#region

//文件创建者：Egg
//创建时间：11-16 08:21

#endregion

using System.Collections.Generic;
using EggFramework.Util;
using UnityEngine;

namespace KOJC.Core.Impl
{
    public sealed class NormalHexCellController : HexCellController
    {
        public List<SpriteRenderer> Backgrounds = new();
        public override void Init(HexCell cell)
        {
            Debug.Log("HexCellController init called");
            //修改颜色
            foreach (var cellTag in cell.Tags)
            {
                
                if (ColorUtil.ColorEnum.Contains(cellTag))
                {
                    SetColor(ColorUtil.ParseColor(cellTag));
                }
            }
        }

        private void SetColor(Color color)
        {
            foreach (var background in Backgrounds)
            {
                background.color = color;
            }
        }
    }
}
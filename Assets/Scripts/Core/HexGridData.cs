#region

//文件创建者：Egg
//创建时间：11-09 01:13

#endregion

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KOJC.Core
{
    [CreateAssetMenu(fileName = "NewHexGridData", menuName = "KOJC/Hex Grid Data")]
    public class HexGridData : ScriptableObject
    {
        [Serializable]
        public class CellData
        {
            [LabelText("坐标")] public HexCoord     coord;
            [LabelText("标签")] public List<string> tags = new();

            // 可以扩展更多属性
            [SerializeField, LabelText("格子类型")] private string     cellType = "Default";
            [SerializeField, LabelText("预制体")]  private GameObject prefab;
            [SerializeField, LabelText("调试颜色")] private Color      debugColor = Color.white;

            public string     CellType   => cellType;

            public Color DebugColor
            {
                get=>debugColor;
                set => debugColor = value;
            } 
            public GameObject Prefab     => prefab;

            public CellData(HexCoord coordinate, GameObject prefab)
            {
                coord       = coordinate;
                this.prefab = prefab;
            }
        }

        [Header("网格配置"), SerializeField, LabelText("网格大小")]
        private float hexSize = 1f;

        [SerializeField, LabelText("网格中心")] private Vector2 gridCenter = Vector2.zero;

        [Header("单元格数据"), SerializeField, LabelText("格子数据")]
        private List<CellData> cells = new();

        [Header("预制格子和物品配置"), SerializeField, LabelText("格子配置")]
        private List<HexCellTemplate> cellTemplates = new List<HexCellTemplate>();

        [SerializeField, LabelText("物品配置")] private List<HexItemTemplate> itemTemplates = new List<HexItemTemplate>();

        #region 属性访问器

        public float                 HexSize       => hexSize;
        public Vector2               GridCenter    => gridCenter;
        public List<CellData>        Cells         => cells;
        public List<HexCellTemplate> CellTemplates => cellTemplates;
        public List<HexItemTemplate> ItemTemplates => itemTemplates;

        #endregion

        #region 数据管理方法

        public void ClearAllData()
        {
            cells.Clear();
        }

        public bool AddCell(CellData cellData)
        {
            if (GetCellData(cellData.coord) != null)
                return false; // 已存在

            cells.Add(cellData);
            return true;
        }

        public bool RemoveCell(HexCoord coord)
        {
            CellData cellToRemove = GetCellData(coord);
            if (cellToRemove != null)
            {
                cells.Remove(cellToRemove);
                return true;
            }

            return false;
        }

        public CellData GetCellData(HexCoord coord)
        {
            return cells.Find(cell => cell.coord == coord);
        }

        public List<CellData> GetCellsInRange(HexCoord center, int radius)
        {
            List<CellData> result = new List<CellData>();

            foreach (CellData cell in cells)
            {
                if (GetDistance(center, cell.coord) <= radius)
                {
                    result.Add(cell);
                }
            }

            return result;
        }

        public List<CellData> FindCellsWithTags(params string[] tags)
        {
            List<CellData> result = new List<CellData>();

            foreach (CellData cell in cells)
            {
                bool hasAllTags = true;
                foreach (string tag in tags)
                {
                    if (!cell.tags.Contains(tag))
                    {
                        hasAllTags = false;
                        break;
                    }
                }

                if (hasAllTags)
                    result.Add(cell);
            }

            return result;
        }

        private int GetDistance(HexCoord a, HexCoord b)
        {
            return (Mathf.Abs(a.q - b.q) + Mathf.Abs(a.r - b.r) + Mathf.Abs(a.s - b.s)) / 2;
        }

        #endregion
    }

    [Serializable]
    public class HexCellTemplate
    {
        [SerializeField, LabelText("模板名")]  private string       templateName = "NewTemplate";
        [SerializeField, LabelText("默认标签")] private List<string> defaultTags  = new List<string>();
        [SerializeField, LabelText("预览颜色")] private Color        previewColor = Color.blue;
        [SerializeField, LabelText("预制体")]  private GameObject   prefab;
        public                                      string       TemplateName => templateName;
        public                                      List<string> DefaultTags  => defaultTags;
        public                                      Color        PreviewColor
        {
            get => previewColor;
            set => previewColor = value;
        } 
        public                                      GameObject   Prefab       => prefab;
    }

    [Serializable]
    public class HexItemTemplate
    {
        [SerializeField, LabelText("模板名")]  private string       templateName = "NewItemTemplate";
        [SerializeField, LabelText("默认标签")] private List<string> defaultTags  = new List<string>();
        [SerializeField, LabelText("物品类型")] private string       itemType     = "Default";

        public string       TemplateName => templateName;
        public List<string> DefaultTags  => defaultTags;
        public string       ItemType     => itemType;
    }
}
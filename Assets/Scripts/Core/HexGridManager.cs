#region

//文件创建者：Egg
//创建时间：11-09 01:06

#endregion

using System;
using System.Collections.Generic;
using EggFramework;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KOJC.Core
{
    public class HexGridManager : MonoBehaviour
    {
        [Header("数据配置"), SerializeField, LabelText("当前数据")]
        private HexGridData currentGridData;

        [Header("运行时数据"), ShowIf("@UnityEngine.Application.isPlaying"), LabelText("网格配置信息"), ShowInInspector,
         HideReferenceObjectPicker]
        private HexGridSystem gridSystem;

        [ShowIf("@UnityEngine.Application.isPlaying"), ShowInInspector, LabelText("网格运行时数据")]
        private Dictionary<HexCoord, HexCell> gridCells = new();

        public event Action<HexGridData>      OnGridDataLoaded;
        public event Action<HexCell>          OnCellCreated;
        public event Action<HexCell>          OnCellRemoved;
        public event Action<HexCell, HexItem> OnItemPlaced;
        public event Action<HexCell, HexItem> OnItemRemoved;

        #region SO数据管理

        private void Awake()
        {
            LoadGridData(currentGridData);
        }

        public void LoadGridData(HexGridData gridData)
        {
            currentGridData = gridData;
            InitializeFromData(gridData);
            OnGridDataLoaded?.Invoke(gridData);
        }

        public void UnloadGridData()
        {
            currentGridData = null;
            gridCells.Clear();
        }

        public HexGridData GetCurrentGridData() => currentGridData;

        public void ClearChild()
        {
            transform.DestroyChildImmediately();
        }
        private void InitializeFromData(HexGridData gridData)
        {
            ClearChild();
            gridSystem = new HexGridSystem(gridData.HexSize, gridData.GridCenter);
            gridCells.Clear();

            // 从SO数据创建格子
            foreach (var cellData in gridData.Cells)
            {
                HexCell cell = CreateCell(cellData.coord, Instantiate(cellData.Prefab, transform));

                cell.Obj.transform.position = GetWorldPosition(cellData.coord);
                // 复制Tag数据
                cell.Tags.Clear();
                foreach (string tag in cellData.tags)
                {
                    cell.AddTag(tag);
                }
            }
        }

        #endregion

        #region Cell管理方法

        public HexCell CreateCell(HexCoord coord, GameObject obj)
        {
            if (gridCells.ContainsKey(coord))
            {
                Debug.LogWarning($"Cell at {coord} already exists");
                return gridCells[coord];
            }

            HexCell newCell = new HexCell(coord, obj);
            gridCells[coord] = newCell;

            OnCellCreated?.Invoke(newCell);
            return newCell;
        }

        public bool RemoveCell(HexCoord coord)
        {
            if (gridCells.TryGetValue(coord, out HexCell cell))
            {
                gridCells.Remove(coord);
                OnCellRemoved?.Invoke(cell);
                return true;
            }

            return false;
        }

        public HexCell GetCell(HexCoord coord)
        {
            gridCells.TryGetValue(coord, out HexCell cell);
            return cell;
        }

        public bool TryGetCell(HexCoord coord, out HexCell cell)
        {
            return gridCells.TryGetValue(coord, out cell);
        }

        public bool CellExists(HexCoord coord)
        {
            return gridCells.ContainsKey(coord);
        }

        public IEnumerable<HexCell> GetAllCells()
        {
            return gridCells.Values;
        }

        public IEnumerable<HexCell> GetCellsInRange(HexCoord center, int radius)
        {
            List<HexCell>  cellsInRange  = new List<HexCell>();
            List<HexCoord> coordsInRange = gridSystem.GetHexesInRange(center, radius);

            foreach (HexCoord coord in coordsInRange)
            {
                if (gridCells.TryGetValue(coord, out HexCell cell))
                {
                    cellsInRange.Add(cell);
                }
            }

            return cellsInRange;
        }

        #endregion

        #region 高级查询方法

        public List<HexCell> FindCellsWithTags(params string[] tags)
        {
            List<HexCell> result = new List<HexCell>();

            foreach (HexCell cell in gridCells.Values)
            {
                if (cell.HasAllTags(tags))
                {
                    result.Add(cell);
                }
            }

            return result;
        }

        public List<HexCell> FindCellsWithAnyTag(params string[] tags)
        {
            List<HexCell> result = new List<HexCell>();

            foreach (HexCell cell in gridCells.Values)
            {
                if (cell.HasAnyTag(tags))
                {
                    result.Add(cell);
                }
            }

            return result;
        }

        public List<HexCell> FindCellsWithItemType(string itemType)
        {
            List<HexCell> result = new List<HexCell>();

            foreach (HexCell cell in gridCells.Values)
            {
                if (cell.HasItem && cell.Item.ItemType == itemType)
                {
                    result.Add(cell);
                }
            }

            return result;
        }

        public HexCell FindNearestCellWithTag(Vector2 worldPos, string tag)
        {
            HexCoord centerCoord     = gridSystem.WorldToHexCoord(worldPos);
            HexCell  nearestCell     = null;
            int      nearestDistance = int.MaxValue;

            foreach (HexCell cell in gridCells.Values)
            {
                if (cell.HasTag(tag))
                {
                    int distance = gridSystem.GetDistance(centerCoord, cell.Coord);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestCell     = cell;
                    }
                }
            }

            return nearestCell;
        }

        #endregion

        #region Item管理方法

        public bool PlaceItemAt(HexCoord coord, HexItem item)
        {
            if (TryGetCell(coord, out HexCell cell))
            {
                if (cell.PlaceItem(item))
                {
                    OnItemPlaced?.Invoke(cell, item);
                    return true;
                }
            }

            return false;
        }

        public HexItem RemoveItemFrom(HexCoord coord)
        {
            if (TryGetCell(coord, out HexCell cell) && cell.HasItem)
            {
                HexItem removedItem = cell.RemoveItem();
                OnItemRemoved?.Invoke(cell, removedItem);
                return removedItem;
            }

            return null;
        }

        public bool MoveItem(HexCoord fromCoord, HexCoord toCoord)
        {
            HexItem item = RemoveItemFrom(fromCoord);
            if (item != null)
            {
                return PlaceItemAt(toCoord, item);
            }

            return false;
        }

        #endregion

        #region 工具方法

        public Vector2 GetWorldPosition(HexCoord coord)
        {
            if (gridSystem == null)
            {
                if (currentGridData)
                    LoadGridData(currentGridData);
                gridSystem ??= new HexGridSystem(1, Vector2.zero);
            }

            return gridSystem!.HexCoordToWorld(coord);
        }

        public HexCoord GetHexCoord(Vector2 worldPos)
        {
            if (gridSystem == null)
            {
                if (currentGridData)
                    LoadGridData(currentGridData);
                gridSystem ??= new HexGridSystem(1, Vector2.zero);
            }

            return gridSystem!.WorldToHexCoord(worldPos);
        }

        public List<HexCell> GetNeighbors(HexCoord coord)
        {
            List<HexCell> neighbors      = new List<HexCell>();
            HexCoord[]    neighborCoords = coord.GetNeighbors();

            foreach (HexCoord neighborCoord in neighborCoords)
            {
                if (TryGetCell(neighborCoord, out HexCell neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        public int GetCellCount()
        {
            return gridCells.Count;
        }

        public void ClearAllItems()
        {
            foreach (HexCell cell in gridCells.Values)
            {
                if (cell.HasItem)
                {
                    RemoveItemFrom(cell.Coord);
                }
            }
        }

        #endregion

        #region 调试方法

        public void DebugLogGridInfo()
        {
            Debug.Log($"=== Hex Grid Info ===");
            Debug.Log($"Total Cells: {gridCells.Count}");

            int cellsWithItems = 0;
            foreach (HexCell cell in gridCells.Values)
            {
                if (cell.HasItem) cellsWithItems++;
            }

            Debug.Log($"Cells with Items: {cellsWithItems}");
            Debug.Log($"Grid Center: {gridSystem.GridCenter}");
            Debug.Log($"Hex Size: {gridSystem.HexSize}");
        }

        public void DebugLogCellInfo(HexCoord coord)
        {
            if (TryGetCell(coord, out HexCell cell))
            {
                Debug.Log(cell.ToString());
            }
            else
            {
                Debug.LogWarning($"No cell found at {coord}");
            }
        }

        #endregion
    }
}
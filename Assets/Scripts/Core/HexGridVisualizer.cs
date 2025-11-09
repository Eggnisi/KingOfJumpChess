#region

//文件创建者：Egg
//创建时间：11-09 11:13

#endregion

using UnityEngine;

namespace KOJC.Core
{
    public class HexGridVisualizer : MonoBehaviour
    {
        [Header("网格设置")] public float   hexSize    = 1f;
        public                  Vector2 gridCenter = Vector2.zero;

        [Header("Gizmos显示设置")] public bool  showInEditMode  = true;
        public                        bool  showInPlayMode  = true;
        public                        int   gridRadius      = 5;
        public                        bool  showCoordinates = true;
        public                        bool  showGridLines   = true;
        public                        Color gridColor       = Color.white;

        private HexGridSystem hexGrid;

        void OnValidate()
        {
            // 当Inspector中的值改变时更新网格系统
            hexGrid = new HexGridSystem(hexSize, gridCenter);
        }

        void OnDrawGizmos()
        {
            if (hexGrid == null)
                hexGrid = new HexGridSystem(hexSize, gridCenter);

            // 设置Gizmos颜色
            Gizmos.color = gridColor;

            // 在编辑模式下显示
            if (showInEditMode && !Application.isPlaying)
            {
                hexGrid.DrawGridGizmos(gridRadius, showCoordinates, showGridLines);
            }

            // 在运行模式下显示
            if (showInPlayMode && Application.isPlaying)
            {
                hexGrid.DrawRuntimeGizmos(gridRadius, showCoordinates, showGridLines);
            }
        }

        void Update()
        {
            // 在运行时实时更新网格中心（跟随GameObject位置）
            if (Application.isPlaying && transform.hasChanged)
            {
                gridCenter           = transform.position;
                hexGrid.GridCenter   = gridCenter;
                transform.hasChanged = false;
            }
        }

        // 公共方法，可以从其他脚本调用
        public Vector2 GetWorldPosition(HexCoord hexCoord)
        {
            return hexGrid.HexCoordToWorld(hexCoord);
        }

        public HexCoord GetHexCoord(Vector2 worldPos)
        {
            return hexGrid.WorldToHexCoord(worldPos);
        }

        public (HexCoord hexCoord, Vector2 relativePos) GetHexWithRelative(Vector2 worldPos)
        {
            return hexGrid.WorldToHexWithRelative(worldPos);
        }
    }
}
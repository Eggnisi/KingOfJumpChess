#region

//文件创建者：Egg
//创建时间：11-09 10:41

#endregion

using System;
using System.Collections.Generic;
using UnityEngine;

namespace KOJC.Core
{
    // 六边形坐标系统（立方坐标q, r, s）
    [Serializable]
    public struct HexCoord
    {
        public          int      q;
        public          int      r;
        public readonly int      s         => -q - r;
        public readonly HexCoord East      => new(q + 1, r);
        public readonly HexCoord SouthEast => new(q + 1, r - 1);
        public readonly HexCoord SouthWest => new(q, r - 1);
        public readonly HexCoord West      => new(q - 1, r);
        public readonly HexCoord NorthWest => new(q - 1, r + 1);
        public readonly HexCoord NorthEast => new(q, r + 1);

        public HexCoord(int q, int r)
        {
            this.q = q;
            this.r = r;
        }

        public static HexCoord operator +(HexCoord a, HexCoord b) => new HexCoord(a.q + b.q, a.r + b.r);
        public static HexCoord operator -(HexCoord a, HexCoord b) => new HexCoord(a.q - b.q, a.r - b.r);
        public static bool operator ==(HexCoord a, HexCoord b) => a.q == b.q && a.r == b.r;
        public static bool operator !=(HexCoord a, HexCoord b) => !(a == b);

        public override bool Equals(object obj)
        {
            if (obj is HexCoord other)
                return this == other;
            return false;
        }

        public override int GetHashCode() => (q, r).GetHashCode();
        public override string ToString() => $"({q}, {r}, {s})";

        public readonly HexCoord[] GetNeighbors() =>
            new[] { East, SouthEast, SouthWest, West, NorthWest, NorthEast };

        // 获取相邻的六边形坐标和方向
        public readonly HexCoordWithDirection[] GetNeighborsWithDirection()
        {
            return new HexCoordWithDirection[]
            {
                new(new HexCoord(q + 1, r), HexDirection.East),          // 右
                new(new HexCoord(q + 1, r - 1), HexDirection.SouthEast), // 右下
                new(new HexCoord(q,     r - 1), HexDirection.SouthWest), // 左下
                new(new HexCoord(q - 1, r), HexDirection.West),          // 左
                new(new HexCoord(q - 1, r + 1), HexDirection.NorthWest), // 左上
                new(new HexCoord(q,     r + 1), HexDirection.NorthEast)  // 右上
            };
        }

        // 计算到另一个六边形的距离
        public readonly int DistanceTo(HexCoord other) =>
            (Mathf.Abs(q - other.q) + Mathf.Abs(r - other.r) + Mathf.Abs(s - other.s)) / 2;
    }

    // 六边形坐标和方向的配对结构
    [Serializable]
    public struct HexCoordWithDirection
    {
        public HexCoord     coord;
        public HexDirection direction;

        public HexCoordWithDirection(HexCoord coord, HexDirection direction)
        {
            this.coord     = coord;
            this.direction = direction;
        }

        public override string ToString() => $"{coord} in {direction}";
    }


    // 六边形方向枚举（基于PointyTop尖顶朝上的方向）
    public enum HexDirection
    {
        Invalid   = -1,
        East      = 0, // 右
        SouthEast = 1, // 右下
        SouthWest = 2, // 左下
        West      = 3, // 左
        NorthWest = 4, // 左上
        NorthEast = 5  // 右上
    }

    [Serializable]
    public class HexGridSystem
    {
        [SerializeField] private float   hexSize    = 1f;
        [SerializeField] private Vector2 gridCenter = Vector2.zero;


        public HexGridSystem(float hexSize = 1f, Vector2 gridCenter = default)
        {
            this.hexSize    = hexSize;
            this.gridCenter = gridCenter;
        }

        #region 公共属性

        public float HexSize
        {
            get => hexSize;
            set => hexSize = Mathf.Max(0.1f, value);
        }

        public Vector2 GridCenter
        {
            get => gridCenter;
            set => gridCenter = value;
        }

        #endregion

        #region 主要转换方法

        /// <summary>
        /// 将世界坐标转换为六边形网格坐标
        /// </summary>
        public HexCoord WorldToHexCoord(Vector2 worldPos)
        {
            Vector2 relativePos = worldPos - gridCenter;
            return WorldToHexCoordPointy(relativePos);
        }

        /// <summary>
        /// 将六边形网格坐标转换为世界坐标（网格中心点）
        /// </summary>
        public Vector2 HexCoordToWorld(HexCoord hexCoord)
        {
            return HexCoordToWorldPointy(hexCoord) + gridCenter;
        }

        /// <summary>
        /// 将世界坐标转换为六边形网格坐标和相对坐标
        /// </summary>
        /// <returns>元组：(网格坐标, 相对于网格中心的坐标)</returns>
        public (HexCoord hexCoord, Vector2 relativePos) WorldToHexWithRelative(Vector2 worldPos)
        {
            HexCoord hexCoord    = WorldToHexCoord(worldPos);
            Vector2  hexCenter   = HexCoordToWorld(hexCoord);
            Vector2  relativePos = worldPos - hexCenter;

            return (hexCoord, relativePos);
        }

        #endregion

        #region 方向相关方法

        /// <summary>
        /// 获取指定方向上的相邻网格坐标
        /// </summary>
        public HexCoord GetNeighborInDirection(HexCoord from, HexDirection direction)
        {
            HexCoord[] neighbors =
            {
                new(from.q + 1, from.r),     // East
                new(from.q + 1, from.r - 1), // SouthEast
                new(from.q, from.r - 1),     // SouthWest
                new(from.q - 1, from.r),     // West
                new(from.q - 1, from.r + 1), // NorthWest
                new(from.q, from.r + 1)      // NorthEast
            };

            return neighbors[(int)direction];
        }

        /// <summary>
        /// 获取两个六边形之间的方向
        /// </summary>
        public HexDirection GetDirectionBetween(HexCoord from, HexCoord to)
        {
            HexCoord diff = to - from;

            for (int i = 0; i < 6; i++)
            {
                HexCoord dirVector = GetDirectionVector((HexDirection)i);
                if (diff.q == dirVector.q && diff.r == dirVector.r)
                {
                    return (HexDirection)i;
                }
            }

            return HexDirection.Invalid; // 不是相邻方向
        }

        /// <summary>
        /// 获取方向向量
        /// </summary>
        private HexCoord GetDirectionVector(HexDirection direction)
        {
            return direction switch
            {
                HexDirection.East      => new HexCoord(1,  0),  // 右
                HexDirection.SouthEast => new HexCoord(1,  -1), // 右下
                HexDirection.SouthWest => new HexCoord(0,  -1), // 左下
                HexDirection.West      => new HexCoord(-1, 0),  // 左
                HexDirection.NorthWest => new HexCoord(-1, 1),  // 左上
                HexDirection.NorthEast => new HexCoord(0,  1),  // 右上
                _                      => new HexCoord(0,  0)
            };
        }

        /// <summary>
        /// 获取相反方向
        /// </summary>
        public HexDirection GetOppositeDirection(HexDirection direction)
        {
            return direction switch
            {
                HexDirection.East      => HexDirection.West,
                HexDirection.SouthEast => HexDirection.NorthWest,
                HexDirection.SouthWest => HexDirection.NorthEast,
                HexDirection.West      => HexDirection.East,
                HexDirection.NorthWest => HexDirection.SouthEast,
                HexDirection.NorthEast => HexDirection.SouthWest,
                _                      => HexDirection.East
            };
        }

        #endregion

        #region 距离和范围方法

        /// <summary>
        /// 计算两个六边形网格之间的距离（步数）
        /// </summary>
        public int GetDistance(HexCoord a, HexCoord b)
        {
            return a.DistanceTo(b);
        }

        /// <summary>
        /// 获取指定半径范围内的所有六边形坐标
        /// </summary>
        public List<HexCoord> GetHexesInRange(HexCoord center, int radius)
        {
            List<HexCoord> results = new List<HexCoord>();

            for (int q = -radius; q <= radius; q++)
            {
                for (int r = -radius; r <= radius; r++)
                {
                    for (int s = -radius; s <= radius; s++)
                    {
                        if (q + r + s == 0)
                        {
                            HexCoord hex = new HexCoord(center.q + q, center.r + r);
                            if (GetDistance(center, hex) <= radius)
                            {
                                results.Add(hex);
                            }
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// 获取两个六边形之间的直线上的所有坐标
        /// </summary>
        public List<HexCoord> GetLine(HexCoord from, HexCoord to)
        {
            List<HexCoord> line     = new List<HexCoord>();
            int            distance = GetDistance(from, to);

            for (int i = 0; i <= distance; i++)
            {
                float    t         = (float)i / distance;
                HexCoord lerpedHex = HexLerp(from, to, t);
                line.Add(lerpedHex);
            }

            return line;
        }

        #endregion

        #region 私有实现方法

        private HexCoord HexLerp(HexCoord a, HexCoord b, float t)
        {
            float q = Mathf.Lerp(a.q, b.q, t);
            float r = Mathf.Lerp(a.r, b.r, t);

            return new HexCoord(Mathf.RoundToInt(q), Mathf.RoundToInt(r));
        }

        private HexCoord WorldToHexCoordPointy(Vector2 relativePos)
        {
            float x = relativePos.x;
            float y = relativePos.y;

            float q = (x * Mathf.Sqrt(3) / 3 - y / 3) / hexSize;
            float r = (y * 2f / 3) / hexSize;

            return new HexCoord(Mathf.RoundToInt(q), Mathf.RoundToInt(r));
        }

        private Vector2 HexCoordToWorldPointy(HexCoord hex)
        {
            float x = hexSize * (Mathf.Sqrt(3) * hex.q + Mathf.Sqrt(3) / 2 * hex.r);
            float y = hexSize * (3f / 2 * hex.r);

            return new Vector2(x, y);
        }

        #endregion

        #region 调试和可视化辅助方法

        /// <summary>
        /// 获取六边形的顶点位置（用于绘制）
        /// </summary>
        public Vector2[] GetHexVertices(HexCoord hexCoord)
        {
            Vector2   center   = HexCoordToWorld(hexCoord);
            Vector2[] vertices = new Vector2[6];

            // PointyTop模式：尖顶朝上，角度从30度开始
            for (int i = 0; i < 6; i++)
            {
                // PointyTop模式：每个顶点间隔60度，从30度开始
                float angle = (30 + 60 * i) * Mathf.Deg2Rad;
                vertices[i] = center + new Vector2(
                    hexSize * Mathf.Cos(angle),
                    hexSize * Mathf.Sin(angle)
                );
            }

            return vertices;
        }

        /// <summary>
        /// 获取六边形的边界矩形（用于碰撞检测等）
        /// </summary>
        public Rect GetHexBounds(HexCoord hexCoord)
        {
            Vector2 center = HexCoordToWorld(hexCoord);

            // PointyTop模式：宽度为√3 * 边长，高度为2 * 边长
            float width  = hexSize * Mathf.Sqrt(3);
            float height = hexSize * 2;

            return new Rect(
                center.x - width / 2,
                center.y - height / 2,
                width,
                height
            );
        }

        #endregion

        #region Gizmos绘制方法

        /// <summary>
        /// 在Scene视图中绘制六边形网格和坐标标签
        /// </summary>
        public void DrawGridGizmos(int gridRadius = 5, bool showCoordinates = true, bool showGridLines = true)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && !UnityEditor.EditorApplication.isPlaying)
            {
                DrawHexGridGizmos(gridRadius, showCoordinates, showGridLines);
            }
#endif
        }

        /// <summary>
        /// 在运行时绘制六边形网格和坐标标签
        /// </summary>
        public void DrawRuntimeGizmos(int gridRadius = 5, bool showCoordinates = true, bool showGridLines = true)
        {
#if UNITY_EDITOR
            DrawHexGridGizmos(gridRadius, showCoordinates, showGridLines);
#endif
        }

#if UNITY_EDITOR
        private void DrawHexGridGizmos(int gridRadius, bool showCoordinates, bool showGridLines)
        {
            // 获取中心点周围的六边形坐标
            HexCoord center       = new HexCoord(0, 0);
            var      hexesInRange = GetHexesInRange(center, gridRadius);

            // 绘制六边形网格线
            if (showGridLines)
            {
                Gizmos.color = Color.white;
                foreach (var hex in hexesInRange)
                {
                    DrawHexGizmos(hex);
                }
            }

            // 绘制坐标标签
            if (showCoordinates)
            {
                foreach (var hex in hexesInRange)
                {
                    DrawHexCoordLabel(hex);
                }
            }
        }

        private void DrawHexGizmos(HexCoord hex)
        {
            Vector2[] vertices = GetHexVertices(hex);

            // 绘制六边形边线
            for (int i = 0; i < 6; i++)
            {
                Vector2 start = vertices[i];
                Vector2 end   = vertices[(i + 1) % 6];
                Gizmos.DrawLine(start, end);
            }
        }

        private void DrawHexCoordLabel(HexCoord hex)
        {
            Vector2 center = HexCoordToWorld(hex);

            // 创建坐标标签文本
            string label = $"({hex.q},{hex.r})";

            // 设置标签样式
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.yellow;
            style.fontSize         = 10;
            style.alignment        = TextAnchor.MiddleCenter;

            // 在Scene视图中绘制标签
            UnityEditor.Handles.Label(center, label, style);
        }
#endif

        #endregion
    }
}
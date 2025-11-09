using System;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

namespace KOJC.Core
{
    [CustomEditor(typeof(HexGridManager))]
    public class HexGridManagerEditor : Editor
    {
        private HexGridManager manager;
        private HexGridData    gridData;

        private enum ToolMode
        {
            Select,
            Paint,
            Erase
        }

        private ToolMode currentMode = ToolMode.Select;

        // 绘制配置
        private HexCellTemplate selectedTemplate;
        private bool            showGrid     = true;
        private bool            showCellInfo = true;

        // 选中状态
        private HexCoord             selectedCoord;
        private HexGridData.CellData selectedCellData;

        // 焦点控制
        private int hexGridControlID;

        private void OnEnable()
        {
            manager          = (HexGridManager)target;
            hexGridControlID = GUIUtility.GetControlID(FocusType.Passive);
        }

        private PropertyTree _objectTree;

        public PropertyTree objectTree
        {
            get
            {
                if (_objectTree == null)
                {
                    try
                    {
                        _objectTree = PropertyTree.Create(serializedObject);
                    }
                    catch (ArgumentException ex)
                    {
                        Debug.Log(ex);
                    }
                }

                return _objectTree;
            }
        }

        public override void OnInspectorGUI()
        {
            DrawOdinInspector();
            EditorGUILayout.Space();

            if (!Application.isPlaying)
            {
                manager.ClearChild();

                DrawToolSettings();
                EditorGUILayout.Space();

                DrawModeSelector();
                EditorGUILayout.Space();

                DrawSelectedCellInfo();
                EditorGUILayout.Space();

                DrawUtilityButtons();
                EditorGUILayout.Space();
            }
        }

        private void DrawOdinInspector()
        {
            objectTree.BeginDraw(true);
            GUIHelper.PushLabelWidth(84);
            objectTree.Draw(true);
            objectTree.EndDraw();
            GUIHelper.PopLabelWidth();
        }

        private void DrawToolSettings()
        {
            GUILayout.Label("编辑器工具设置", EditorStyles.boldLabel);

            // 网格数据显示
            gridData = manager.GetCurrentGridData();
            if (gridData == null)
            {
                EditorGUILayout.HelpBox("未加载任何网格数据SO", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField($"当前数据: {gridData.name}",        EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"格子数量: {gridData.Cells.Count}", EditorStyles.miniLabel);

            showGrid     = EditorGUILayout.Toggle("显示网格",   showGrid);
            showCellInfo = EditorGUILayout.Toggle("显示格子信息", showCellInfo);
        }

        private void DrawModeSelector()
        {
            GUILayout.Label("编辑模式", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Toggle(currentMode == ToolMode.Select, "选中模式", "Button"))
                    currentMode = ToolMode.Select;
                if (GUILayout.Toggle(currentMode == ToolMode.Paint, "绘制模式", "Button"))
                    currentMode = ToolMode.Paint;
                if (GUILayout.Toggle(currentMode == ToolMode.Erase, "擦除模式", "Button"))
                    currentMode = ToolMode.Erase;
            }
            EditorGUILayout.EndHorizontal();

            // 绘制模式下的模板选择
            if (currentMode == ToolMode.Paint)
            {
                EditorGUILayout.Space();
                GUILayout.Label("绘制模板", EditorStyles.miniLabel);

                if (gridData.CellTemplates.Count == 0)
                {
                    EditorGUILayout.HelpBox("没有可用的格子模板", MessageType.Info);
                }
                else
                {
                    int selectedIndex = gridData.CellTemplates.IndexOf(selectedTemplate);
                    int newIndex = EditorGUILayout.Popup("选择模板", selectedIndex,
                        gridData.CellTemplates.ConvertAll(t => t.TemplateName).ToArray());

                    if (newIndex >= 0 && newIndex < gridData.CellTemplates.Count)
                    {
                        selectedTemplate = gridData.CellTemplates[newIndex];
                    }

                    if (selectedTemplate != null)
                    {
                        selectedTemplate.PreviewColor = EditorGUILayout.ColorField("预览颜色", selectedTemplate.PreviewColor);
                        EditorGUILayout.LabelField("默认标签:", string.Join(", ", selectedTemplate.DefaultTags));
                    }
                }
            }
        }

        private void DrawSelectedCellInfo()
        {
            if (currentMode != ToolMode.Select || selectedCellData == null) return;

            GUILayout.Label("选中格子信息", EditorStyles.boldLabel);

            EditorGUILayout.LabelField($"坐标: {selectedCoord}");
            EditorGUILayout.LabelField($"格子类型: {selectedCellData.CellType}");

            // Tag编辑
            EditorGUILayout.LabelField("标签:");
            EditorGUI.indentLevel++;

            for (int i = 0; i < selectedCellData.tags.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                selectedCellData.tags[i] = EditorGUILayout.TextField(selectedCellData.tags[i]);
                if (GUILayout.Button("×", GUILayout.Width(20)))
                {
                    selectedCellData.tags.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("添加标签"))
            {
                selectedCellData.tags.Add("NewTag");
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }

        private void DrawUtilityButtons()
        {
            GUILayout.Label("工具", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("手动保存"))
            {
                EditorUtility.SetDirty(gridData);
                AssetDatabase.SaveAssets();
            }

            if (GUILayout.Button("清除所有格子"))
            {
                if (EditorUtility.DisplayDialog("确认清除", "这将删除所有格子数据，确定要继续吗？", "确定", "取消"))
                {
                    gridData.ClearAllData();
                    AssetDatabase.SaveAssets();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void OnSceneGUI()
        {
            if (gridData == null) return;

            DrawGrid();
            HandleInput();
        }

        private void DrawGrid()
        {
            if (!showGrid) return;

            // 绘制所有存在的格子
            foreach (var cellData in gridData.Cells)
            {
                Vector3 worldPos = GetWorldPosition(cellData.coord);
                DrawHexagon(worldPos, gridData.HexSize, cellData.DebugColor);

                // 显示格子信息
                if (showCellInfo)
                {
                    Handles.Label(worldPos + Vector3.up * 0.2f,
                        $"{cellData.coord}\n{string.Join(",", cellData.tags)}",
                        EditorStyles.miniLabel);
                }
            }

            // 在选中模式下高亮选中格子
            if (currentMode == ToolMode.Select && selectedCellData != null)
            {
                Vector3 worldPos = GetWorldPosition(selectedCoord);
                DrawHexagon(worldPos, gridData.HexSize * 1.1f, Color.yellow);
            }
        }

        private void HandleInput()
        {
            Event e = Event.current;

            // 在布局阶段添加默认控制，确保工具保持焦点
            if (e.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(hexGridControlID);
            }

            // 获取鼠标位置对应的格子坐标
            Ray      ray           = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Vector2  mouseWorldPos = new Vector2(ray.origin.x, ray.origin.y);
            HexCoord mouseCoord    = WorldToHexCoord(mouseWorldPos);

            // 显示鼠标悬停的格子预览
            if (currentMode == ToolMode.Paint && selectedTemplate != null)
            {
                Vector3 previewPos = GetWorldPosition(mouseCoord);
                DrawHexagon(previewPos, gridData.HexSize, selectedTemplate.PreviewColor);
            }

            // 处理鼠标输入
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        switch (currentMode)
                        {
                            case ToolMode.Select:
                                SelectCell(mouseCoord);
                                break;
                            case ToolMode.Paint:
                                PaintCell(mouseCoord);
                                e.Use();
                                break;
                            case ToolMode.Erase:
                                EraseCell(mouseCoord);
                                e.Use();
                                break;
                        }
                    }

                    break;

                case EventType.MouseDrag:
                    if (e.button == 0)
                    {
                        switch (currentMode)
                        {
                            case ToolMode.Paint:
                                PaintCell(mouseCoord);
                                e.Use();
                                break;
                            case ToolMode.Erase:
                                EraseCell(mouseCoord);
                                e.Use();
                                break;
                        }
                    }

                    break;

                case EventType.KeyDown:
                    // 支持快捷键操作
                    if (e.keyCode == KeyCode.LeftControl || e.keyCode == KeyCode.RightControl)
                    {
                        // Ctrl键按下时可以连续操作
                        Repaint();
                    }

                    break;

                case EventType.KeyUp:
                    if (e.keyCode == KeyCode.LeftControl || e.keyCode == KeyCode.RightControl)
                    {
                        Repaint();
                    }

                    break;
            }

            // 显示坐标信息
            if (currentMode != ToolMode.Select || !e.alt) // Alt键可以隐藏坐标信息
            {
                Handles.Label(mouseWorldPos + Vector2.up * 0.5f,
                    $"坐标: {mouseCoord}\n模式: {currentMode}\nCtrl: 连续操作",
                    EditorStyles.whiteLabel);
            }

            // 在绘制和擦除模式下显示操作提示
            if ((currentMode == ToolMode.Paint || currentMode == ToolMode.Erase) && selectedCellData == null)
            {
                string hint = currentMode == ToolMode.Paint ? "点击绘制格子\n拖动连续绘制" : "点击擦除格子\n拖动连续擦除";

                Handles.Label(mouseWorldPos + Vector2.down * 0.8f, hint, EditorStyles.miniLabel);
            }
        }

        private void SelectCell(HexCoord coord)
        {
            selectedCoord    = coord;
            selectedCellData = gridData.GetCellData(coord);

            if (selectedCellData != null)
            {
                Debug.Log($"选中格子: {coord}, 标签: {string.Join(", ", selectedCellData.tags)}");
            }
            else
            {
                Debug.Log($"坐标 {coord} 没有格子");
            }

            Repaint();
        }

        private void PaintCell(HexCoord coord)
        {
            if (selectedTemplate == null)
            {
                Debug.LogWarning("请先选择绘制模板");
                return;
            }

            var existingCell = gridData.GetCellData(coord);
            if (existingCell != null)
            {
                gridData.RemoveCell(coord);
            }

            // 创建新格子
            var newCell = new HexGridData.CellData(coord, selectedTemplate.Prefab);
            newCell.tags.AddRange(selectedTemplate.DefaultTags);
            newCell.DebugColor = selectedTemplate.PreviewColor;
            if (gridData.AddCell(newCell))
            {
                // 如果是选择模式，选中新绘制的格子
                if (currentMode == ToolMode.Select)
                {
                    SelectCell(coord);
                }
            }
        }

        private void EraseCell(HexCoord coord)
        {
            var cellToErase = gridData.GetCellData(coord);
            if (cellToErase == null)
            {
                return;
            }

            if (gridData.RemoveCell(coord))
            {
                // 如果擦除的是选中的格子，清空选中状态
                if (selectedCoord == coord)
                {
                    selectedCoord    = default;
                    selectedCellData = null;
                }
            }
        }


        private Vector3 GetWorldPosition(HexCoord coord)
        {
            return manager.GetWorldPosition(coord);
        }

        private HexCoord WorldToHexCoord(Vector2 worldPos)
        {
            return manager.GetHexCoord(worldPos);
        }

        private void DrawHexagon(Vector3 center, float size, Color color)
        {
            Handles.color = color;
            Vector3[] vertices = new Vector3[6];

            for (int i = 0; i < 6; i++)
            {
                float angle = (30f + 60f * i) * Mathf.Deg2Rad;
                vertices[i] = center + new Vector3(
                    size * Mathf.Cos(angle),
                    size * Mathf.Sin(angle),
                    0
                );
            }

            Handles.DrawPolyLine(vertices[0], vertices[1], vertices[2], vertices[3],
                vertices[4], vertices[5], vertices[0]);
        }
    }
}
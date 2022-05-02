using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    public partial class Ability
    {
#if UNITY_EDITOR
        private bool DrawColoredEnumElement(Rect rect, bool value)
        {
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                value = !value;
                GUI.changed = true;
                Event.current.Use();
            }

            UnityEditor.EditorGUI.DrawRect(rect.Padding(1), value ? new Color(0.1f, 0.8f, 0.2f) : new Color(0, 0, 0, 0.5f));

            return value;
        }
        [OnInspectorInit]
        private void _EditorCreateData()
        {
            if (multiTargetSetupList.Count == 0)
            {
                matrixMultiTargetSetupSize = new Vector2Int(3, 3);
                editorMatrixMultiTargetSetup = new bool[matrixMultiTargetSetupSize.x, matrixMultiTargetSetupSize.y];
                editorMatrixMultiTargetSetup[matrixMultiTargetSetupSize.x / 2, matrixMultiTargetSetupSize.y / 2] = true;
            }
            else
            {
                editorMatrixMultiTargetSetup = new bool[matrixMultiTargetSetupSize.x, matrixMultiTargetSetupSize.y];
                for (int x = 0; x < matrixMultiTargetSetupSize.x; x++)
                    for (int y = 0; y < matrixMultiTargetSetupSize.y; y++)
                    {
                        Vector2Int posRelativeToMiddle = new Vector2Int(x, matrixMultiTargetSetupSize.y - y - 1);
                        posRelativeToMiddle -= _EditorGetCurMiddleIndex();
                        editorMatrixMultiTargetSetup[x, y] = 
                            multiTargetSetupList.Exists(e => e.Equals(posRelativeToMiddle));

                    }
            }
            _EditorOnMatrixMultiTargetSetupValueChanged();
        }
        private void _EditorOnMatrixMultiTargetSetupValueChanged()
        {
            bool CheckTargetSetupInRow(int row)
            {
                for (int column = 0; column < editorMatrixMultiTargetSetup.GetLength(0); column++)
                {
                    if (editorMatrixMultiTargetSetup[column, row])
                        return true;
                }
                return false;
            }
            bool CheckTargetSetupInColumn(int column)
            {
                for (int row = 0; row < editorMatrixMultiTargetSetup.GetLength(1); row++)
                {
                    if (editorMatrixMultiTargetSetup[column, row])
                        return true;
                }
                return false;
            }


            Vector2Int curSize = new Vector2Int(editorMatrixMultiTargetSetup.GetLength(0), editorMatrixMultiTargetSetup.GetLength(1));
            multiTargetSetupList.Clear();
            for (int x = 0; x < curSize.x; x++)
                for (int y = 0; y < curSize.y; y++)
                    if (editorMatrixMultiTargetSetup[x, y])
                        multiTargetSetupList.Add(new Vector2Int(x, curSize.y - y - 1) - _EditorGetCurMiddleIndex());

            Vector2Int middleIndex = _EditorGetCurMiddleIndex();
            editorMatrixMultiTargetSetup[middleIndex.x, middleIndex.y] = true;

            bool needToExtend =
                CheckTargetSetupInColumn(0) ||
                CheckTargetSetupInColumn(curSize.x - 1) ||
                CheckTargetSetupInRow(0) ||
                CheckTargetSetupInRow(curSize.y - 1);
            bool needToShrink =
                !CheckTargetSetupInColumn(1) &&
                !CheckTargetSetupInColumn(curSize.x - 2) &&
                !CheckTargetSetupInRow(1) &&
                !CheckTargetSetupInRow(curSize.y - 2);

            Vector2Int newSize = curSize;
            bool[,] newMatrix = new bool[0, 0];
            if (needToExtend)
            {
                newSize += new Vector2Int(2, 2);
                newMatrix = new bool[newSize.x, newSize.y];

                for (int x = 1; x < newSize.x && x - 1 < curSize.x; x++)
                    for (int y = 1; y < newSize.y && y - 1 < curSize.y; y++)
                        newMatrix[x, y] = editorMatrixMultiTargetSetup[x - 1, y - 1];
            }
            else if (needToShrink)
            {
                newSize -= new Vector2Int(2, 2);
                newMatrix = new bool[newSize.x, newSize.y];

                for (int x = 1; x - 1 < newSize.x && x < curSize.x; x++)
                    for (int y = 1; y - 1 < newSize.y && y < curSize.y; y++)
                        newMatrix[x - 1, y - 1] = editorMatrixMultiTargetSetup[x, y];
            }
            else
                return;
            editorMatrixMultiTargetSetup = newMatrix;
            matrixMultiTargetSetupSize = newSize;
        }
        private Vector2Int _EditorGetCurMiddleIndex()
        {
            Vector2Int result =
                new Vector2Int(editorMatrixMultiTargetSetup.GetLength(0), editorMatrixMultiTargetSetup.GetLength(1)) / 2;
            return result;
        }
#endif
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace TeamAlpha.Source
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    [RequireComponent(typeof(RectTransform))]
    public class SimpleGrid : MonoBehaviour
    {
        public struct ItemPosition
        {
            public int column;
            public int row;
            public int page;
            public int index;
            public Vector3 position;
        }

        public bool centerContentByX;
        public bool centerContentByY;
        [ShowIf("@centerContentByY || centerContentByX")]
        public bool centerByFixedGridSize;
        public bool dynamicSize;
        public float cellScale = 1f;
        public Vector2 cellSize = new Vector2(100f, 100f);
        public Vector2 anchorPoint = new Vector2(0, 1f);
        [MinValue(1)]
        public int columnCount;
        [MinValue(1)]
        public int rowCount;
        public float offsetByCellX;
        public float offsetByCellY;
        public float offsetTop, offsetlLeft;
        public bool multiPaged;
        [ShowIf("multiPaged"), InlineProperty]
        public Vector2 offsetByPage;

        public int ItemsPerPage => columnCount * rowCount;
        public int CurColumnsUsed => Mathf.Clamp(transform.childCount, 0, columnCount);
        public int CurRowsUsed => Mathf.Clamp(transform.childCount / columnCount, 0, rowCount);
        public Vector2 StartPoint
        {
            get
            {
                Vector2 result = Vector2.zero;
                result.x += offsetlLeft * cellScale;
                result.y -= offsetTop * cellScale;

                return result;
            }
        }
        //public Vector2 AnchorPoint
        //{
        //    get
        //    {
        //        Vector2 rectSize = localRect.rect.size;
        //        Vector2 result = new Vector2();
        //        result.x = Mathf.Lerp(-l)
        //        result *= localRect.rect.size / 2f;

        //        return result;
        //    }
        //}
        private bool wasUpdated, prevGOState;
        [SerializeField]
        private RectTransform localRect;

        private void Awake()
        {
            localRect = GetComponent<RectTransform>();
        }
        private void Update()
        {
            wasUpdated = false;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UpdateView();
#endif
        }
        public void UpdateView()
        {
            if (wasUpdated || !gameObject.activeSelf)
                return;

            Vector2 newLocalSize = new Vector2();
            RectTransform rt = null;

            int cellsActive = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                rt = transform.GetChild(i).GetComponent<RectTransform>();
                if (!rt.gameObject.activeInHierarchy)
                {
                    continue;
                }
                ItemPosition newPos = GetPositionInMatrixByIndex(cellsActive);

                rt.anchorMax = anchorPoint;
                rt.anchorMin = anchorPoint;
                rt.anchoredPosition = newPos.position;
                rt.sizeDelta = cellSize;
                rt.localScale = Vector3.one * cellScale;

                if (dynamicSize)
                {
                    newLocalSize.y += (cellsActive % rowCount == 1 ? rt.sizeDelta.y : 0) * cellScale;
                }
                cellsActive++;
            }
            if (dynamicSize)
                localRect.sizeDelta = newLocalSize;
            wasUpdated = true;
        }
        public ItemPosition GetPositionInMatrixByIndex(int index)
        {
            ItemPosition result = new ItemPosition();
            result.row = index / columnCount;
            result.column = index % columnCount;
            result.page = index / ItemsPerPage;
            result.index = GetIndex(result.column, result.row);
            result.position = GetPosition(result.column, result.row);

            return result;
        }
        public Vector3 GetPosition(int column, int row)
        {
            Vector3 result = new Vector3();
            result.x = StartPoint.x + column * offsetByCellX;
            result.y = StartPoint.y - row * offsetByCellY;

            int index = GetIndex(column, row);
            int page = GetPageNumberByIndex(index);
            if (multiPaged)
                result += (Vector3)offsetByPage * page;
            if (centerContentByX)
            {
                Vector2 offset = new Vector2();
                offset.x = localRect.rect.width;
                if (centerByFixedGridSize)
                    offset.x -= columnCount * offsetByCellX;
                else
                    offset.x -= CurColumnsUsed * offsetByCellX;
                offset.x /= 2f;
                result += (Vector3)offset;
            }
            if (centerContentByY)
            {
                Vector2 offset = new Vector2();
                offset.y = localRect.rect.height;
                if (centerByFixedGridSize)
                    offset.y -= rowCount * offsetByCellY;
                else
                    offset.y -= CurRowsUsed * offsetByCellY;
                offset.y /= 2f;
                result += (Vector3)offset;
            }

            return result;
        }
        public int GetPageNumberByIndex(int index)
        {
            return index / ItemsPerPage;
        }
        public int GetIndex(int column, int row)
        {
            return row * columnCount + column;
        }
    }
}

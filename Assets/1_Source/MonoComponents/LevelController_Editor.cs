using Sirenix.OdinInspector;
using Sirenix.Utilities;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    public partial class LevelController
    {
#if UNITY_EDITOR
        [DisableInPrefabAssets, AssetsOnly, ShowInInspector, 
            TableMatrix(SquareCells = true, DrawElementMethod = "DrawMatrixElement"), 
            OnValueChanged("_EditorOnEnemiesMatrixValueChanged")]
        public NPC[,] enemiesMatrix = new NPC[5, 4];

        private static NPC DrawMatrixElement(Rect rect, NPC value)
        {
            return (NPC)SirenixEditorFields.UnityPreviewObjectField(
            rect: rect,
            value: value,
            texture: value?.character.spriteIcon.texture, // We provide a custom preview texture
            type: typeof(NPC));
        }
        private void _EditorOnEnemiesMatrixValueChanged()
        {
            poolSize = enemiesMatrix.GetLength(1);
            enemies = gameObject.GetAllComponents<Character>(true);
            foreach (Character character in enemies)
            {
                DestroyImmediate(character.gameObject);
            }
            enemies.Clear();
            for (int x = 0; x < enemiesMatrix.GetLength(0); x++)
            {
                for (int y = 0; y < enemiesMatrix.GetLength(1); y++)
                {
                    NPC enemyPrefab = enemiesMatrix[x, y];
                    if (enemyPrefab == null)
                        continue;
                    NPC enemyInstance = 
                        (UnityEditor.PrefabUtility.InstantiatePrefab(enemyPrefab.gameObject) as GameObject).GetComponent<NPC>();
                    enemyInstance.transform.SetParent(transform);
                    enemyInstance.gridPositionFromBottom = new Vector2Int(x, y);
                    //Invert indexing by Y
                    enemyInstance.gridPositionFromBottom.y = poolSize - (enemyInstance.gridPositionFromBottom.y + 1);
                    enemies.Add(enemyInstance.character);
                }
            }
        }
        private void _EditorOnSizeBattlefieldValueChanged()
        {
            Vector2Int prevSize = new Vector2Int(enemiesMatrix.GetLength(0), enemiesMatrix.GetLength(1));
            Vector2Int newSize = new Vector2Int(sizeBattlefield.x, poolSize);

            NPC[,] newEnemiesMatrix = new NPC[newSize.x, newSize.y];

            int columnsToWrite = Mathf.Min(enemiesMatrix.GetLength(0), newEnemiesMatrix.GetLength(0));
            int rowsToWrite = Mathf.Min(enemiesMatrix.GetLength(1), newEnemiesMatrix.GetLength(1));
            for (int x = 0; x < columnsToWrite; x++)
            {
                for (int y = 0; y < rowsToWrite; y++)
                {
                    newEnemiesMatrix[x, y] = enemiesMatrix[x, y];
                }
            }
            enemiesMatrix = newEnemiesMatrix;
            _EditorOnEnemiesMatrixValueChanged();
        }
        [OnInspectorInit]
        private void _EditorCreateData()
        {
            SortDropPoolConfigByProbability();

            enemiesMatrix = new NPC[sizeBattlefield.x, poolSize];

            int columns = enemiesMatrix.GetLength(0);
            int rows = enemiesMatrix.GetLength(1);
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    Vector2Int invertedPosition = new Vector2Int(x, poolSize - (y + 1));
                    NPC enemy = enemies
                        .Find(character =>
                        {
                            NPC _enemy = character.GetComponent<NPC>();
                            bool equals = _enemy.gridPositionFromBottom.Equals(invertedPosition);
                            return equals;
                        })?.GetComponent<NPC>();
                    if (enemy == null)
                        continue;
                    enemiesMatrix[x, y] = enemy.character.Origin.GetComponent<NPC>();
                }
            }
        }
        private IEnumerable _EditorItemLootDropDown
        {
            get
            {
                ValueDropdownList<int> types = new ValueDropdownList<int>();
                foreach (ItemTierPool pool in DataGameMain.Default.allItemsTierPools)
                {
                    if (lootDropConfiguration.Exists(cfg => cfg.tier == pool.tier))
                        continue;
                    types.Add(new ValueDropdownItem<int>
                    {
                        Text = pool.tier.ToString(),
                        Value = pool.tier
                    });
                }
                return types;
            }
        }
        private bool _EditorValidateInputDropPoolConfig(int input)
        {
            return DataGameMain.Default.allItemsTierPools.Exists(pool => pool.tier == input);
        }
        private bool _EditorValidateDropPoolConfig()
        {
            if (lootDropConfiguration.Count == 0)
                return true;
            else
            {
                lootDropConfiguration.Last().probability = 1f;
                return false;
            }
        }
#endif
    }
}

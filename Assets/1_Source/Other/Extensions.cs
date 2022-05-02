//using Bolt;
using TeamAlpha.Source;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using Pixeye.Actors;
using DG.Tweening.Core;
using System.Reflection;
//using static TeamAlpha.Source.MonoPathController;

namespace TeamAlpha.Source
{
    public static partial class Functions
    {
        public static string GetFullName(this GameObject go)
        {
            string name = go.name;
            while (go.transform.parent != null)
            {

                go = go.transform.parent.gameObject;
                name = go.name + "/" + name;
            }
            return name;
        }
        public static void RemoveNull<T>(List<T> list) where T : class
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i] == null)
                {
                    list.RemoveAt(i);
                    i--;
                }
        }
        public static GameObject FindObject(this GameObject parent, string name)
        {
            Transform[] trs = parent.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in trs)
            {
                if (t.name == name)
                {
                    return t.gameObject;
                }
            }
            return null;
        }
        public static void SetActiveAllComponents(this GameObject content, bool active)
        {
            List<Behaviour> components = new List<Behaviour>(content.GetComponentsInChildren<Behaviour>(true));
            components.AddRange(content.GetComponents<Behaviour>());
            for (int i = 0; i < components.Count; i++)
                components[i].enabled = active;
        }
        public static void ChangeLayerForAll(this IEnumerable<SpriteRenderer> renders, int increment)
        {
            foreach (SpriteRenderer renderer in renders)
                renderer.sortingOrder += increment;
        }
        public static void ChangeLayerIncludingChildrens(this GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                child.gameObject.layer = layer;
                if (child.childCount > 0)
                    ChangeLayerIncludingChildrens(child.gameObject, layer);
            }
        }
        public static List<T> GetAllComponents<T>(this GameObject obj, bool includeInactive)
        {
            List<T> result = new List<T>();
            result.AddRange(obj.GetComponents<T>());

            for (int i = 0; i < obj.transform.childCount; i++)
                result.AddRange(obj.transform.GetChild(i).gameObject.GetAllComponents<T>(includeInactive));
            if (!includeInactive)
                result.RemoveAll(r => !(r as UnityEngine.Component).gameObject.activeInHierarchy);

            return result;
        }
        public static T FindNearest<T>(this Transform obj, List<T> objects, float maxSearchDistance = float.MaxValue, params T[] exclude) where T : MonoBehaviour
        {
            return FindNearestFromPoint(obj.position, objects, maxSearchDistance, exclude).GetComponent<T>();
        }
        public static T FindNearestFromPoint<T>(this Vector3 pos, List<T> objects, float maxSearchDistance = float.MaxValue, params T[] exclude) where T : MonoBehaviour
        {
            List<Transform> listExclude = new List<Transform>(exclude.Length);
            List<Transform> _objects = new List<Transform>(objects.Count);
            foreach (MonoBehaviour mb in exclude)
                listExclude.Add(mb.transform);
            foreach (MonoBehaviour mb in objects)
                _objects.Add(mb.transform);


            return FindNearestFromPoint(pos, _objects, maxSearchDistance, listExclude.ToArray())?.GetComponent<T>();
        }
        public static Transform FindNearest(this Transform obj, List<Transform> objects, float maxSearchDistance = float.MaxValue, params Transform[] exclude)
        {
            List<Transform> listExclude = new List<Transform>(exclude);
            listExclude.Add(obj);
            return FindNearestFromPoint(obj.position, objects, maxSearchDistance, exclude);
        }
        public static Transform FindNearestFromPoint(this Vector2 pointFrom, List<Transform> objects, float maxSearchDistance = float.MaxValue, params Transform[] exclude)
        {
            return FindNearestFromPoint((Vector3)pointFrom, objects, maxSearchDistance, exclude);
        }
        public static Transform FindNearestFromPoint(this Vector3 pointFrom, List<Transform> objects, float maxSearchDistance = float.MaxValue, params Transform[] exclude)
        {
            List<Transform> listExclude = new List<Transform>(exclude);
            Transform nearestObject = null;
            for (int i = 0; i < objects.Count; i++)
            {
                if (listExclude.Contains(objects[i]))
                    continue;
                if (nearestObject == null)
                    nearestObject = objects[i];
                else if
                    ((Vector3.Distance(pointFrom, objects[i].position) <
                    Vector3.Distance(pointFrom, nearestObject.position)))
                    nearestObject = objects[i];
            }
            if (nearestObject != null &&
                Vector3.Distance(nearestObject.position, pointFrom) > maxSearchDistance)
                return null;
            return nearestObject;
        }
        public static bool IsObjectVisible(this UnityEngine.Camera camera, Renderer renderer)
        {
            return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(camera), renderer.bounds);
        }
        public static List<T> GetAllPrefabsWithComponent<T>(string path) where T : MonoBehaviour
        {
            List<GameObject> temp = GetAllPrefabs(path);
            List<T> result = new List<T>();
#if UNITY_EDITOR
            for (int i = 0; i < temp.Count; i++)
                if (temp[i].GetComponent<T>() != null)
                    result.Add(temp[i].GetComponent<T>());
#endif
            return result;
        }
        public static List<GameObject> GetAllPrefabs(string path)
        {
            List<GameObject> result = new List<GameObject>();
#if UNITY_EDITOR
            if (!path.Contains(Application.dataPath))
                path = path.Insert(0, Application.dataPath + "/");
            if (path[path.Length - 1] != '/')
                path += '/';
            string[] aFilePaths = Directory.GetFiles(path);

            string[] directoriesInPath = Directory.GetDirectories(path);

            for (int i = 0; i < directoriesInPath.Length; i++)
            {
                Debug.Log("Search in: " + directoriesInPath[i]);
                result.AddRange(GetAllPrefabs(directoriesInPath[i]));
            }
            foreach (string sFilePath in aFilePaths)
            {
                if (!sFilePath.Contains(".prefab") || sFilePath.Contains(".meta"))
                    continue;
                string sAssetPath = sFilePath.Substring(Application.dataPath.Length - 6);

                GameObject objAsset = UnityEditor.AssetDatabase.LoadAssetAtPath(sAssetPath, typeof(GameObject)) as GameObject;

                if (objAsset != null)
                    result.Add(objAsset);
            }
#endif
            return result;
        }
        public static void AddState(this Dictionary<int, StateDefault> statesMap,
            int stateId, Action OnStart = default, Action<int> OnEnd = default)
        {
            StateDefault state = new StateDefault();
            state.OnStart = OnStart;
            state.OnEnd = OnEnd;
            statesMap.Add(stateId, state);
        }
        public static DG.Tweening.Tween DOVolume(this AudioSource audioSource, float endValue, float duration)
        {
            return DG.Tweening.DOTween.To(
                () => audioSource.volume,
                (float value) => audioSource.volume = value,
                endValue, duration);
        }
        public static string AsNumber(this int number)
        {
            string result = number.ToString();
            if (number == 0)
                result += "st";
            else if (number == 1)
                result += "nd";
            else if (number == 2)
                result += "rd";
            else if (number >= 3)
                result += "th";
            return result;
        }
        public static void Log(this object source, string message)
        {
            ProcessorDebug.Log(source, message);
        }
        public static void LogWarning(this object source, string message)
        {
            ProcessorDebug.LogWarning(source, message);
        }
        public static void LogError(this object source, string message)
        {
            ProcessorDebug.LogError(source, message);
        }
        public static void DestroyAllChilds(this Transform transform)
        {
            while (transform.childCount != 0)
            {
                GameObject child = transform.GetChild(0).gameObject;
                child.transform.SetParent(null);
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(child);
                else
                    UnityEngine.Object.DestroyImmediate(child);
            }
        }
        public static void InitializeAllChilds(this Transform transform, Layer layer)
        {
            List<IRequireActorsLayer> requireToInitialize = new List<IRequireActorsLayer>();
            requireToInitialize.AddRange(transform.gameObject.GetAllComponents<IRequireActorsLayer>(true));
            foreach (IRequireActorsLayer init in requireToInitialize)
                init.Bootstrap(layer);
        }
        public static void LookAtByY(this Transform transform, Vector3 lookAt)
        {
            transform.LookAt(lookAt);
            transform.localEulerAngles += Vector3.right * 90f;
        }
        public static void RegisterTweener(this Tweener tweener)
        {
            LevelController.Current.RegisterTweener(tweener);
        }
        public static ParticleSystem InstantiateParticlesOverUI(this ParticleSystem prefabParticles)
        {
            ParticleSystem result =
                GameObject.Instantiate(prefabParticles.gameObject, LevelController.Current.dynamicHolder)
                    .GetComponent<ParticleSystem>();
            SetParticlesOverUI(result);
            return result;
        }
        public static void SetParticlesOverUI(this ParticleSystem particles, int layerZ = 5)
        {
            particles.gameObject.ChangeLayerIncludingChildrens(DataGameMain.LayerUIParticles);
            particles.gameObject.SetUILevelZ(layerZ);
        }
        public static void SetUILevelZ(this GameObject go, int layerZ)
        {
            go.transform.position =
                new Vector3(
                    go.transform.position.x,
                    go.transform.position.y,
                    CameraManager.Default.transform.position.z + UIManager.Default.mainCanvas.planeDistance - layerZ);
        }
        public static char GetPositiveOrNegativeSign(this float value, bool inverse = false)
        {
            if ((value > 0 && !inverse) || (value < 0 && inverse))
                return '+';
            else
                return '-';
        }
        public static List<FieldInfoContainer> GetFields(this object obj, Func<FieldInfoContainer, bool> filter)
        {
            List<FieldInfoContainer> result = GetFields(obj);
            for (int i = 0; i < result.Count; i++)
                if (filter(result[i]))
                {
                    result.RemoveAt(i);
                    i--;
                }
            return result;
        }
        public static List<FieldInfoContainer> GetFields(this object obj)
        {
            List<FieldInfoContainer> result = new List<FieldInfoContainer>();
            List<FieldInfo> fields = new List<FieldInfo>(obj.GetType().GetFields());

            foreach (FieldInfo field in fields)
                result.Add(new FieldInfoContainer
                {
                    owner = obj,
                    fieldInfo = field
                });
            return result;
        }
        public static Vector3 ScreenToWorldPoint(this Vector3 input)
        {
            return CameraManager.Default.cam.ScreenToWorldPoint(input);
        }
        public static Vector3 WorldToScreenPoint(this Vector3 input)
        {
            return CameraManager.Default.cam.WorldToScreenPoint(input);
        }
#if UNITY_EDITOR
        public static string GetAssetPath(this UnityEngine.Object asset)
        {
            GameObject go = null;
            if (asset is GameObject)
                go = asset as GameObject;
            else if (asset is Transform)
                go = (asset as Transform).gameObject;
            else if (asset is MonoBehaviour)
                go = (asset as MonoBehaviour).gameObject;

            //UnityEditor.PrefabInstanceStatus prefabInstanceStatus = 
            //    UnityEditor.PrefabUtility.GetPrefabInstanceStatus(go);
            //bool isPrefabInstance =
            //    prefabInstanceStatus == 
            //    UnityEditor.PrefabInstanceStatus.Connected;

            string assetPath = string.Empty;

            UnityEngine.Object assetToGet = null;
            if (asset is ScriptableObject)
                assetToGet = asset;
            else if (go != null)
                assetToGet = go;
            if (asset is ScriptableObject || (go != null && go.transform.parent == null))
                assetPath = UnityEditor.AssetDatabase.GetAssetPath(assetToGet);
            if (assetPath == string.Empty || assetPath == null)
            {

                if (go != null)
                {
                    try
                    {
                        assetPath =
                            UnityEditor.AssetDatabase.GetAssetPath(
                                UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(
                                    UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(go)));
                    }
                    catch
                    {

                    }

                    if (assetPath == string.Empty || assetPath == null)
                    {
                        assetPath =
                        UnityEditor.Experimental.SceneManagement.
                        PrefabStageUtility.GetPrefabStage(go)?.assetPath;
                    }
                }
            }
            return assetPath;
        }
#endif
    }
}

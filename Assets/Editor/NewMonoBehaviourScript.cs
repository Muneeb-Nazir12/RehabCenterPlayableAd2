using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UI;

public class MissingFieldChecker : EditorWindow
{
    private Vector2 _scrollPos;

    private List<string> _missingResults = new List<string>();
    private List<string> _colliderResults = new List<string>();

    private bool _hasRun = false;
    private bool _showMissing = true;
    private bool _showColliders = true;

    [MenuItem("Tools/Check Missing Inspector Fields")]
    public static void ShowWindow()
    {
        GetWindow<MissingFieldChecker>("Missing Fields Checker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Missing Inspector Field Checker", EditorStyles.boldLabel);
        GUILayout.Space(5);

        if (GUILayout.Button("Scan All GameObjects in Scene", GUILayout.Height(35)))
        {
            RunCheck();
        }

        GUILayout.Space(5);

        if (!_hasRun) return;

        // ---------- Missing Fields ----------
        _showMissing = EditorGUILayout.Foldout(_showMissing,
            $"Missing Fields ({_missingResults.Count})", true, EditorStyles.foldoutHeader);

        if (_showMissing)
        {
            if (_missingResults.Count == 0)
            {
                EditorGUILayout.HelpBox("All SerializeField references are assigned!", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(_missingResults.Count + " missing field(s) found!", MessageType.Warning);
                GUILayout.Space(3);

                _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.MaxHeight(300));
                foreach (string result in _missingResults)
                {
                    GUILayout.BeginVertical("box");
                    GUILayout.Label(result, EditorStyles.wordWrappedLabel);
                    GUILayout.EndVertical();
                    GUILayout.Space(2);
                }
                GUILayout.EndScrollView();
            }
        }

        GUILayout.Space(8);

        // ---------- Collider Scale ----------
        _showColliders = EditorGUILayout.Foldout(_showColliders,
            $"Scaled Colliders ({_colliderResults.Count})", true, EditorStyles.foldoutHeader);

        if (_showColliders)
        {
            if (_colliderResults.Count == 0)
            {
                EditorGUILayout.HelpBox("No non-uniform scaled colliders found!", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(_colliderResults.Count + " collider(s) with non-uniform scale!", MessageType.Warning);
                GUILayout.Space(3);

                foreach (string result in _colliderResults)
                {
                    GUILayout.BeginVertical("box");
                    GUILayout.Label(result, EditorStyles.wordWrappedLabel);
                    GUILayout.EndVertical();
                    GUILayout.Space(2);
                }
            }
        }
    }

    private void RunCheck()
    {
        _missingResults.Clear();
        _colliderResults.Clear();
        _hasRun = true;

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject go in allObjects)
        {
            // Skip prefabs not placed in the scene
            if (go.scene.name == null ||
                go.hideFlags == HideFlags.NotEditable ||
                go.hideFlags == HideFlags.HideAndDontSave)
                continue;

            // ---- Missing Field Check ----
            MonoBehaviour[] components = go.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour mb in components)
            {
                if (mb == null) continue;

                System.Type type = mb.GetType();

                FieldInfo[] fields = type.GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

                foreach (FieldInfo field in fields)
                {
                    bool isSerializedPublic = field.IsPublic &&
                                              !field.IsDefined(typeof(System.NonSerializedAttribute), false);
                    bool isSerializedPrivate = field.IsDefined(typeof(SerializeField), false);

                    if (!isSerializedPublic && !isSerializedPrivate) continue;

                    // Skip value types, strings
                    if (field.FieldType.IsValueType) continue;
                    if (field.FieldType == typeof(string)) continue;

                    // Skip visual/asset fields — commonly left empty intentionally
                    if (field.FieldType == typeof(Image)) continue;
                    if (field.FieldType == typeof(Sprite)) continue;
                    if (field.FieldType == typeof(Texture)) continue;
                    if (field.FieldType == typeof(Texture2D)) continue;
                    if (field.FieldType == typeof(Material)) continue;

                    object value = field.GetValue(mb);
                    bool isMissing = false;

                    if (value == null)
                    {
                        isMissing = true;
                    }
                    else if (value is Object unityObj)
                    {
                        if (unityObj == null) isMissing = true;
                    }

                    if (isMissing)
                    {
                        _missingResults.Add(
                            $"GameObject: '{go.name}'\n" +
                            $"Script:     {type.Name}\n" +
                            $"Field:      {field.Name}  [{field.FieldType.Name}]"
                        );
                    }
                }
            }

            // ---- Collider Scale Check ----
            // A collider behaves unexpectedly when its GameObject has non-uniform lossy scale
            Collider[] colliders = go.GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                Vector3 s = go.transform.lossyScale;

                bool nonUniform = !Mathf.Approximately(s.x, s.y) ||
                                  !Mathf.Approximately(s.y, s.z);

                bool hasScale = !Mathf.Approximately(s.x, 1f) ||
                                  !Mathf.Approximately(s.y, 1f) ||
                                  !Mathf.Approximately(s.z, 1f);

                if (nonUniform || hasScale)
                {
                    string issue = nonUniform ? "Non-Uniform Scale" : "Scaled (non-1)";

                    _colliderResults.Add(
                        $"GameObject: '{go.name}'\n" +
                        $"Collider:   {col.GetType().Name}\n" +
                        $"Issue:      {issue}\n" +
                        $"LossyScale: ({s.x:F3}, {s.y:F3}, {s.z:F3})"
                    );
                }
            }
        }

        _missingResults.Sort();
        _colliderResults.Sort();

        Debug.Log($"[MissingFieldChecker] Scan done — {_missingResults.Count} missing field(s), {_colliderResults.Count} scaled collider(s).");
    }
}
#endif
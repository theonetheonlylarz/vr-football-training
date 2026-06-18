#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using FootballTraining.Core;
using FootballTraining.Data;
using FootballTraining.Formations;

namespace FootballTraining.Editor
{
    /// <summary>
    /// Scene-view gizmo tool that draws a selected FormationConfig in the editor
    /// so you can verify player positions without entering Play mode.
    /// Open via Football Training → Formation Visualizer.
    /// </summary>
    public class FormationVisualizerWindow : EditorWindow
    {
        private FormationConfig _formation;
        private FormationType _builtInType = FormationType.IFormation;
        private float _scale = 1f;
        private Vector3 _origin = Vector3.zero;
        private bool _showLabels = true;

        [MenuItem("Football Training/Formation Visualizer")]
        public static void Open() => GetWindow<FormationVisualizerWindow>("Formation Visualizer");

        private void OnEnable()  => SceneView.duringSceneGui += DrawFormationGizmos;
        private void OnDisable() => SceneView.duringSceneGui -= DrawFormationGizmos;

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Formation Visualizer", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _formation = (FormationConfig)EditorGUILayout.ObjectField(
                "Formation Asset", _formation, typeof(FormationConfig), false);

            EditorGUILayout.LabelField("— or use built-in —", EditorStyles.miniLabel);
            _builtInType = (FormationType)EditorGUILayout.EnumPopup("Built-in Formation", _builtInType);

            if (GUILayout.Button("Load Built-in"))
            {
                var all = FormationLibrary.BuildAll();
                if (all.TryGetValue(_builtInType, out var cfg)) _formation = cfg;
            }

            EditorGUILayout.Space();
            _origin    = EditorGUILayout.Vector3Field("Origin", _origin);
            _scale     = EditorGUILayout.Slider("Scale", _scale, 0.5f, 2f);
            _showLabels = EditorGUILayout.Toggle("Show Labels", _showLabels);

            EditorGUILayout.Space();
            if (_formation != null)
            {
                EditorGUILayout.LabelField($"Formation: {_formation.FormationType}");
                EditorGUILayout.LabelField($"Players:   {_formation.Positions.Count}");
            }

            SceneView.RepaintAll();
        }

        private void DrawFormationGizmos(SceneView sv)
        {
            if (_formation == null) return;

            Handles.color = Color.yellow;
            Handles.DrawLine(_origin + Vector3.left * 30f, _origin + Vector3.right * 30f);

            foreach (var pos in _formation.Positions)
            {
                Vector3 world = _origin + pos.OffsetFromBall * _scale;

                bool isOL = pos.Role is PlayerRole.Center or PlayerRole.LeftGuard or PlayerRole.RightGuard
                                      or PlayerRole.LeftTackle or PlayerRole.RightTackle or PlayerRole.TightEnd;
                bool isQB = pos.Role == PlayerRole.Quarterback;

                Handles.color = isQB ? Color.cyan : isOL ? Color.white : Color.green;
                Handles.DrawSolidDisc(world, Vector3.up, 0.3f * _scale);

                if (_showLabels)
                    Handles.Label(world + Vector3.up * 0.4f * _scale, pos.Role.ToString(), EditorStyles.miniLabel);
            }

            // Draw LOS label
            Handles.color = Color.yellow;
            Handles.Label(_origin + Vector3.up * 0.1f + Vector3.right * 15f, "LOS");
        }
    }
}
#endif

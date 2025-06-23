using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputSystemExtra
{
    [CustomEditor(typeof(CommonInputManager))]
    public class CommonInputManagerEditor : Editor
    {
        private CommonInputManager _target;

        private void OnEnable()
        {
            _target = CommonInputManager.Instance;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if(Application.isPlaying == false) return;
            using (new GUILayout.VerticalScope("Box"))
            {
                foreach (var wrapper in _target.InputActionMaps.Values)
                {
                    ShowMapInfo(wrapper);
                }
            }
        }

        private void ShowMapInfo(IActionMapWrapper map)
        {
            using (new GUILayout.HorizontalScope("Box"))
            {
                GUILayout.Label($"Map: {map.mapName}");
                if (map.mapEnabled)
                {
                    if (GUILayout.Button("Disable Map", GUILayout.Width(100)))
                    {
                        map.mapEnabled = !map.mapEnabled;
                    }
                }
                else
                {
                    if (GUILayout.Button("Enable Map", GUILayout.Width(100)))
                    {
                        map.mapEnabled =!map.mapEnabled;
                    }
                }
            }
        }
    }
}
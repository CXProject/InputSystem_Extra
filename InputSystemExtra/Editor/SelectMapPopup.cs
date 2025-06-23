using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputSystemExtra
{
    public class SelectMapPopup : EditorWindow
    {
        private string[] _mapNames;
        private int _index;
        private bool _isClosed;
        
        public static SelectMapPopup ShowWindow(InputActionAsset asset)
        {
            if(asset == null) return null;
            if(asset.actionMaps.Count == 0) return null;
            var window = GetWindow<SelectMapPopup>();
            window.titleContent = new GUIContent("Select Map");
            window.Initialize(asset);
            window.Show();
            return window;
        }

        private void Initialize(InputActionAsset asset)
        {
            _mapNames = new string[asset.actionMaps.Count];
            _index = 0;
            for (int i = 0; i < _mapNames.Length; i++)
            {
                _mapNames[i] = asset.actionMaps[i].name;
            }
            _isClosed = false;
        }

        private void OnGUI()
        {
            _index = EditorGUILayout.Popup(_index, _mapNames);
            if (GUILayout.Button("Select"))
            {
                _isClosed = true;
                Close();
            }
        }

        public async Task<string> WaitWindowClose()
        {
            while (_isClosed == false)
            {
                await Task.Yield();
            }
            return _mapNames[_index];
        }
    }
}
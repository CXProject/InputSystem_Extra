using System.Collections.Generic;
using LCCommonTools.DebugUtility;
using LCCommonTools.ScriptUtility;
using UnityEngine.InputSystem;

namespace InputSystemExtra
{
    public interface IActionMapWrapper
    {
        string mapName { get; }
        void InitializeMap(InputActionMap map);
        bool mapEnabled { get; set; }
        bool enabled { get; }
    }

    /// <summary>
    ///  对InputSystem的封装。
    /// </summary>
    public class CommonInputManager : SingletonBehaviour<CommonInputManager>
    {
        public InputActionAsset inputActionAsset;

        internal Dictionary<string, IActionMapWrapper> InputActionMaps;

        protected override void Initialize()
        {
            if (inputActionAsset == null)
            {
                LogUtility.LogError("InputActionAsset is null.");
                return;
            }
            var maps = inputActionAsset.actionMaps;
            var inputActionMaps = new Dictionary<string, InputActionMap>(maps.Count);
            foreach (var map in maps)
            {
                inputActionMaps.Add(map.name, map);
            }

            this.InputActionMaps = new Dictionary<string, IActionMapWrapper>(maps.Count);
            var wrappers = GetComponents<IActionMapWrapper>();
            foreach (var wrapper in wrappers)
            {
                if(wrapper.enabled == false) continue;
                if (inputActionMaps.TryGetValue(wrapper.mapName, out var map))
                {
                    wrapper.InitializeMap(map);
                    this.InputActionMaps.Add(wrapper.mapName, wrapper);
                }
                else
                {
                    LogUtility.LogWarning("Cannot find input action map: " + wrapper.mapName);
                }

            }
        }

        public bool TryGetActionMap<T>(string mapName, out T wrapper) where T : class, IActionMapWrapper 
        {
            wrapper = null;
            if (InputActionMaps.TryGetValue(mapName, out var w) == false)
            {
                LogUtility.LogWarning("Cannot find input action map: " + mapName);
            }
            wrapper = w as T;
            return wrapper != null;
        }

        public void SetAllMapsEnableState(bool enable)
        {
            foreach (var wrapper in InputActionMaps.Values)
            {
                wrapper.mapEnabled = enable;
            }
        }

        public void DisableAnotherMap(string mapName)
        {
            foreach (var wrapper in InputActionMaps.Values)
            {
                wrapper.mapEnabled = mapName == wrapper.mapName;
            }
        }
    }
}



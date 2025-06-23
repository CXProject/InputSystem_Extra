//---------------------------------------------------------------------------------------
// Copyright (c) LAPCAT STUDIOS - 2025
// Desc: Wrapper for SettingActionMap.
// GenTime: 2025/6/23
//  Please add this component above the CommonInputManager.
//  PLEASE DON'T EDIT IT!!!
//---------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;

namespace InputSystemExtra
{
	/// <summary>
	/// SimpleControls_Setting_Wrapper
	/// </summary>
	public class SimpleControls_Setting_Wrapper : MonoBehaviour , IActionMapWrapper
	{
		public static string MAP_NAME = "Setting";
		public string mapName => MAP_NAME;
		public bool enabledOnStart = true;
		/// <summary>
		/// Button type : Button
		/// </summary>
		public InputAction ESCAction { get; private set; }
		/// <summary>
		/// Button type : Button
		/// </summary>
		public InputAction OpenMapAction { get; private set; }
		private InputActionMap _map;

		public bool mapEnabled
		{
			get => _map.enabled;
			set {
			if (value)
			{
				_map.Enable();
			}
			else
			{
				_map.Disable();
			}
			}
		}

		public void InitializeMap(InputActionMap map)
		{
			_map = map;
			ESCAction = _map.FindAction("ESC");
			OpenMapAction = _map.FindAction("OpenMap");
			if (enabledOnStart)
			{
				_map.Enable();
			}
			else
			{
				_map.Disable();
			}
		}
	}
}

//---------------------------------------------------------------------------------------
// Copyright (c) LAPCAT STUDIOS - 2025
// Desc: Wrapper for gameplayActionMap.
// GenTime: 2025/6/23
//  Please add this component above the CommonInputManager.
//  PLEASE DON'T EDIT IT!!!
//---------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;

namespace InputSystemExtra
{
	/// <summary>
	/// SimpleControls_gameplay_Wrapper
	/// </summary>
	public class SimpleControls_gameplay_Wrapper : MonoBehaviour , IActionMapWrapper
	{
		public static string MAP_NAME = "gameplay";
		public string mapName => MAP_NAME;
		public bool enabledOnStart = true;
		/// <summary>
		/// Button type : Button
		/// </summary>
		public InputAction fireAction { get; private set; }
		/// <summary>
		/// Value type : Vector2
		/// </summary>
		public InputAction moveAction { get; private set; }
		/// <summary>
		/// PassThrough type : Vector2
		/// </summary>
		public InputAction lookAction { get; private set; }
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
			fireAction = _map.FindAction("fire");
			moveAction = _map.FindAction("move");
			lookAction = _map.FindAction("look");
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

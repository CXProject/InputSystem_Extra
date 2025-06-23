//---------------------------------------------------------------------------------------
// Copyright (c) LAPCAT STUDIOS - 2025
// Desc: Common Input Template for SimpleControls/gameplay.
// GenTime: 2025/6/23
//  Please used with the SimpleControls_gameplay_Wrapper.cs file.
//---------------------------------------------------------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace InputSystemExtra
{
	/// <summary>
	/// Gameplay_Template
	/// </summary>
	public class Gameplay_Template : MonoBehaviour
	{
		public float moveSpeed;
		public float rotateSpeed;
		public float burstSpeed;
		public GameObject projectile;

		private bool m_Charging;
		private Vector2 m_Rotation;
		private Vector2 m_Move;

		private void Start()
		{
			if (CommonInputManager.Instance.TryGetActionMap<SimpleControls_gameplay_Wrapper>(SimpleControls_gameplay_Wrapper.MAP_NAME, out var wrapper))
			{
				// wrapper.fireAction.started += OnFireStart;
				// wrapper.fireAction.canceled += OnFireCancel;
				// wrapper.moveAction.started += OnMoveStart;
				// wrapper.lookAction.started += OnLookStart;
				// wrapper.lookAction.canceled += OnLookCancel;
				wrapper.fireAction.performed += OnFire;
				wrapper.moveAction.performed += OnMove;
				wrapper.moveAction.canceled += OnMoveCancel;
				wrapper.lookAction.performed += OnLook;
			}
		}

		// private void OnFireStart(InputAction.CallbackContext context)
		// {
		// 	Debug.Log($"fire is {context.phase}");
		// }

		private void OnFire(InputAction.CallbackContext context)
		{
			if (context.interaction is SlowTapInteraction)
			{
				StartCoroutine(BurstFire((int)(context.duration * burstSpeed)));
			}
			else
			{
				Fire();
			}
		}

		// private void OnFireCancel(InputAction.CallbackContext context)
		// {
		// 	Debug.Log($"fire is {context.phase}");
		// }
		//
		// private void OnMoveStart(InputAction.CallbackContext context)
		// {
		// 	Debug.Log($"move is {context.phase}");
		// }

		private void OnMove(InputAction.CallbackContext context)
		{
			m_Move = context.ReadValue<Vector2>();
		}

		private void OnMoveCancel(InputAction.CallbackContext context)
		{
			m_Move = Vector2.zero;
		}

		// private void OnLookStart(InputAction.CallbackContext context)
		// {
		// 	Debug.Log($"look is {context.phase}");
		// }

		private void OnLook(InputAction.CallbackContext context)
		{
			Look(context.ReadValue<Vector2>());
		}

		// private void OnLookCancel(InputAction.CallbackContext context)
		// {
		// 	Debug.Log($"look is {context.phase}");
		// }

		private void OnDestroy()
		{
			if(CommonInputManager.IsExist == false ) return;
			if (CommonInputManager.Instance.TryGetActionMap<SimpleControls_gameplay_Wrapper>(SimpleControls_gameplay_Wrapper.MAP_NAME, out var wrapper))
			{
				// wrapper.fireAction.started -= OnFireStart;
				// wrapper.fireAction.canceled -= OnFireCancel;
				// wrapper.moveAction.started -= OnMoveStart;
				// wrapper.lookAction.started -= OnLookStart;
				// wrapper.lookAction.canceled -= OnLookCancel;
				wrapper.fireAction.performed -= OnFire;
				wrapper.moveAction.performed -= OnMove;
				wrapper.moveAction.canceled -= OnMoveCancel;
				wrapper.lookAction.performed -= OnLook;
			}
		}

		private void Update()
		{
			Move(m_Move);
		}

		private void Move(Vector2 direction)
		{
			if (direction.sqrMagnitude < 0.01)
				return;
			var scaledMoveSpeed = moveSpeed * Time.deltaTime;
			// For simplicity's sake, we just keep movement in a single plane here. Rotate
			// direction according to world Y rotation of player.
			var move = Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(direction.x, 0, direction.y);
			transform.position += move * scaledMoveSpeed;
		}

		private void Look(Vector2 rotate)
		{
			if (rotate.sqrMagnitude < 0.01)
				return;
			var scaledRotateSpeed = rotateSpeed * Time.deltaTime;
			m_Rotation.y += rotate.x * scaledRotateSpeed;
			m_Rotation.x = Mathf.Clamp(m_Rotation.x - rotate.y * scaledRotateSpeed, -89, 89);
			transform.localEulerAngles = m_Rotation;
		}

		private IEnumerator BurstFire(int burstAmount)
		{
			for (var i = 0; i < burstAmount; ++i)
			{
				Fire();
				yield return new WaitForSeconds(0.1f);
			}
		}

		private void Fire()
		{
			var transform = this.transform;
			var newProjectile = Instantiate(projectile);
			newProjectile.transform.position = transform.position + transform.forward * 0.6f;
			newProjectile.transform.rotation = transform.rotation;
			const int size = 1;
			newProjectile.transform.localScale *= size;
			newProjectile.GetComponent<Rigidbody>().mass = Mathf.Pow(size, 3);
			newProjectile.GetComponent<Rigidbody>().AddForce(transform.forward * 20f, ForceMode.Impulse);
			newProjectile.GetComponent<MeshRenderer>().material.color =
				new Color(Random.value, Random.value, Random.value, 1.0f);
		}
	}
}

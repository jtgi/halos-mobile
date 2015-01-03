/************************************************************************************

Copyright   :   Copyright 2014 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.2 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

http://www.oculusvr.com/licenses/LICENSE-3.2

Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Controls the player's movement in virtual reality.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
	/// <summary>
	/// The rate acceleration during movement.
	/// </summary>
	public float Acceleration = 0.1f;
	
	/// <summary>
	/// The rate of damping on movement.
	/// </summary>
	public float Damping = 0.3f;
	
	/// <summary>
	/// The rate of additional damping when moving sideways or backwards.
	/// </summary>
	public float BackAndSideDampen = 0.5f;
	
	/// <summary>
	/// The force applied to the character when jumping.
	/// </summary>
	public float JumpForce = 0.3f;
	
	/// <summary>
	/// The rate of rotation when using a gamepad.
	/// </summary>
	public float RotationAmount = 1.5f;
	
	/// <summary>
	/// The rate of rotation when using the keyboard.
	/// </summary>
	public float RotationRatchet = 45.0f;
	
	/// <summary>
	/// If true, tracking data from a child OVRCameraRig will update the direction of movement.
	/// </summary>
	public bool HmdRotatesY = true;
	
	/// <summary>
	/// Modifies the strength of gravity.
	/// </summary>
	public float GravityModifier = 0.379f;

	public bool editorModeEnabled = false;
	public float accelerometerMultiplier = 10f;


	private float MoveScale = 1.0f;
	private Vector3 MoveThrottle = Vector3.zero;
	private float FallSpeed = 0.0f;	


	/// <summary>
	/// If true, each OVRPlayerController will use the player's physical height.
	/// </summary>
	public bool useProfileHeight = true;
	
	public float terminalVelocity = 2f;
	public AudioSource windSound;
	public AudioSource standbyWindSound;
	
	protected CharacterController Controller = null;

	private float MoveScaleMultiplier = 1.0f;
	private float RotationScaleMultiplier = 1.0f;
	private bool  HaltUpdateMovement = false;
	private float SimulationRate = 60f;
	private Vector3 baseAxis = Vector3.zero;
	
	
	void Awake()
	{
		Controller = gameObject.GetComponent<CharacterController>();
		
		
		if(Controller == null)
			Debug.LogWarning("OVRPlayerController: No CharacterController attached.");

	
	}
	
	protected virtual void FixedUpdate()
	{
		UpdateMovement();
		
		Vector3 moveDirection = Vector3.zero;
		
		float motorDamp = (1.0f + (Damping * SimulationRate * Time.deltaTime));
		
		MoveThrottle.x /= motorDamp;
		MoveThrottle.y = (MoveThrottle.y > 0.0f) ? (MoveThrottle.y / motorDamp) : MoveThrottle.y;
		MoveThrottle.z /= motorDamp;
		
		moveDirection += MoveThrottle * SimulationRate * Time.deltaTime;
		
		bool hitTerminalVelocity = 	Mathf.Abs(FallSpeed) > terminalVelocity;
		
		// Gravity
		if(hitTerminalVelocity) {
			FallSpeed = -terminalVelocity;
		} else if (Controller.isGrounded && FallSpeed <= 0) {
			FallSpeed = ((Physics.gravity.y * (GravityModifier * 0.002f)));
		} else {
			FallSpeed += ((Physics.gravity.y * (GravityModifier * 0.002f)) * SimulationRate * Time.deltaTime);
		}
		
		if(!hitTerminalVelocity) {
			moveDirection.y += FallSpeed * SimulationRate * Time.deltaTime;
		} else {
			moveDirection.y = FallSpeed * SimulationRate * Time.deltaTime;
		}
		
		if(FallSpeed < -0.5f) {
			if (!windSound.isPlaying) {
				windSound.Play();
			}
			
			if(windSound.volume < 0.35f) {
				windSound.volume += 0.1f * Time.deltaTime;
				standbyWindSound.volume -= 0.5f * Time.deltaTime;
			}
			
			if(standbyWindSound.volume <= 0) {
				standbyWindSound.Stop();
			}
		} else if(Controller.isGrounded) {
			if(windSound.isPlaying) {
				windSound.volume -= 0.5f * Time.deltaTime;
			}
			
			if(!standbyWindSound.isPlaying) {
				standbyWindSound.Play();
			}
			
			if(standbyWindSound.volume < 1f) {
				standbyWindSound.volume += 0.1f * Time.deltaTime;
			}
			
			if(windSound.volume <= 0) {
				windSound.Stop();
			}
		}
		
		// Offset correction for uneven ground
		float bumpUpOffset = 0.0f;
		
		if (Controller.isGrounded && MoveThrottle.y <= 0.001f)
		{
			bumpUpOffset = Mathf.Max(Controller.stepOffset, new Vector3(moveDirection.x, 0, moveDirection.z).magnitude);
			moveDirection -= bumpUpOffset * Vector3.up;
		}
		
		Vector3 predictedXZ = Vector3.Scale((Controller.transform.localPosition + moveDirection), new Vector3(1, 0, 1));
		
		// Move contoller
		Controller.Move(moveDirection);
		
		Vector3 actualXZ = Vector3.Scale(Controller.transform.localPosition, new Vector3(1, 0, 1));
		
		if (predictedXZ != actualXZ)
			MoveThrottle += (actualXZ - predictedXZ) / (SimulationRate * Time.deltaTime);
	}
	
	public virtual void UpdateMovement()
	{
		if (HaltUpdateMovement)
			return;
		
		bool moveForward = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
		bool moveLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
		bool moveRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
		bool moveBack = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
	
		MoveScale = 1.0f;
		
		if ( (moveForward && moveLeft) || (moveForward && moveRight) ||
		    (moveBack && moveLeft)    || (moveBack && moveRight) )
			MoveScale = 0.70710678f;
		
		
		MoveScale *= SimulationRate * Time.deltaTime;
		
		// Compute this for key movement
		float moveInfluence = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;
		
		Quaternion ort = transform.rotation;
		Vector3 ortEuler = ort.eulerAngles;
		ortEuler.z = ortEuler.x = 0f;
		ort = Quaternion.Euler(ortEuler);
		
		if (moveForward)
			MoveThrottle += ort * (transform.lossyScale.z * moveInfluence * BackAndSideDampen * Vector3.forward);
		if (moveBack)
			MoveThrottle += ort * (transform.lossyScale.z * moveInfluence * BackAndSideDampen * Vector3.back);
		if (moveLeft)
			MoveThrottle += ort * (transform.lossyScale.x * moveInfluence * BackAndSideDampen * Vector3.left);
		if (moveRight)
			MoveThrottle += ort * (transform.lossyScale.x * moveInfluence * BackAndSideDampen * Vector3.right);
		
		if(Input.GetKey(KeyCode.Space)) 
			Jump ();
	
		moveInfluence = SimulationRate * Time.deltaTime * Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;

		float leftAxisX = editorModeEnabled ? Input.GetAxis("Horizontal") : (Input.acceleration.x - baseAxis.x) * accelerometerMultiplier;
		float leftAxisY = editorModeEnabled ? Input.GetAxis("Vertical") : (Input.acceleration.y - baseAxis.y) * accelerometerMultiplier;

		if(leftAxisY > 0.0f)
			MoveThrottle += ort * (leftAxisY * moveInfluence * BackAndSideDampen * Vector3.forward);
		
		if(leftAxisY < 0.0f)
			MoveThrottle += ort * (Mathf.Abs(leftAxisY) * moveInfluence * BackAndSideDampen * Vector3.back);
		
		if(leftAxisX < 0.0f)
			MoveThrottle += ort * (Mathf.Abs(leftAxisX) * moveInfluence * BackAndSideDampen * Vector3.left);
		
		if(leftAxisX > 0.0f)
			MoveThrottle += ort * (leftAxisX * moveInfluence * BackAndSideDampen * Vector3.right);

	}

	public void SetBaseAxis(Vector3 axis) {
		baseAxis = axis;
	}

	public Vector3 GetBaseAxis() {
		return baseAxis;
	}

	public bool Jump()
	{
		if (!Controller.isGrounded)
			return false;
		
		MoveThrottle += new Vector3(0, JumpForce, 0);
		
		return true;
	}
	

	/// <summary>
	/// Gets the move scale multiplier.
	/// </summary>
	/// <param name="moveScaleMultiplier">Move scale multiplier.</param>
	public void GetMoveScaleMultiplier(ref float moveScaleMultiplier)
	{
		moveScaleMultiplier = MoveScaleMultiplier;
	}
	
	/// <summary>
	/// Sets the move scale multiplier.
	/// </summary>
	/// <param name="moveScaleMultiplier">Move scale multiplier.</param>
	public void SetMoveScaleMultiplier(float moveScaleMultiplier)
	{
		MoveScaleMultiplier = moveScaleMultiplier;
	}
	
	/// <summary>
	/// Gets the rotation scale multiplier.
	/// </summary>
	/// <param name="rotationScaleMultiplier">Rotation scale multiplier.</param>
	public void GetRotationScaleMultiplier(ref float rotationScaleMultiplier)
	{
		rotationScaleMultiplier = RotationScaleMultiplier;
	}
	
	/// <summary>
	/// Sets the rotation scale multiplier.
	/// </summary>
	/// <param name="rotationScaleMultiplier">Rotation scale multiplier.</param>
	public void SetRotationScaleMultiplier(float rotationScaleMultiplier)
	{
		RotationScaleMultiplier = rotationScaleMultiplier;
	}

	
	/// <summary>
	/// Gets the halt update movement.
	/// </summary>
	/// <param name="haltUpdateMovement">Halt update movement.</param>
	public void GetHaltUpdateMovement(ref bool haltUpdateMovement)
	{
		haltUpdateMovement = HaltUpdateMovement;
	}
	
	/// <summary>
	/// Sets the halt update movement.
	/// </summary>
	/// <param name="haltUpdateMovement">If set to <c>true</c> halt update movement.</param>
	public void SetHaltUpdateMovement(bool haltUpdateMovement)
	{
		HaltUpdateMovement = haltUpdateMovement;
	}

}


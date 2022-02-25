using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
	CharacterController Controller;

	[SerializeField]
	Transform CameraHolder;

	[SerializeField]
	float MoveSpeed, SprintSpeed, Gravity, VelY = 0.0f, JumpHeight;

	[HideInInspector]
	float SmoothTurnAngle;

	[SerializeField]
	LayerMask Mask;

	[SerializeField]
	int Jumps = 2;

	[SerializeField]
	Vector3 GroundNormal = Vector3.zero;

	[SerializeField]
	Animator Animator;

	[SerializeField]
	bool IsSliding = false;

	void Start()
	{
		Controller = GetComponent<CharacterController>();
	}

	bool IsGrounded()
	{
		return Controller.isGrounded || Physics.CheckSphere(transform.position + Vector3.down * (Controller.height / 2.0f), Controller.skinWidth + 0.1f, Mask);
	}

	// Update is called once per frame
	void Update()
	{
		IsSliding = false;

		if (IsGrounded() && Physics.CapsuleCast(transform.position + Vector3.up * (Controller.height / 2.0f - Controller.radius), transform.position + Vector3.down * (Controller.height / 2.0f - Controller.radius), Controller.radius, Vector3.down, out RaycastHit hit, Mathf.Abs(VelY), Mask))
		{
			GroundNormal = hit.normal;
		}
		else
		{
			GroundNormal = Vector3.zero;
		}

		if(GroundNormal.magnitude < 0.1f)
        {
			VelY -= Gravity * Time.deltaTime;
		}

		else if(Mathf.Acos(Vector3.Dot(GroundNormal, Vector3.up)) * Mathf.Rad2Deg > Controller.slopeLimit)
        {
			VelY -= Gravity * (1.0f - Vector3.Dot(GroundNormal, Vector3.up)) * Time.deltaTime;

			Vector3 groundZ = Vector3.Cross(Vector3.right, GroundNormal).normalized;
			Vector3 groundX = Vector3.Cross(GroundNormal, groundZ).normalized;

			Vector3 downVel = Vector3.up * VelY;
			Vector3 slideVel = groundX * Vector3.Dot(downVel, groundX) + groundZ * Vector3.Dot(downVel, groundZ) + Vector3.down;

			Controller.Move(slideVel * Time.deltaTime);

			IsSliding = true;
		}
		else if(VelY < 0.0f)
        {
			VelY = -1.0f;
			Jumps = 2;
		}

		if (Input.GetKeyDown(KeyCode.Space) && Jumps > 0)
		{
			Jumps--;
			VelY = Mathf.Sqrt(2.0f * Gravity * JumpHeight);
		}

		if(!IsSliding)
			Controller.Move(Vector3.up * VelY * Time.deltaTime);

		Vector3 input = new Vector3
		{
			x = Input.GetAxis("Horizontal"),
			z = Input.GetAxis("Vertical")

		};

		input = Vector3.ClampMagnitude(input, 1.0f);

		if (input.magnitude >= 0.1f && !IsSliding)
		{
			float targetAngle = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg + CameraHolder.rotation.eulerAngles.y;
			float angle = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, targetAngle, ref SmoothTurnAngle, 0.15f);

			transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);

			Vector3 moveDir = Quaternion.Euler(0.0f, targetAngle, 0.0f) * Vector3.forward * input.magnitude;

			if (GroundNormal.magnitude >= 0.1f)
			{
				Vector3 groundZ = Vector3.Cross(Vector3.right, GroundNormal).normalized;
				Vector3 groundX = Vector3.Cross(GroundNormal, groundZ).normalized;

				moveDir = groundX * Vector3.Dot(moveDir, groundX) + groundZ * Vector3.Dot(moveDir, groundZ);
			}

			Controller.Move((Input.GetKey(KeyCode.R) ? SprintSpeed : MoveSpeed) * Time.deltaTime * moveDir);
		}

		Animator.SetBool("Grounded", IsGrounded() && !IsSliding);
		Animator.SetFloat("MoveMag", input.magnitude);
	}
}

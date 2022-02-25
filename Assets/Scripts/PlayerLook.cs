using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
	[SerializeField]
	float MouseX = 0.0f, MouseY = 0.0f, SensitivityX, SensitivityY, ArmLength;

	[SerializeField]
	Transform CameraHolder, Head;

	// Start is called before the first frame update
	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}

	// Update is called once per frame
	void Update()
	{
		MouseX -= Input.GetAxis("Mouse Y") * SensitivityX * Time.deltaTime;
		MouseX = Mathf.Clamp(MouseX, -60.0f, 60.0f);

		MouseY += Input.GetAxis("Mouse X") * SensitivityX * Time.deltaTime;

		CameraHolder.rotation = Quaternion.Euler(MouseX, MouseY, 0.0f);
		Vector3 targetPosition = Head.position + CameraHolder.rotation * (ArmLength * Vector3.back);

        if (Physics.SphereCast(new Ray(Head.position, targetPosition - Head.position), 0.35f, out RaycastHit cameraHit, Vector3.Distance(Head.position, targetPosition), ~(1 << LayerMask.NameToLayer("Player"))))
        {
            targetPosition = cameraHit.point + 0.35f * cameraHit.normal;
            CameraHolder.rotation = Quaternion.LookRotation((Head.position - targetPosition).normalized);
        }

        CameraHolder.position = targetPosition;
	}
}

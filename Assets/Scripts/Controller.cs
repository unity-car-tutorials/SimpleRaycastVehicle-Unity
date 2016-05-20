using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Controller : MonoBehaviour {

	private Rigidbody rb;
	public Transform FL, FR, BL, BR;
	public float torque = 100;
	public float maxAngle = 30;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		rb.centerOfMass = new Vector3 (0, -.5f, 0);
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		float forward = Input.GetAxis ("Vertical");
		float steer = Input.GetAxis ("Horizontal");

		Vector3 ff = (Vector3.forward * torque * forward);
		//Vector3 ss = (Vector3.up * torque * 100 * steer);

		rb.AddRelativeForce (ff);
		//rb.AddRelativeTorque (ss);
		if (FL & FR) 
		{
			FR.localRotation = Quaternion.AngleAxis (steer * maxAngle, Vector3.up);
			FL.localRotation = Quaternion.AngleAxis (steer * maxAngle, Vector3.up);

		}
	}
}

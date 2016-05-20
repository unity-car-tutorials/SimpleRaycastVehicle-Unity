// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;

public class RaycastWheelSimple : MonoBehaviour {

	// Simple Vehicle Raycast wheel

	public Transform graphic;

	public float mass = 1.0f;
	public float radius = 1.0f;
	public float maxSuspension = 0.2f;
	public float spring = 100.0f;
	public float damper = 0.0f;

	public float wheelAngle = 0f;

	private float circumference;

	private float contactPatchArea;
	private Rigidbody parent;
	private bool grounded = false;

	public bool IsGrounded 
	{
		get 
		{
			return grounded;
		}
	}

	void  Awake ()
	{
		circumference = (radius * 2) * Mathf.PI;
		parent = transform.root.GetComponent<Rigidbody>();
	}

	void  FixedUpdate ()
	{
		GetGround();
	}

	void  GetGround (){
		grounded = false;
		Vector3 downwards = transform.TransformDirection (-Vector3.up);
		RaycastHit hit;

		// down = local downwards direction
		Vector3 down = transform.TransformDirection(Vector3.down); 

		if (Physics.Raycast(transform.position, downwards, out hit, radius + maxSuspension)) {

			grounded = true;
			// the velocity at point of contact
			Vector3 velocityAtTouch = parent.GetPointVelocity(hit.point);

			// calculate spring compression
			// difference in positions divided by total suspension range
			float compression = hit.distance / (maxSuspension + radius);
			compression = -compression + 1;

			// final force
			Vector3 force = -downwards * compression * spring;
			// velocity at point of contact transformed into local space

			Vector3 t = transform.InverseTransformDirection(velocityAtTouch);

			// local x and z directions = 0
			t.z = t.x = 0;

			// back to world space * -damping
			Vector3 damping = transform.TransformDirection(t) * -damper;
			Vector3 finalForce = force + damping;

			// VERY simple turning - force rigidbody in direction of wheel
			t = parent.transform.InverseTransformDirection(velocityAtTouch);
			t.y = 0;
			t.z = 0;

			t = transform.TransformDirection(t);

			parent.AddForceAtPosition(finalForce + (t), hit.point);
	
			if (graphic) graphic.position = transform.position + (down * (hit.distance - radius));

		}
		else
		{
			if (graphic) graphic.position = transform.position + (down * maxSuspension);
		}
			
		float speed = parent.velocity.magnitude;

		if (graphic) 
		{
			//graphic.transform.localEulerAngles = new Vector3 (graphic.transform.localEulerAngles.x, wheelAngle, graphic.transform.localEulerAngles.z); 
			//graphic.transform.Rotate (360 * (speed / circumference) * Time.fixedDeltaTime, 0, 0); 
		}
	}

	void  OnDrawGizmosSelected ()
	{
		Gizmos.color = new Color(0,1,0,1);
		Vector3 direction = transform.TransformDirection (-Vector3.up) * (this.radius);
		Gizmos.DrawRay(transform.position,direction);

		Gizmos.color = new Color(0,0,1,1);
		direction = transform.TransformDirection (-Vector3.up) * (this.maxSuspension);
		Gizmos.DrawRay(new Vector3(transform.position.x,transform.position.y - radius,transform.position.z),direction);
	}
}
using UnityEngine;
using System.Collections;

// Do test the code! You usually need to change a few small bits.

[RequireComponent(typeof(Rigidbody))]	
public class RaycastWheel : MonoBehaviour {

	// More advanced Vehicle Raycast wheel

	public Transform graphic;

	public float mass = 1.00f;
	public float wheelRadius  = 0.5f;
	public float suspensionRange  = 0.10f;
	public float suspensionForce  = 0.00f;
	public float suspensionDamp  = 0.00f;
	public float compressionFrictionFactor  = 0.00f;

	public float sidewaysFriction  = 0.00f;
	public float sidewaysDamp  = 0.00f;
	public float sidewaysSlipVelocity  = 0.00f;
	public float sidewaysSlipForce  = 0.00f;
	public float sidewaysSlipFriction  = 0.00f; 
	public float sidewaysStiffnessFactor  = 0.00f;

	public float forwardFriction  = 0.00f;
	public float forwardSlipVelocity  = 0.00f;
	public float forwardSlipForce  = 0.00f;
	public float forwardSlipFriction  = 0.00f;
	public float forwardStiffnessFactor  = 0.00f; 

	public float frictionSmoothing  = 1.00f;

	public float usedSideFriction  = 0.00f;
	public float usedForwardFriction  = 0.00f;
	public float sideSlip  = 0.00f;
	public float forwardSlip  = 0.00f;

	public bool driven  = false;
	public bool brake  = false;
	public bool skidbrake  = false;

	public float speed  = 0.00f;

	private RaycastHit hit;

	private Rigidbody parent;

	private float wheelCircumference = 0.00f;

	private bool grounded = false;

	public bool IsGrounded 
	{
		get 
		{
			return grounded;
		}
	}

	void Start ()
	{
		wheelCircumference = wheelRadius * Mathf.PI * 2;
		parent = transform.root.GetComponent<Rigidbody>();
	}

	void  FixedUpdate ()
	{
		// the object this script is attached to does not move. This ray is like imagining
		// that the shock is compressed as much as possible, and then extends out as far as it can before it hits the ground. 
		// If it doesn't, then the wheel is fully extended.

		Vector3 down = transform.TransformDirection(Vector3.down); 

		// if raycast from position downwards with range suspensionrange + wheelRadius
		// and hit.collider != the parent (car)

		// calculate the force of the shock as it pushes on the car and the earth due to compression. 
		// Since the earth does not move, this is an upward force on the car only

		if(Physics.Raycast (transform.position, down, out hit, suspensionRange + wheelRadius) && hit.collider.transform.root != transform.root)
		{
			// how fast is the wheel moving relative to the ground, without taking the wheel's rotation into account
			Vector3 velocityAtTouch = parent.GetPointVelocity(hit.point);

			// calculate spring compression
			// difference in positions divided by total suspension range
			float compression = hit.distance / (suspensionRange + wheelRadius);
			compression = -compression + 1;

			// final force
			Vector3 force = -down * compression * suspensionForce;

		
			// velocity at point of contact transformed into local space
			Vector3 t = transform.InverseTransformDirection(velocityAtTouch);

			// local x and z directions = 0
			// Here we set t equal to the speed at which the shock is contracting / expanding
			t.z = 0;
			t.x = 0;

			// back to world space * -damping
			// this force simulates the force exerted by the friction in the suspension.
			Vector3 shockDrag = transform.TransformDirection(t) * -suspensionDamp;

			// forwardDifference = local speed along the z axis
			// the difference between the speed of the ground and the wheel taking rotation + the car's velocity into account. 
			// (in local space, that means, relative to the wheel. So if you were standing on the wheel surface, 
			// how fast would you percieve the ground moving? Either squashing you every time the wheel goes around 
			// (not skidding, absolute value close to zero) or scrapping you to bits (skidding, large absolute value))

			float forwardDifference = transform.InverseTransformDirection(velocityAtTouch).z - speed;

			// newForwardFriction = interpolated between forwardFriction (constant) and
			// forwardSlipFriction. Interpolation is forwardSlip squared.
			// so if forwardSlip = 1, newForwardFriction = forwardSlipFriction


			//Ok, this next part is not related to real physics an any way whatsoever. Ummm... Basically the friction that the wheel has with 
			// the ground changes (using a lerp function over time) depending on the current friction force. 
			// I guess this simulates how once a car starts skidding, the friction goes down so it skids more.
			// __________________z-friction__________________
			// move the current working friction value toward the minimum about porportional to the "skidding-ness" squared

			float newForwardFriction = Mathf.Lerp(forwardFriction, forwardSlipFriction, forwardSlip * forwardSlip);

			// increase the current working friction value depending on how hard the shock is pressing the wheel against the ground
			newForwardFriction = Mathf.Lerp(newForwardFriction, newForwardFriction * compression * 1, compressionFrictionFactor);

			if(frictionSmoothing > 0)
				usedForwardFriction = Mathf.Lerp(usedForwardFriction, newForwardFriction, Time.fixedDeltaTime / frictionSmoothing);
			else
				usedForwardFriction = newForwardFriction;

			// calculate one component of the friction force: the difference between the wheel surface velocity and ground surface 
			// velocity times friction (not based on real physics AFAIK)
			Vector3 forwardForce = transform.TransformDirection(new Vector3(0, 0, -forwardDifference)) * usedForwardFriction;

			// calculate how much we be slippin  (if the force is high, the tire will give and slip on the road)
			forwardSlip = Mathf.Lerp(forwardForce.magnitude / forwardSlipForce, forwardDifference / forwardSlipVelocity, forwardStiffnessFactor);

			float sidewaysDifference = transform.InverseTransformDirection(velocityAtTouch).x;
			float newSideFriction = Mathf.Lerp(sidewaysFriction, sidewaysSlipFriction, sideSlip * sideSlip);
			newSideFriction = Mathf.Lerp(newSideFriction, newSideFriction * compression * 1, compressionFrictionFactor);

			if(frictionSmoothing > 0)
				usedSideFriction = Mathf.Lerp(usedSideFriction, newSideFriction, Time.fixedDeltaTime / frictionSmoothing);
			else
				usedSideFriction = newSideFriction;

			// ____________________________________
			// this is much the same as the block above, but for the x-axis
			// __________________x-friction__________________

			Vector3 sideForce = transform.TransformDirection(new Vector3(-sidewaysDifference, 0, 0)) * usedSideFriction;
			sideSlip = Mathf.Lerp(sideForce.magnitude / sidewaysSlipForce, sidewaysDifference / sidewaysSlipVelocity, sidewaysStiffnessFactor);

			// this thing is totally made up. It's as if god's hand nudges the car back on course whenever it is moving sideways, 
			// by a factor of sidewaysDamp
			t = transform.InverseTransformDirection(velocityAtTouch);
			t.z = 0;
			t.y = 0;

			Vector3 sideDrag = transform.TransformDirection(t) * -sidewaysDamp;

			// By the some of all you combined, I become CAPTAIN FORCE
			parent.AddForceAtPosition(force + shockDrag - forwardForce + sideForce + sideDrag, transform.position);


			// for every action there is an opposite reaction: the wheel adds a force on the ground, but the ground does not move, 
			// so that force is added to the car instead in the opposite direction (this lets the engine propel the thing forward). 
			// But!!! we also have to add a force on the engine, because otherwise we'd be adding force from nothing. 
			// The opposite reaction is this force on the motor. The motor is not a rigidbody, it is a made up thing in the Car.js script
			if(driven)
			{
				//car.AddForceOnMotor (forwardDifference * usedForwardFriction * Time.fixedDeltaTime);
			}
			else
			speed += forwardDifference;	

			graphic.position = transform.position + (down * (hit.distance - wheelRadius));
		}
		else
		{
			graphic.position = transform.position + (down * suspensionRange);
		}

		speed = parent.velocity.magnitude;

		graphic.transform.Rotate(360 * (speed / wheelCircumference) * Time.fixedDeltaTime, 0, 0); 
	}

	void  OnDrawGizmosSelected ()
	{
		Gizmos.color = new Color(0,1,0,1);
		Vector3 direction = transform.TransformDirection (-Vector3.up) * (this.wheelRadius);
		Gizmos.DrawRay(transform.position,direction);

		Gizmos.color = new Color(0,0,1,1);
		direction = transform.TransformDirection (-Vector3.up) * (this.suspensionRange);
		Gizmos.DrawRay(new Vector3(transform.position.x,transform.position.y - wheelRadius,transform.position.z),direction);
	}
}
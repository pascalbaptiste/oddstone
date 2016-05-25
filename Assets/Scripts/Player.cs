using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	float accelerationTimeAir = 0.2f;
	float accelerationTimeGround = 0.1f;
	float timeToJumpApex = 0.4f;
	float jumpHeight = 3f;
	float moveSpeed = 6f;

	float gravity;
	float jumpVelocity;
	float velocityXSmoothing;
	Vector3 velocity;

	Controller2D controller;

	void Start () {
		controller = GetComponent<Controller2D> ();

		gravity = -(2 * jumpHeight / Mathf.Pow(timeToJumpApex, 2));
		jumpVelocity = Mathf.Abs (gravity) * timeToJumpApex;
	}

	void Update () {
		// capture device input
		float horizontal = Input.GetAxisRaw ("Horizontal");
		float vertical = Input.GetAxisRaw ("Vertical");
		Vector2 input = new Vector2 (horizontal, vertical);

		// Store reference to jump button.
		bool jumpButton = Input.GetKeyDown (KeyCode.JoystickButton1);

		// If we jump into something, nuke all vertical velocity.
		if (controller.collisions.above) {
			velocity.y = 0;
		}
		// Or if we are on the ground, nuke all vertical velocity
		// and reset our jump counter.
		else if (controller.collisions.below) {
			velocity.y = 0;
			controller.jumpInfo.count = 0;
		}
		// Otherwise we must be in freefall, so let’s update our
		// jump count to only allow for a single jump
		else if (controller.jumpInfo.count != 2) {
			controller.jumpInfo.count = 1;
		}

		// If our jump button is pressed, and we haven’t reached
		// our double jump limit, let’s update our vertical velocity.
		if (jumpButton && controller.jumpInfo.count < 2) {
			velocity.y = jumpVelocity;
			controller.jumpInfo.count++;
		}

		// Augment our horizontal velocity using a `Smooth Damp` function to
		// emulate acceleration, using different values for air and ground travel.
		float targetVelocityX = input.x * moveSpeed;
		float smoothTime = (controller.collisions.below) ? accelerationTimeGround : accelerationTimeAir;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, smoothTime);

		// Augment our vertical velocity with gravity.
		velocity.y += gravity * Time.deltaTime;

		// Send the updated velocity to our controller!
		controller.Move (velocity * Time.deltaTime);
	}
}

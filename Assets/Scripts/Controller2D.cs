using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour {

	// When our character is flush with the ground, a small
	// offset is used to reduce the collider bounds, so that
	// we have some space for raycasting.
	const float boundsOffset = 0.015f;

	public LayerMask collisionMask;

	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;

	float horizontalRaySpacing;
	float verticalRaySpacing;

	new BoxCollider2D collider;
	RaycastOrigins raycastOrigins;

	public CollisionInfo collisions;
	public JumpInfo jumpInfo;

	void Start () {
		collider = GetComponent<BoxCollider2D> ();
		CalculateRaySpacing ();

		// Confirm our controller is registered.
		if (Input.GetJoystickNames ().Length > 0) {
			print ("DualShock 4 Ready!");
		}
	}

	public void Move (Vector3 velocity) {
		UpdateRaycastOrigins ();

		// Delete the previous state, and check for collisions.
		collisions.Reset ();
		if (velocity.x != 0) CheckHorizontalCollisions (ref velocity);
		if (velocity.y != 0) CheckVerticalCollisions (ref velocity);

		transform.Translate (velocity);
	}

	void CheckHorizontalCollisions (ref Vector3 velocity) {
		// Resolves to 1 for right movement, or -1 for left movement.
		float directionX = Mathf.Sign (velocity.x);

		// We need to add the bounds offset back for an accurate ray length.
		float rayLength = Mathf.Abs (velocity.x) + boundsOffset;

		for (int i = 0; i < horizontalRayCount; i++) {
			// We’ll start our raycasting from the bottom left bound when moving
			// left, and from the bottom right bound when moving right.
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;

			// Our ray origin will be offset right by the ray spacing, but we also
			// want to make sure to include the horizontal velocity so that rays are
			// cast from the imminent horizontal position (not where we used to be).
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			
			// Visualize ray casting...
			Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.white);

			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
			if (hit) {
				// Velocity matches distance to collision objection,
				// so when distance is 0, velocity is 0.
				velocity.x = (hit.distance - boundsOffset) * directionX;
				rayLength = hit.distance;

				// Update our collisions struct.
				collisions.left = directionX == -1;
				collisions.right = directionX == 1;
			}
		}
	}

	void CheckVerticalCollisions (ref Vector3 velocity) {
		// Resolves to 1 for upward movement, or -1 for downward movement.
		float directionY = Mathf.Sign (velocity.y);

		// We need to add the bounds offset back for an accurate ray length.
		float rayLength = Mathf.Abs (velocity.y) + boundsOffset;

		for (int i = 0; i < verticalRayCount; i++) {
			// We’ll start our raycasting from the bottom left bound when moving
			// downward, and from the top left bound when moving upwards.
			Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;

			// Our ray origin will be offset right by the ray spacing, but we also
			// want to make sure to include the horizontal velocity so that rays are
			// cast from the imminent horizontal position (not where we used to be).
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			
			// Visualize ray casting...
			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.white);

			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
			if (hit) {
				// Velocity matches distance to collision objection,
				// so when distance is 0, velocity is 0.
				velocity.y = (hit.distance - boundsOffset) * directionY;
				rayLength = hit.distance;

				// Update our collisions struct.
				collisions.below = directionY == -1;
				collisions.above = directionY == 1;
			}
		}
	}

	/**
	 * Simple method that returns the collider bounds,
	 * but slightly shrunken using our bounds offset.
	 **/
	Bounds getBounds () {
		Bounds bounds = collider.bounds;
		bounds.Expand (boundsOffset * -2);
		return bounds;
	}
		
	void UpdateRaycastOrigins () {
		Bounds bounds = getBounds ();
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
	}

	void CalculateRaySpacing () {
		Bounds bounds = getBounds ();

		// Ensure that we have at least 2 rays per axis.
		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp (verticalRayCount, 2, int.MaxValue);

		// Calculate the appropriate space between rays.
		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}

	public struct CollisionInfo {
		public bool above, below;
		public bool left, right;
		public void Reset () {
			above = below = false;
			left = right = false;
		}
	}

	public struct JumpInfo {
		public int count;
	}
}

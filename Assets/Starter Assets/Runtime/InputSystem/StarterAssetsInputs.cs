using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;
		public bool lookFromTouch;

        [Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
		[Header("Shooting settings")]
		public GameObject ProjectilePrefab;
		public Transform ProjectileSpawnPoint;
		public float projectileForce = 20f;
        [Header("Aiming")]
        public Camera aimCamera;
        public LayerMask aimMask = ~0;
        public float aimDistance = 150f;
        public bool projectileUsesGravity = false;
        readonly RaycastHit[] aimHits = new RaycastHit[64];
#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				lookFromTouch = false;
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
		public void OnShoot(InputValue value)
        {
         
            {
                if (!enabled || !value.isPressed || Time.timeScale == 0) return;
				Shoot();
            }
        }




#endif

		public Camera GetAimCamera()
        {
            if (!aimCamera && ProjectileSpawnPoint) aimCamera = ProjectileSpawnPoint.GetComponentInParent<Camera>();
            if (!aimCamera) aimCamera = Camera.main;
            return aimCamera;
        }

        public Ray GetAimRay()
        {
            var camera = GetAimCamera();
            return camera ? camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)) : new Ray(transform.position + Vector3.up * 1.5f, transform.forward);
        }

        public Vector3 GetAimPoint()
        {
            var ray = GetAimRay();
            return ClosestAimHit(ray, aimDistance, out var hit) ? hit.point : ray.GetPoint(aimDistance);
        }

        bool ClosestAimHit(Ray ray, float distance, out RaycastHit closest)
        {
            closest = default;
            float nearest = distance;
            bool found = false;
            int count = Physics.RaycastNonAlloc(ray, aimHits, distance, aimMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var hit = aimHits[i];
                var collider = hit.collider;
                if (!collider || collider.GetComponentInParent<StarterAssetsInputs>() == this || collider.CompareTag("Projectile") || (hit.rigidbody && hit.rigidbody.CompareTag("Projectile"))) continue;
                if (hit.distance >= nearest) continue;
                nearest = hit.distance; closest = hit; found = true;
            }
            return found;
        }

        private void Shoot()
        {
            if (!ProjectilePrefab || !ProjectileSpawnPoint) return;
            Vector3 target = GetAimPoint();
            Vector3 origin = ProjectileSpawnPoint.position;
            var camera = GetAimCamera();
            if (camera)
            {
                Vector3 connection = origin - camera.transform.position;
                float distance = connection.magnitude;
                if (distance > 0.01f && ClosestAimHit(new Ray(camera.transform.position, connection / distance), distance, out var obstruction))
                    origin = obstruction.point - connection.normalized * 0.12f;
            }
            Vector3 direction = (target - origin).normalized;
            if (direction.sqrMagnitude < 0.5f) direction = GetAimRay().direction;
            var projectile = Instantiate(ProjectilePrefab, origin, Quaternion.LookRotation(direction));
            var body = projectile.GetComponent<Rigidbody>();
            if (!body) { Destroy(projectile); return; }
            body.useGravity = projectileUsesGravity;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            foreach (var projectileCollider in projectile.GetComponentsInChildren<Collider>())
                foreach (var ownCollider in GetComponentsInChildren<Collider>())
                    Physics.IgnoreCollision(projectileCollider, ownCollider);
            body.AddForce(direction * projectileForce, ForceMode.Impulse);
            ProjectileSpawnPoint.SendMessageUpwards("OnWeaponFired", SendMessageOptions.DontRequireReceiver);
            Destroy(projectile, 5f);
        }

        public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}
		
		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}



using UnityEngine;
using MelonLoader;

namespace LIARSBAR_UTILS
{
    /// <summary>
    /// Handles player movement and rotation controls within the mod.
    /// Allows toggling control on/off.
    /// Rotation syncs player to camera direction when right mouse button is held.
    /// Vertical movement controlled by scroll wheel.
    /// </summary>
    public class PlayerController
    {
        // --- Settings ---
        private float moveSpeed = 5.0f;                 // Speed for keyboard movement (units per second)
        private float verticalSpeed = 20.0f;            // Speed for scroll wheel vertical movement (units per second)
        private float cameraSyncRotationSpeed = 180.0f; // Max degrees per second to sync player rotation to camera yaw

        // --- Control Toggle ---
        /// <summary>
        /// Gets whether the player position/rotation control is currently active.
        /// </summary>
        public bool IsPositionControlEnabled { get; private set; } = false;

        /// <summary>
        /// Toggles the activation state of the position and rotation controls.
        /// </summary>
        public void TogglePositionControl()
        {
            IsPositionControlEnabled = !IsPositionControlEnabled;
            MelonLogger.Msg($"Player control toggled: {IsPositionControlEnabled}"); // Log toggle state
        }

        /// <summary>
        /// Called every frame to update player position and rotation based on input,
        /// but only if position control is enabled.
        /// </summary>
        /// <param name="playerObject">The GameObject representing the local player.</param>
        public void Update(GameObject playerObject)
        {
            // Do nothing if the player object is invalid or control is disabled
            if (playerObject == null || !IsPositionControlEnabled) return;

            // Process input for movement and rotation
            HandleKeyboardMovement(playerObject);
            HandleCameraSyncRotation(playerObject); // Sync player rotation to camera only when right-clicking
            HandleScrollWheelMovement(playerObject); // Handle vertical movement
        }

        /// <summary>
        /// Handles player movement based on WASD and Arrow Keys.
        /// </summary>
        /// <param name="playerObject">The player's GameObject.</param>
        private void HandleKeyboardMovement(GameObject playerObject)
        {
            Vector3 movementDirection = Vector3.zero; // Initialize movement direction

            // --- Read Input ---
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) movementDirection += playerObject.transform.forward;
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) movementDirection -= playerObject.transform.forward;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) movementDirection -= playerObject.transform.right;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) movementDirection += playerObject.transform.right;

            // --- Apply Movement ---
            if (movementDirection != Vector3.zero)
            {
                movementDirection.Normalize();
                MovePlayer(playerObject, movementDirection);
            }
        }

        /// <summary>
        /// Handles vertical movement based on the mouse scroll wheel.
        /// </summary>
        /// <param name="playerObject">The player's GameObject.</param>
        private void HandleScrollWheelMovement(GameObject playerObject)
        {
            // Get scroll wheel input (-1 to 1 range, usually 0 unless scrolling)
            float scrollDelta = Input.GetAxis("Mouse ScrollWheel");

            // Check if there was any scroll input
            if (Mathf.Abs(scrollDelta) > 0.01f)
            {
                // Calculate vertical movement amount
                float verticalMovement = scrollDelta * verticalSpeed * Time.deltaTime;

                // Apply movement along the world's Y axis
                // Using Translate with Space.World ensures consistent up/down movement
                playerObject.transform.Translate(0, verticalMovement, 0, Space.World);

                // Sync position after moving
                SyncNetworkPosition(playerObject);
            }
        }

        /// <summary>
        /// Rotates the player object to match the main camera's horizontal direction,
        /// but only when right mouse button is held down.
        /// </summary>
        /// <param name="playerObject">The player's GameObject.</param>
        private void HandleCameraSyncRotation(GameObject playerObject)
        {
            // Only proceed if right mouse button is held down
            // This works even when the mouse is locked because Input.GetMouseButton still detects the button state
            if (!Input.GetMouseButton(1)) // 1 is right mouse button
            {
                return; // Exit if right mouse button is not being held
            }

            // Get the main camera
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return; // Exit if no main camera is tagged
            }

            // Get the camera's forward direction
            Vector3 cameraForward = mainCamera.transform.forward;

            // Project the camera's forward direction onto the horizontal plane (ignore vertical tilt)
            Vector3 cameraForwardHorizontal = Vector3.ProjectOnPlane(cameraForward, Vector3.up);

            // Ensure the projected vector is not zero (can happen if camera looks straight up/down)
            if (cameraForwardHorizontal.sqrMagnitude < 0.01f)
            {
                return; // Avoid issues with LookRotation if vector is near zero
            }

            // Normalize the horizontal direction
            cameraForwardHorizontal.Normalize();

            // Calculate the target rotation for the player to face this direction
            Quaternion targetRotation = Quaternion.LookRotation(cameraForwardHorizontal, Vector3.up);

            // Smoothly rotate the player towards the target rotation
            // Quaternion.RotateTowards limits the rotation speed per frame
            playerObject.transform.rotation = Quaternion.RotateTowards(
                playerObject.transform.rotation,
                targetRotation,
                cameraSyncRotationSpeed * Time.deltaTime // Max degrees to rotate this frame
            );

            // Sync rotation after potentially changing it
            SyncNetworkRotation(playerObject);
        }

        /// <summary>
        /// Calculates and applies horizontal movement to the player object.
        /// </summary>
        /// <param name="playerObject">The player's GameObject.</param>
        /// <param name="direction">The normalized direction vector for movement.</param>
        private void MovePlayer(GameObject playerObject, Vector3 direction)
        {
            // Calculate horizontal movement
            Vector3 movement = direction * moveSpeed * Time.deltaTime;
            // Apply movement
            playerObject.transform.position += movement;
            // Sync position
            SyncNetworkPosition(playerObject);
        }

        // --- Network Syncing Methods ---
        private void SyncNetworkPosition(GameObject playerObject)
        {
            Component networkTransform = FindComponentByName(playerObject, "NetworkTransformReliable");
            // Network sync would happen here in the actual implementation
        }

        private void SyncNetworkRotation(GameObject playerObject)
        {
            Component networkTransform = FindComponentByName(playerObject, "NetworkTransformReliable");
            // Network sync would happen here in the actual implementation
        }

        /// <summary>
        /// Helper method to find a component on a GameObject by its type name string.
        /// </summary>
        private Component FindComponentByName(GameObject obj, string componentName)
        {
            if (obj == null) return null;
            foreach (var component in obj.GetComponents<Component>())
            {
                if (component != null && component.GetIl2CppType().Name == componentName)
                {
                    return component;
                }
            }
            return null;
        }
    }
}
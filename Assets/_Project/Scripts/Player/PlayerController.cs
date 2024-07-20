using Project.Input;
using UnityEngine;

namespace Project.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : PlayerPrediction
    {
        #region Variables

        [Header("SETTINGS")]
        [SerializeField] private float walkSpeed;
        [SerializeField] private float sprintSpeed;
        [SerializeField] private float crouchSpeed;
        [SerializeField] private float groundDrag;
        [Space(2)]
        [SerializeField] private float jumpForce;
        [SerializeField] private float jumpColldown;
        [SerializeField] private float airMultipier;
        [Space(10)]
        [Header("REFERENCES")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform orientation;
        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private float playerHeight;

        private bool grounded;
        private bool readyToJump = true;

        // References
        private Rigidbody rb;

        #endregion

        #region Callbacks

        public override void OnStartPlayer()
        {
            // Referencing
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            playerCamera.enabled = IsOwner;
            playerCamera.GetComponent<AudioListener>().enabled = IsOwner;

            // Cursoe
            if (IsLocalPlayer)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        public override void OnStopPlayer()
        {
            
        }

        public override void OnInput(ClientInputState input)
        {
            // Applying movement
            // Setting the drag
            grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

            if (grounded)
                rb.drag = groundDrag;
            else
                rb.drag = 0;

            // Calculating movement
            Vector2 moveInput = input.GetMoveInput();

            orientation.rotation = Quaternion.Euler(0, input.playerRotation, 0);
            Vector3 moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;

            // Applying movement

            float moveSpeed = input.isSprinting ? sprintSpeed : input.isCrouching ? crouchSpeed : walkSpeed;

            // Grounded
            if (grounded)
                rb.AddForce(moveDirection.normalized * moveSpeed * 10, ForceMode.Force);

            // In air
            else
                rb.AddForce(moveDirection.normalized * moveSpeed * 10 * airMultipier, ForceMode.Force);

            // Speed Control
            Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }

            if (input.isJumping && grounded && readyToJump)
            {
                // Resetting Y velocity
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

                // Applying Force
                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

                // Applying Cooldown
                readyToJump = false;
                Invoke(nameof(ResetJump), jumpColldown);
            }
        }

        public override void ApplyState(PlayerState state, uint tick = 0)
        {
            transform.position = state.position;
            transform.rotation = Quaternion.Euler(state.rotation);

            rb.velocity = state.velocity;

            if (tick == 0) CheckSavePlayerPrediction(tick);
        }

        public override PlayerState GetPlayerState(uint tick)
        {
            PlayerState state = new PlayerState();

            state.SetUp(uint.Parse(OwnerClientId.ToString()), transform.position, transform.eulerAngles, rb.velocity);

            return state;
        }
        #endregion

        #region Private Methods

        private void ResetJump()
        {
            readyToJump = true;
        }

        #endregion
    }
}
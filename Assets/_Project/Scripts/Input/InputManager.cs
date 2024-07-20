using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Input
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputManager : Singleton<InputManager>
    {
        #region Variables

        // References
        private PlayerInput playerInput;

        #endregion

        #region Callbacks

        private void Start()
        {
            playerInput = GetComponent<PlayerInput>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the input for the current Tick.
        /// </summary>
        /// <returns></returns>
        public ClientInputState GetInputForCurrentTick(uint currentTick)
        {
            // Getting Input
            Vector2 moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
            bool isSprinting = playerInput.actions["Sprint"].ReadValue<float>() >= 0.4f;
            bool isJumping = playerInput.actions["Jump"].ReadValue<float>() >= 0.4f;
            bool isCrouching = playerInput.actions["Crouch"].ReadValue<float>() >= 0.4f;

            // Setting Input
            ClientInputState input = new ClientInputState();
            input.SetUp(currentTick, moveInput, isSprinting, isJumping, isCrouching, transform.rotation.y);

            return input;
        }

        #endregion
    }

    #region Classes

    public class ClientInputState : INetworkSerializable
    {
        public uint tick;
        public float playerRotation;
        private byte essentialKeys;
        public bool isSprinting => (essentialKeys & (1 << 4)) != 0;
        public bool isJumping => (essentialKeys & (1 << 5)) != 0;
        public bool isCrouching => (essentialKeys & (1 << 6)) != 0;

        public void SetUp(uint tick, Vector2 moveInput, bool isSprinting, bool isJumping, bool isCrouching, float playerRotation)
        {
            this.tick = tick;
            this.playerRotation = playerRotation;

            essentialKeys = 0;
            if (moveInput.y > 0) essentialKeys |= 1 << 0;
            if (moveInput.x < 0) essentialKeys |= 1 << 1;
            if (moveInput.y < 0) essentialKeys |= 1 << 2;
            if (moveInput.x > 0) essentialKeys |= 1 << 3;
            if (isSprinting) essentialKeys |= 1 << 4;
            if (isJumping) essentialKeys |= 1 << 5;
            if (isCrouching) essentialKeys |= 1 << 6;
            //if (isCrouching) essentialKeys |= 1 << 7; Empty Slot to use in the future
        }

        public Vector2 GetMoveInput()
        {
            return new Vector2(
                (essentialKeys & (1 << 1)) != 0 ? -1 :
                (essentialKeys & (1 << 3)) != 0 ? 1 : 0,
                (essentialKeys & (1 << 0)) != 0 ? 1 :
                (essentialKeys & (1 << 2)) != 0 ? -1 : 0
                );
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref playerRotation);
            serializer.SerializeValue(ref essentialKeys);
        }
    }

    #endregion
}
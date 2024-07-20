using Project.Input;
using Project.Network;
using Project.UI;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Project.Player
{
    [RequireComponent(typeof(TickSystem))]
    public class InputBuffer : UpstreamThrotteling
    {
        #region Variables

        public Dictionary<uint, ClientInputState> inputBuffer { get; private set; } = new Dictionary<uint, ClientInputState>();
        public uint startingTick { get; private set; }
        private uint newestInputTick;

        private TickSystem tickSystem;

        #endregion

        #region Callbacks

        public override void OnSpawn()
        {
            // Input Buffer and Client Network Data Tick System
            if(IsHost && !IsLocalPlayer)
            {
                tickSystem = GetComponent<TickSystem>();
                tickSystem.SetTickRate(Simulation.TICKADJUSTMENTRATE);
                tickSystem.StartSystem();
            }

            SimulationStarted();
        }

        public override void OnDespawn()
        {
            Stopped();
        }

        public override void OnPlayer(uint tick)
        {
            // Getting input and predicting player.
            ClientInputState input = GetOwnInput(tick);
            ProcessInput(input);

            // The Host shouldnm't do that (useless for him)
            if (IsHost) 
            {
                NetworkInfo info = new NetworkInfo();
                info.SetUp(0, 0, 0, 0, 0);
                DebugUI.Instance.UpdateUI(info);
                return;
            }

            // Setting starting tick
            if (startingTick == 0) startingTick = tick;

            // Saving input
            inputBuffer[tick - startingTick] = input;

            // Getting last inputs and sending them to Server
            ClientInputState[] lastInputs = GetLastInputs(tick);
            OnClientInputRPC(lastInputs);
        }

        public override void OnServer(uint tick)
        {
            // Info for server
            NetworkData.Instance.UpdatePlayer(OwnerClientId);

            // Getting input for current Tick.
            ClientInputState input = new ClientInputState();
            if (!inputBuffer.ContainsKey(tick))
            {
                if (!inputBuffer.ContainsKey(tick - 1)) return;
                else
                {
                    input = inputBuffer[tick - 1];
                    inputBuffer[tick] = input;
                    Debug.LogWarning("Using last input.");
                }
            }
            else
            {
                input = inputBuffer[tick];
            }

            // Moving player on the Server / Host.
            ProcessInput(input);

            // Removing last input so we don't use too much space.
            inputBuffer.Remove(tick - 1);
            CheckInputBuffer(inputBuffer);
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Runs on Spawn
        /// </summary>
        public virtual void SimulationStarted() { }

        /// <summary>
        /// Runs on Despawn
        /// </summary>
        public virtual void Stopped() { }

        /// <summary>
        /// Runs every tick with the input to use.
        /// </summary>
        /// <param name="input"></param>
        public virtual void ProcessInput(ClientInputState input)
        {

        }

        #endregion

        #region RPCs

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
        private void OnClientInputRPC(ClientInputState[] inputs)
        {
            foreach (ClientInputState input in inputs)
            {
                if (input.tick <= NetworkSimulation.CurrentTick) continue;
                if (inputBuffer.ContainsKey(input.tick)) continue;
                if (inputBuffer.Keys.Count > Simulation.MAXBUFFERSIZE) continue;

                inputBuffer.Add(input.tick, input);

                if (newestInputTick < input.tick)
                    newestInputTick = input.tick;
            }
        }

        #endregion

        #region Input

        private ClientInputState[] GetLastInputs(uint tick)
        {
            uint size = uint.Parse(Mathf.Min(tick - startingTick, Simulation.INPUTAMMOUNT).ToString());
            ClientInputState[] inputs = new ClientInputState[size];

            for (uint i = 0; i < size; i++)
            {
                uint index = tick - startingTick - size + i;
                inputs[i] = inputBuffer[index];
            }

            return inputs;
        }

        /// <summary>
        /// Returns the client input
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
        private ClientInputState GetOwnInput(uint tick) => InputManager.Instance.GetInputForCurrentTick(tick);

        #endregion
    }
}
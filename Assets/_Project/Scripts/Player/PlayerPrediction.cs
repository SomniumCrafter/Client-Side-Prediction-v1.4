using Project.Input;
using Project.Network;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Project.Player
{
    public class PlayerPrediction : InputBuffer
    {
        #region Variables

        // Prediction
        private static ulong localClientId;
        private static Dictionary<ulong, PlayerPrediction> players = new Dictionary<ulong, PlayerPrediction>();
        public Dictionary<uint, PlayerState> stateBuffer { get; private set; } = new Dictionary<uint, PlayerState>();

        #endregion

        #region Callbacks

        public override void SimulationStarted()
        {
            players.Add(OwnerClientId, this);
            if (IsLocalPlayer) localClientId = OwnerClientId;
            OnStartPlayer();
        }

        public override void Stopped()
        {
            players.Remove(OwnerClientId);

            OnStopPlayer();
        }

        public override void ProcessInput(ClientInputState input)
        {
            OnInput(input);

            // Saving game state
            CheckSavePlayerPrediction(input.tick);
        }

        #endregion

        #region Virtual Methods

        public virtual void OnStartPlayer() { }
        public virtual void OnStopPlayer() { }
        public virtual void OnInput(ClientInputState input)
        {

        }
        public virtual void ApplyState(PlayerState state, uint tick = 0)
        {
            
        }
        public virtual PlayerState GetPlayerState(uint tick)
        {
            PlayerState state = new PlayerState();

            return state;
        }

        #endregion

        #region State

        public void CheckSavePlayerPrediction(uint tick)
        {
            // Getting Player State
            PlayerState state = GetPlayerState(tick);

            if (IsLocalPlayer && !IsHost)
            {
                if (!stateBuffer.ContainsKey(tick))
                    stateBuffer.Add(tick, state);
                else stateBuffer[tick] = state;
            }
            else if (!IsLocalPlayer && IsHost)
            {
                OnGameStateRPC(GetGameState(tick));
            }
        }

        private GameState GetGameState(uint tick)
        {
            PlayerState[] playerStates = new PlayerState[players.Count];

            int i = 0;
            foreach(PlayerPrediction player in players.Values)
            {
                PlayerState pS = player.GetPlayerState(tick);
                playerStates[i] = pS;
                i++;
            }

            GameState state = new GameState();

            state.SetUp(tick, playerStates);
            return state;
        }

        private void CheckReconciliation(uint tick, PlayerState state, GameState gameState)
        {
            if (!stateBuffer.ContainsKey(tick)) return;

            // Getting State and removing state from input Buffer for removing useless data.
            PlayerState predictedState = stateBuffer[tick];

            bool reconcile = false;

            if (Vector3.Distance(predictedState.position, state.position) > Simulation.MAXPOSITIONOFFSET) reconcile = true;

            if (Vector3.Distance(predictedState.rotation, state.rotation) > Simulation.MAXROTATIONOFFSET) reconcile = true;

            if (reconcile) Reconcile(tick, state, gameState);
        }

        private void Reconcile(uint tick, PlayerState serverState, GameState gameState)
        {
            Debug.LogWarning("Reconcile");

            // Moving to correct old Position.
            ApplyState(serverState, tick);

            uint t = tick;
            while(tick <= CurrentTick)
            {
                // Simulating
                Physics.Simulate(NetworkSimulation.Instance.tickSystem.timeBetweenTicks);

                // Moving Player
                ProcessInput(inputBuffer[tick - startingTick]);

                // Increasing Tick
                tick++;
            }
        }

        #endregion

        #region RPCs

        [Rpc(SendTo.Owner, Delivery = RpcDelivery.Reliable)]
        public void OnGameStateRPC(GameState state)
        {
            // Checking for Reconiliation
            for(int i = 0; i < state.playerStates.Length; i++)
            {
                PlayerState playerState = state.playerStates[i];
                PlayerPrediction player = players[ulong.Parse(playerState.clientId.ToString())];

                // This state is the state from the local Player
                if (playerState.clientId == localClientId)
                    player.CheckReconciliation(state.tick, playerState, state);
                else 
                    player.ApplyState(playerState);
            }
        }

        #endregion
    }

    #region Classes

    public class GameState : INetworkSerializable
    {
        public uint tick;
        public PlayerState[] playerStates;

        public void SetUp(uint tick, PlayerState[] playerStates)
        {
            this.tick = tick;
            this.playerStates = playerStates;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref playerStates);
        }
    }

    public class PlayerState : INetworkSerializable
    {
        public uint clientId;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 velocity;

        public void SetUp(uint clientId, Vector3 position, Vector3 rotation, Vector3 velocity)
        {
            this.clientId = clientId;
            this.position = position;
            this.rotation = rotation;
            this.velocity = velocity;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
            serializer.SerializeValue(ref velocity);
        }
    }

    #endregion
}
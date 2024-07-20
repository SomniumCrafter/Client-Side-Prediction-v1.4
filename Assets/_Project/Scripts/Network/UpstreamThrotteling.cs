using Project.Input;
using Project.UI;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Project.Network
{
    public class UpstreamThrotteling : NetworkBehaviour
    {
        #region Variables

        public uint CurrentTick => NetworkSimulation.CurrentTick;
        public float timeBetweenTicks => NetworkSimulation.Instance.tickSystem.startingTimeBetweenTicks;

        #endregion

        #region Callbacks

        public override void OnNetworkSpawn()
        {
            if(!IsLocalPlayer && (IsHost || IsServer))
                NetworkSimulation.Instance.tickSystem.OnTick += OnServer;
            else if(IsLocalPlayer)
                NetworkSimulation.Instance.tickSystem.OnTick += OnPlayer;

            if (IsLocalPlayer)
                StartSimulationFromServerTickRPC();
            else if (IsServer || IsHost)
                NetworkSimulation.Instance.StartSimulation();

            OnSpawn();
        }

        public override void OnNetworkDespawn()
        {
            if (!IsLocalPlayer && (IsHost || IsServer))
                NetworkSimulation.Instance.tickSystem.OnTick -= OnServer;
            else if (IsLocalPlayer)
                NetworkSimulation.Instance.tickSystem.OnTick -= OnPlayer;

            OnDespawn();
        }

        #endregion

        #region Virtual Methods

        public virtual void OnSpawn()
        {

        }

        public virtual void OnDespawn()
        {

        }

        /// <summary>
        /// Runs every tick on the Owner
        /// </summary>
        public virtual void OnPlayer(uint tick)
        {

        }

        /// <summary>
        /// Runs every tick on the Server/Host.
        /// </summary>
        public virtual void OnServer(uint tick)
        {

        }

        #endregion

        #region Input Buffer

        public void CheckInputBuffer(Dictionary<uint, ClientInputState> inputBuffer)
        {
            // Calculating Rick Adjustment based on Buffer Size
            int bufferSize = inputBuffer.Keys.Count;
            int targetBufferSize = NetworkData.Instance.GetBestBufferSizeForClient(OwnerClientId);
            int adjustmentValue = 0;

            if (bufferSize < Mathf.Max(targetBufferSize, Simulation.MINBUFFERSIZE))
            {
                // Calculating the Tick Adjustment Value
                adjustmentValue = Mathf.RoundToInt(Mathf.Max(targetBufferSize, Simulation.MINBUFFERSIZE) - bufferSize);
            }
            else if (bufferSize > Mathf.Min(targetBufferSize, Simulation.MAXBUFFERSIZE))
            {
                // Calculating the Tick Adjustment Value
                adjustmentValue = Mathf.RoundToInt(Mathf.Min(targetBufferSize, Simulation.MAXBUFFERSIZE) - bufferSize);
            }

            // Rounding it up so it isn't that extreme to the client.
            int clampedAdjustmentValue = Mathf.RoundToInt(Mathf.Clamp(adjustmentValue, -Simulation.MAXADJUSTMENTVALUE, Simulation.MAXADJUSTMENTVALUE));

            // Sending tick Adjustment
            AdjustTickRateRPC(clampedAdjustmentValue);

            // Sending network data
            NetworkInfo network = new NetworkInfo();
            network.SetUp(
                short.Parse(clampedAdjustmentValue.ToString()),
                byte.Parse(bufferSize.ToString()),
                byte.Parse(NetworkData.Instance.GetBestBufferSizeForClient(OwnerClientId).ToString()),
                byte.Parse(NetworkData.Instance.GetLatency(OwnerClientId).ToString()),
                byte.Parse(NetworkData.Instance.GetJitter(OwnerClientId).ToString()));

            NetworkDataRPC(network);
        }

        #endregion

        #region RPCs 

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
        public void StartSimulationFromServerTickRPC()
        {
            StartSimulationFromServerTickRPC(NetworkSimulation.CurrentTick);
        }

        [Rpc(SendTo.Owner, Delivery = RpcDelivery.Reliable)]
        public void StartSimulationFromServerTickRPC(uint serverTick)
        {
            NetworkSimulation.Instance.StartSimulation(serverTick + Simulation.TICKOFFSET);
        }

        [Rpc(SendTo.Owner, Delivery = RpcDelivery.Reliable)]
        private void AdjustTickRateRPC(int adjustmentValue)
        {
            // Setting TickRate
            NetworkSimulation.Instance.tickSystem.AdjustTickRate(adjustmentValue);
        }

        [Rpc(SendTo.Owner, Delivery = RpcDelivery.Reliable)]
        private void NetworkDataRPC(NetworkInfo network)
        {
            DebugUI.Instance.UpdateUI(network);
        }

        #endregion
    }
}
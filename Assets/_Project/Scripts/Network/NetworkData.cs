using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Project.Network
{
    [RequireComponent(typeof(UnityTransport))]
    public class NetworkData : Singleton<NetworkData>
    {
        #region Variables

        private Dictionary<ulong, Queue<ulong>> rttValues = new Dictionary<ulong, Queue<ulong>>();

        private UnityTransport transport;

        #endregion

        #region Callbacks

        private void Start()
        {
            transport = GetComponent<UnityTransport>();
        }

        #endregion

        #region Stats

        public void UpdatePlayer(ulong clientId)
        {
            // Creating player if needed.
            if (!rttValues.ContainsKey(clientId))
                rttValues.Add(clientId, new Queue<ulong>());

            if (rttValues[clientId].Count >= Simulation.MAXRTTVALUES)
            {
                rttValues[clientId].Enqueue(GetLatency(clientId));
                rttValues[clientId].Dequeue();
            }
            else rttValues[clientId].Enqueue(GetLatency(clientId));
        }
        public int GetBestBufferSizeForClient(ulong clientId)
        {
            // Buffer Size = (Latency + Jitter) / (1000 / Tick Rate)
            int bufferSize = Mathf.RoundToInt((GetLatency(clientId) + GetJitter(clientId)) / (1000 / NetworkSimulation.Instance.tickSystem.tickRate));
            return bufferSize;
        }
        public ulong GetLatency(ulong clientId) => transport.GetCurrentRtt(clientId);
        public ulong GetJitter(ulong clientId)
        {
            ulong jitter = Simulation.DEFAULTJITTERVALUE;
            if (!rttValues.ContainsKey(clientId)) return jitter;
            if (rttValues[clientId].Count < Simulation.MAXRTTVALUES) return jitter;

            ulong[] rtts = rttValues[clientId].ToArray();

            // Convert ulong to float for jitter calculation
            float rtt0 = (float)rtts[0];
            float rtt1 = (float)rtts[1];
            float rtt2 = (float)rtts[2];

            float diff1 = Mathf.Abs(rtt1 - rtt0);
            float diff2 = Mathf.Abs(rtt2 - rtt1);

            float averageJitter = (diff1 + diff2) / 2;

            // Convert back to ulong if needed, ensuring valid range
            jitter = (ulong)Mathf.Clamp(averageJitter, 0, ulong.MaxValue);

            return jitter;
        }
        public void ResetValues()
        {
            rttValues.Clear();
        }

        #endregion
    }

    #region Classes

    public class NetworkInfo : INetworkSerializable
    {
        public short tickAdjustmentValue;
        public byte bufferSize;
        public byte bestBufferSize;
        public byte rtt;
        public byte jitter;

        public void SetUp(short tickAdjustmentValue, byte bufferSize, byte bestBufferSize, byte rtt, byte jitter)
        {
            this.tickAdjustmentValue = tickAdjustmentValue;
            this.bufferSize = bufferSize;
            this.bestBufferSize = bestBufferSize;
            this.rtt = rtt;
            this.jitter = jitter;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tickAdjustmentValue);
            serializer.SerializeValue(ref bufferSize);
            serializer.SerializeValue(ref bestBufferSize);
            serializer.SerializeValue(ref rtt);
            serializer.SerializeValue(ref jitter);
        }
    }

    #endregion
}
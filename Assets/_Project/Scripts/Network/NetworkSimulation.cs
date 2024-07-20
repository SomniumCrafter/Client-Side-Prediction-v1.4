using System;
using UnityEngine;

namespace Project.Network
{
    [RequireComponent(typeof(TickSystem))]
    public class NetworkSimulation : Singleton<NetworkSimulation>
    {
        #region Variables

        public static Action<uint> OnTick = delegate { };
        public static uint CurrentTick => Instance.tickSystem.currentTick;
        public TickSystem tickSystem { get; private set; }

        #endregion

        #region Callbacks

        private void Start()
        {
            tickSystem = GetComponent<TickSystem>();
            // Freezing Physics
            Physics.simulationMode = SimulationMode.Script;
        }

        private void OnDestroy()
        {
            tickSystem.StopSystem();
        }

        #endregion

        #region Public Methods

        public void StartSimulation(uint startingTick = 0)
        {
            tickSystem.SetTickRate(Simulation.TICKRATE);
            tickSystem.StartSystem(startingTick, true);

            tickSystem.OnTick += OnTick;
        }

        #endregion
    }
}
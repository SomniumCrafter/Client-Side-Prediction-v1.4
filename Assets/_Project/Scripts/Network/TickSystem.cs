using System;
using UnityEngine;

namespace Project.Network
{
    public class TickSystem : MonoBehaviour
    {
        #region Variables

        public Action<uint> OnTick = delegate { };

        public bool isRunning {  get; private set; }
        public uint tickRate { get; private set; }
        public uint currentTick { get; private set; }
        public float startingTimeBetweenTicks { get; private set; }
        public float timeBetweenTicks { get; private set; }
        private float time;
        private bool runPhysics;

        #endregion

        #region Callbacks

        private void Update()
        {
            if (!isRunning) return;

            time += Time.deltaTime;

            if (time >= timeBetweenTicks)
            {
                currentTick++;

                if(runPhysics) Physics.Simulate(startingTimeBetweenTicks);
                OnTick?.Invoke(currentTick);

                time -= timeBetweenTicks;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the current Server tick.
        /// </summary>
        /// <param name="tickRate"></param>
        public void SetTickRate(uint tickRate)
        {
            if (tickRate == 0)
            {
                Debug.LogWarning("Tick Rate not valid!");
                return;
            }

            this.tickRate = tickRate;
            timeBetweenTicks = 1f / tickRate;
        }

        public void AdjustTickRate(int adjustment)
        {
            uint tickRate = uint.Parse((Simulation.TICKRATE + adjustment).ToString());

            SetTickRate(tickRate);
        }

        /// <summary>
        /// Starts the Tick System.
        /// </summary>
        /// <param name="startingTick"></param>
        public void StartSystem(uint startingTick = 0, bool runPhysics = false)
        {
            if (timeBetweenTicks == 0f)
            {
                Debug.LogWarning("Tick Rate not valid!");
                return;
            }

            startingTimeBetweenTicks = timeBetweenTicks;
            currentTick = startingTick;
            time = 0;

            this.runPhysics = runPhysics;
            if (runPhysics)
                Physics.simulationMode = SimulationMode.Script;
            isRunning = true;
        }

        /// <summary>
        /// Stops the Tick System
        /// </summary>
        public void StopSystem()
        {
            currentTick = 0;
            timeBetweenTicks = 0;
            time = 0;

            if (runPhysics)
                Physics.simulationMode = SimulationMode.FixedUpdate;
            isRunning = false;
        }

        #endregion
    }
}
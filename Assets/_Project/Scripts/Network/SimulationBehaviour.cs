using Unity.Netcode;

namespace Project.Network
{
    public class SimulationBehaviour : NetworkBehaviour
    {
        #region Variables

        public uint CurrentTick => NetworkSimulation.CurrentTick;
        public float timeBetweenTicks => NetworkSimulation.Instance.tickSystem.startingTimeBetweenTicks;

        #endregion

        #region Callbacks

        public override void OnNetworkSpawn()
        {
            NetworkSimulation.Instance.tickSystem.OnTick += OnTick;

            OnSpawn();
        }

        public override void OnNetworkDespawn()
        {
            NetworkSimulation.Instance.tickSystem.OnTick -= OnTick;

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

        public virtual void OnTick(uint tick)
        {

        }

        #endregion
    }
}
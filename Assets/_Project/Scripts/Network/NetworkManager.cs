using Unity.Netcode.Transports.UTP;

namespace Project.Network
{
    public class NetworkManager : Singleton<NetworkManager>
    {
        #region Host/Server/Client

        /// <summary>
        /// Starts the Client and connects to the given ip and port
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void StartClient(string ip, string port)
        {
            NetworkData.Instance.ResetValues();

            Unity.Netcode.NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ip, ushort.Parse(port));
            Unity.Netcode.NetworkManager.Singleton.StartClient();
        }

        /// <summary>
        /// Stops the Client
        /// </summary>
        public void StopClient()
        {
            UnityEngine.Debug.LogWarning("Not implemented!");
        }

        /// <summary>
        /// Starts the Host and the Server Tick Simulation
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void StartHost(string ip, string port)
        {
            NetworkData.Instance.ResetValues();

            Unity.Netcode.NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ip, ushort.Parse(port));
            Unity.Netcode.NetworkManager.Singleton.StartHost();
        }

        /// <summary>
        /// Stops Host
        /// </summary>
        public void StopHost()
        {
            UnityEngine.Debug.LogWarning("Not implemented!");
        }

        /// <summary>
        /// Starts the Server and the Server Tick Simulation
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void StartServer(string ip, string port)
        {
            NetworkData.Instance.ResetValues();

            Unity.Netcode.NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ip, ushort.Parse(port));
            Unity.Netcode.NetworkManager.Singleton.StartServer();
        }

        /// <summary>
        /// Stops the Server
        /// </summary>
        public void StopServer()
        {
            UnityEngine.Debug.LogWarning("Not implemented!");
        }

        #endregion
    }
}
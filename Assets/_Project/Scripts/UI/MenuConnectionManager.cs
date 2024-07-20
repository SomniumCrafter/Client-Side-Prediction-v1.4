using Project.Network;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    public class MenuConnectionManager : MonoBehaviour
    {
        #region Variables

        [Header("REFERENCES")]
        [SerializeField] private Button joinGameButton;
        [SerializeField] private Button hostGameButton;
        [Space(5)]
        [SerializeField] private TMP_InputField myNameInputField;
        [SerializeField] private TMP_InputField portInputField;
        [SerializeField] private TMP_InputField ipInputField;
        [Space(5)]
        [SerializeField] private GameObject connectionMenu;

        #endregion

        #region Mono Behaviour Callbacks

        private void Start()
        {
            connectionMenu.SetActive(true);
            CheckForIPAndPort();

            joinGameButton.onClick.RemoveAllListeners();
            joinGameButton.onClick.AddListener(() => { JoinGameButton(myNameInputField.text, ipInputField.text, portInputField.text); });
            hostGameButton.onClick.RemoveAllListeners();
            hostGameButton.onClick.AddListener(() => { HostGameButton(myNameInputField.text, ipInputField.text, portInputField.text); });

            myNameInputField.onValueChanged.AddListener(delegate { CheckForIPAndPort(); });
            portInputField.onValueChanged.AddListener(delegate { CheckForIPAndPort(); });
            ipInputField.onValueChanged.AddListener(delegate { CheckForIPAndPort(); });
        }

        #endregion

        #region Private Methods

        private void CheckForIPAndPort()
        {
            bool connectionDataIsValid = 
                IPChecker.IsValidIP(ipInputField.text) && IPChecker.IsValidPort(portInputField.text) && !string.IsNullOrEmpty(myNameInputField.text);
            joinGameButton.interactable = connectionDataIsValid;
            hostGameButton.interactable = connectionDataIsValid;
        }

        private void JoinGameButton(string name, string ip, string port)
        {
            NetworkManager.Instance.StartClient(ip, port);
            connectionMenu.SetActive(false);
        }

        private void HostGameButton(string name, string ip, string port)
        {
            NetworkManager.Instance.StartHost(ip, port);
            connectionMenu.SetActive(false);
        }

        #endregion
    }

    public class IPChecker
    {
        public static bool IsValidPort(string portString)
        {
            if (int.TryParse(portString, out int port))
            {
                if (port >= 0 && port <= 65535)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsValidIP(string ipString)
        {
            if (IPAddress.TryParse(ipString, out _))
            {
                return true;
            }

            return false;
        }
    }
}
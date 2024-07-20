using Project.Network;
using TMPro;
using UnityEngine;

namespace Project.UI
{
    public class DebugUI : Singleton<DebugUI>
    {
        #region Variables

        [Header("References")]
        [SerializeField] private TMP_Text tickText;
        [SerializeField] private TMP_Text tickRateText;
        [SerializeField] private TMP_Text tickAdjustmentValueText;
        [SerializeField] private TMP_Text bufferSizeText;
        [SerializeField] private TMP_Text bestBufferSizeText;
        [SerializeField] private TMP_Text rttText;
        [SerializeField] private TMP_Text jitterText;

        #endregion

        #region Public Methods

        public void UpdateUI(NetworkInfo network)
        {
            UpdateUI(
                NetworkSimulation.CurrentTick,
                NetworkSimulation.Instance.tickSystem.tickRate,
                network.tickAdjustmentValue,
                network.bufferSize,
                network.bestBufferSize,
                network.rtt,
                network.jitter
                );
        }

        public void UpdateUI(uint tick, uint tickRate, int tickAdjustmentValue, int bufferSize, int bestBufferSize, ulong rtt, ulong jitter)
        {
            tickText.text = "T: " + tick;
            tickRateText.text = "TR: " + tickRate;
            tickAdjustmentValueText.text = "ADJ: " + tickAdjustmentValue;
            bufferSizeText.text = "BS: " + bufferSize;
            bestBufferSizeText.text = "BBS: " + bestBufferSize;
            rttText.text = "RTT: " + rtt;
            jitterText.text = "JT: " + jitter;
        }

        #endregion
    }
}
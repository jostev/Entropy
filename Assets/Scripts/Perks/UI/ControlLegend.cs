using TMPro;
using UnityEngine;

namespace Entropy.Perks.UI
{
    public class ControlLegend : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _legendText;

        [Header("Keybind Labels")]
        [SerializeField] private string _switchDeckLabel = "Arrow Keys";
        [SerializeField] private string _editDeckLabel = "Mouse";
        [SerializeField] private string _viewStatsLabel = "Shift + P";
        [SerializeField] private string _closeLabel = "Escape";

        public void Refresh()
        {
            if (_legendText == null) return;

            _legendText.text =
                $"<b>Controls</b>\n\n" +
                $"<color=#ffcc66>Switch Deck</color>\n{_switchDeckLabel}\n\n" +
                $"<color=#ffcc66>Edit Deck</color>\n{_editDeckLabel}\n\n" +
                $"<color=#ffcc66>View Stats</color>\n{_viewStatsLabel}\n\n" +
                $"<color=#ffcc66>Close Menu</color>\n{_closeLabel}";
        }
    }
}

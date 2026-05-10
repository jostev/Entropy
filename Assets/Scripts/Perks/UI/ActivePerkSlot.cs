using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Entropy.Perks.UI
{
    public class ActivePerkSlot : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private GameObject _emptyOverlay;

        public void SetEmpty()
        {
            if (_iconImage != null) _iconImage.gameObject.SetActive(false);
            if (_nameText != null) _nameText.text = "";
            if (_descriptionText != null) _descriptionText.text = "Empty Slot";
            if (_emptyOverlay != null) _emptyOverlay.SetActive(true);
        }

        public void SetPerk(Entropy.Perks.IPerk perk, PerkDisplayData data)
        {
            if (_iconImage != null)
            {
                _iconImage.gameObject.SetActive(true);
                if (data?.Icon != null) _iconImage.sprite = data.Icon;
            }

            if (_nameText != null)
                _nameText.text = data?.GetDisplayName(perk) ?? perk?.Title ?? "Unknown";

            if (_descriptionText != null)
                _descriptionText.text = data?.GetDisplayDescription(perk) ?? perk?.Description ?? "";

            if (_emptyOverlay != null) _emptyOverlay.SetActive(false);
        }
    }
}

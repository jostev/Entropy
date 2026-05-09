using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Entropy.Perks.UI
{
    public class PerkDebugRow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _countText;
        [SerializeField] private Button _addButton;
        [SerializeField] private Button _removeButton;

        [Header("Config")]
        [SerializeField] private string _perkID;
        [SerializeField] private PerkDebugMenu _menu;

        void Start()
        {
            if (_menu == null)
                _menu = FindAnyObjectByType<PerkDebugMenu>();

            if (!string.IsNullOrEmpty(_perkID) && _menu != null)
                Setup(_perkID, _menu);
        }

        public void Setup(string perkID, PerkDebugMenu menu)
        {
            _perkID = perkID;
            _menu = menu;
            Refresh();
        }

        public void Refresh()
        {
            if (_menu == null) return;
            _nameText.text = _menu.GetPerkName(_perkID);
            _countText.text = _menu.GetStackCount(_perkID).ToString();
        }

        void OnEnable()
        {
            _addButton?.onClick.AddListener(OnAdd);
            _removeButton?.onClick.AddListener(OnRemove);
        }

        void OnDisable()
        {
            _addButton?.onClick.RemoveListener(OnAdd);
            _removeButton?.onClick.RemoveListener(OnRemove);
        }

        void OnAdd() => _menu?.AddPerk(_perkID);
        void OnRemove() => _menu?.RemovePerk(_perkID);
    }
}

using System.Collections.Generic;
using Entropy.Perks;
using Entropy.Perks.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Entropy.Player
{
    public class DeathPerkSelectorUI : MonoBehaviour
    {
        private PerkUIDatabase _database;

        [Header("Visuals")]
        [SerializeField] private Color _commonColor = new Color(0.75f, 0.75f, 0.75f);
        [SerializeField] private Color _uncommonColor = new Color(0.4f, 0.9f, 0.4f);
        [SerializeField] private Color _rareColor = new Color(0.3f, 0.6f, 1f);
        [SerializeField] private Color _epicColor = new Color(0.7f, 0.3f, 1f);
        [SerializeField] private Color _legendaryColor = new Color(1f, 0.7f, 0.2f);
        [SerializeField] private Color _mythicColor = new Color(1f, 0.2f, 0.4f);

        private Canvas _canvas;
        private RectTransform _cardContainer;
        private System.Action<string> _onSelected;
        private readonly List<GameObject> _cardObjects = new();

        void Awake()
        {
            _database = Resources.Load<PerkUIDatabase>("PerkUIDatabase");
            if (_database == null)
                _database = Object.FindAnyObjectByType<PerkUIDatabase>();

            BuildUI();
            if (_canvas != null)
                _canvas.enabled = false;
        }

        public void Show(List<PerkBase> perks, System.Action<string> onSelected)
        {
            _onSelected = onSelected;

            foreach (var go in _cardObjects)
            {
                if (go != null) Destroy(go);
            }
            _cardObjects.Clear();

            if (_canvas != null)
                _canvas.enabled = true;

            if (perks == null || perks.Count == 0)
            {
                BuildNoPerksMessage();
                return;
            }

            Debug.Log($"[DeathPerkSelectorUI] Showing {perks.Count} perks");

            foreach (var perk in perks)
            {
                if (perk == null) continue;
                BuildCard(perk);
            }
        }

        private void BuildNoPerksMessage()
        {
            var go = new GameObject("NoPerks", typeof(Text));
            go.transform.SetParent(_cardContainer, false);
            _cardObjects.Add(go);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;

            var txt = go.GetComponent<Text>();
            txt.text = "No perks available.\nClick anywhere to respawn.";
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 28;
            txt.color = Color.gray;
            txt.alignment = TextAnchor.MiddleCenter;

            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => _onSelected?.Invoke(null));
        }

        public void Hide()
        {
            if (_canvas != null)
                _canvas.enabled = false;

            foreach (var go in _cardObjects)
            {
                if (go != null) Destroy(go);
            }
            _cardObjects.Clear();
        }

        private void BuildUI()
        {
            var existingEventSystem = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (existingEventSystem == null)
            {
                var esGo = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
                esGo.transform.SetParent(null);
            }

            var go = new GameObject("DeathPerkCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvas = go.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var bg = new GameObject("Background", typeof(Image));
            bg.transform.SetParent(go.transform, false);
            var bgImg = bg.GetComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.85f);
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            var title = new GameObject("Title", typeof(Text));
            title.transform.SetParent(go.transform, false);
            var titleText = title.GetComponent<Text>();
            titleText.text = "Choose a Perk to Resurrect";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 48;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleCenter;
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.75f);
            titleRect.anchorMax = new Vector2(1, 0.9f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            _cardContainer = new GameObject("CardContainer", typeof(RectTransform)).GetComponent<RectTransform>();
            _cardContainer.SetParent(go.transform, false);
            _cardContainer.anchorMin = new Vector2(0.1f, 0.2f);
            _cardContainer.anchorMax = new Vector2(0.9f, 0.7f);
            _cardContainer.offsetMin = Vector2.zero;
            _cardContainer.offsetMax = Vector2.zero;

            var layout = _cardContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 32;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
        }

        private void BuildCard(PerkBase perk)
        {
            var go = new GameObject($"Card_{perk.ID}", typeof(Image), typeof(Button), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            go.transform.SetParent(_cardContainer, false);
            _cardObjects.Add(go);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var le = go.GetComponent<LayoutElement>();
            le.flexibleWidth = 1;
            le.minWidth = 200;
            le.minHeight = 150;

            var img = go.GetComponent<Image>();
            img.color = new Color(0.12f, 0.12f, 0.14f, 1f);

            var border = new GameObject("Border", typeof(Image));
            border.transform.SetParent(go.transform, false);
            var borderImg = border.GetComponent<Image>();
            borderImg.color = GetRarityColor(perk.Rarity);
            borderImg.raycastTarget = false;
            var borderRect = border.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-4, -4);
            borderRect.offsetMax = new Vector2(4, 4);

            var data = _database?.Get(perk.ID);
            string title = data?.GetDisplayName(perk) ?? perk.Title;
            string desc = data?.GetDisplayDescription(perk) ?? perk.Description;

            var nameObj = new GameObject("Name", typeof(Text));
            nameObj.transform.SetParent(go.transform, false);
            var nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.7f);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            var nameText = nameObj.GetComponent<Text>();
            nameText.text = $"[{perk.Rarity}] {title}";
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 24;
            nameText.color = Color.white;
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.raycastTarget = false;

            var descObj = new GameObject("Desc", typeof(Text));
            descObj.transform.SetParent(go.transform, false);
            var descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0);
            descRect.anchorMax = new Vector2(1, 0.65f);
            descRect.offsetMin = new Vector2(8, 8);
            descRect.offsetMax = new Vector2(-8, -4);
            var descText = descObj.GetComponent<Text>();
            descText.text = desc;
            descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            descText.fontSize = 18;
            descText.color = new Color(0.8f, 0.8f, 0.8f);
            descText.alignment = TextAnchor.MiddleCenter;
            descText.raycastTarget = false;

            var btn = go.GetComponent<Button>();
            btn.interactable = true;
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.25f, 0.25f, 0.28f);
            colors.pressedColor = new Color(0.3f, 0.3f, 0.35f);
            colors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            btn.colors = colors;

            string capturedID = perk.ID;
            btn.onClick.AddListener(() => {
                Debug.Log($"[DeathPerkSelectorUI] Clicked perk: {capturedID}");
                _onSelected?.Invoke(capturedID);
            });

            var vlg = go.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(12, 12, 12, 12);
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
        }

        private Color GetRarityColor(PerkRarity rarity)
        {
            return rarity switch
            {
                PerkRarity.Common => _commonColor,
                PerkRarity.Uncommon => _uncommonColor,
                PerkRarity.Rare => _rareColor,
                PerkRarity.Epic => _epicColor,
                PerkRarity.Legendary => _legendaryColor,
                PerkRarity.Mythic => _mythicColor,
                _ => Color.gray
            };
        }
    }
}

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
        private System.Action _onRefresh;

        void Awake()
        {
            _database = Resources.Load<PerkUIDatabase>("PerkUIDatabase");
            if (_database == null)
                _database = Object.FindAnyObjectByType<PerkUIDatabase>();

            BuildUI();
            if (_canvas != null)
                _canvas.enabled = false;
        }

        public void Show(List<PerkBase> perks, System.Action<string> onSelected, System.Action onRefresh)
        {
            _onSelected = onSelected;
            _onRefresh = onRefresh;
            ClearCards();

            if (_canvas != null)
                _canvas.enabled = true;

            if (perks == null || perks.Count == 0)
            {
                BuildNoPerksFallback();
                return;
            }

            foreach (var perk in perks)
            {
                if (perk == null) continue;
                BuildCard(perk);
            }

            BuildRefreshButton();
        }

        public void Hide()
        {
            if (_canvas != null)
                _canvas.enabled = false;
            ClearCards();
        }

        private void ClearCards()
        {
            foreach (var go in _cardObjects)
            {
                if (go != null) Destroy(go);
            }
            _cardObjects.Clear();
        }

        private void BuildUI()
        {
            EnsureEventSystem();

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
            _cardContainer.anchorMin = new Vector2(0.1f, 0.25f);
            _cardContainer.anchorMax = new Vector2(0.9f, 0.65f);
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

        private void EnsureEventSystem()
        {
            var existing = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (existing != null) return;

            var esGo = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        private void BuildCard(PerkBase perk)
        {
            var card = new GameObject($"Card_{perk.ID}", typeof(Image), typeof(LayoutElement));
            card.transform.SetParent(_cardContainer, false);
            _cardObjects.Add(card);

            var cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = Vector2.zero;
            cardRect.anchorMax = Vector2.one;
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;

            var cardImg = card.GetComponent<Image>();
            cardImg.color = new Color(0.12f, 0.12f, 0.14f, 1f);

            var le = card.GetComponent<LayoutElement>();
            le.flexibleWidth = 1;
            le.minWidth = 200;
            le.minHeight = 200;

            var border = new GameObject("Border", typeof(Image));
            border.transform.SetParent(card.transform, false);
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
            nameObj.transform.SetParent(card.transform, false);
            var nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.72f);
            nameRect.anchorMax = new Vector2(0.95f, 0.95f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            var nameText = nameObj.GetComponent<Text>();
            nameText.text = $"[{perk.Rarity}] {title}";
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 20;
            nameText.color = GetRarityColor(perk.Rarity);
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.raycastTarget = false;

            var descObj = new GameObject("Desc", typeof(Text));
            descObj.transform.SetParent(card.transform, false);
            var descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.05f, 0.25f);
            descRect.anchorMax = new Vector2(0.95f, 0.68f);
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = Vector2.zero;
            var descText = descObj.GetComponent<Text>();
            descText.text = desc;
            descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            descText.fontSize = 16;
            descText.color = new Color(0.8f, 0.8f, 0.8f);
            descText.alignment = TextAnchor.MiddleCenter;
            descText.raycastTarget = false;

            var pickBtn = new GameObject("PickButton", typeof(Image), typeof(Button));
            pickBtn.transform.SetParent(card.transform, false);
            var pickRect = pickBtn.GetComponent<RectTransform>();
            pickRect.anchorMin = new Vector2(0.15f, 0.06f);
            pickRect.anchorMax = new Vector2(0.85f, 0.20f);
            pickRect.offsetMin = Vector2.zero;
            pickRect.offsetMax = Vector2.zero;

            var pickImg = pickBtn.GetComponent<Image>();
            pickImg.color = new Color(0.2f, 0.6f, 0.3f, 1f);

            var pickBtnComp = pickBtn.GetComponent<Button>();
            pickBtnComp.interactable = true;
            pickBtnComp.targetGraphic = pickImg;

            var pickColors = pickBtnComp.colors;
            pickColors.normalColor = new Color(0.2f, 0.6f, 0.3f, 1f);
            pickColors.highlightedColor = new Color(0.3f, 0.7f, 0.4f, 1f);
            pickColors.pressedColor = new Color(0.15f, 0.5f, 0.25f, 1f);
            pickBtnComp.colors = pickColors;

            string capturedID = perk.ID;
            pickBtnComp.onClick.AddListener(() => {
                Debug.Log($"[DeathPerkSelectorUI] Picked perk: {capturedID}");
                _onSelected?.Invoke(capturedID);
            });

            var pickLabel = new GameObject("Label", typeof(Text));
            pickLabel.transform.SetParent(pickBtn.transform, false);
            var pickLabelRect = pickLabel.GetComponent<RectTransform>();
            pickLabelRect.anchorMin = Vector2.zero;
            pickLabelRect.anchorMax = Vector2.one;
            pickLabelRect.offsetMin = Vector2.zero;
            pickLabelRect.offsetMax = Vector2.zero;
            var pickLabelText = pickLabel.GetComponent<Text>();
            pickLabelText.text = "Pick";
            pickLabelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            pickLabelText.fontSize = 20;
            pickLabelText.color = Color.white;
            pickLabelText.alignment = TextAnchor.MiddleCenter;
            pickLabelText.raycastTarget = false;
        }

        private void BuildRefreshButton()
        {
            var refresh = new GameObject("RefreshButton", typeof(Image), typeof(Button));
            refresh.transform.SetParent(_canvas.transform, false);
            _cardObjects.Add(refresh);

            var refreshRect = refresh.GetComponent<RectTransform>();
            refreshRect.anchorMin = new Vector2(0.4f, 0.12f);
            refreshRect.anchorMax = new Vector2(0.6f, 0.18f);
            refreshRect.offsetMin = Vector2.zero;
            refreshRect.offsetMax = Vector2.zero;

            var refreshImg = refresh.GetComponent<Image>();
            refreshImg.color = new Color(0.3f, 0.3f, 0.35f, 1f);

            var refreshBtn = refresh.GetComponent<Button>();
            refreshBtn.interactable = true;
            refreshBtn.targetGraphic = refreshImg;

            var colors = refreshBtn.colors;
            colors.normalColor = new Color(0.3f, 0.3f, 0.35f, 1f);
            colors.highlightedColor = new Color(0.4f, 0.4f, 0.45f, 1f);
            colors.pressedColor = new Color(0.25f, 0.25f, 0.3f, 1f);
            refreshBtn.colors = colors;

            refreshBtn.onClick.AddListener(() => {
                Debug.Log("[DeathPerkSelectorUI] Refresh clicked");
                _onRefresh?.Invoke();
            });

            var label = new GameObject("Label", typeof(Text));
            label.transform.SetParent(refresh.transform, false);
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var labelText = label.GetComponent<Text>();
            labelText.text = "Refresh Choices";
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 22;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.raycastTarget = false;
        }

        private void BuildNoPerksFallback()
        {
            var go = new GameObject("NoPerks", typeof(Text));
            go.transform.SetParent(_cardContainer, false);
            _cardObjects.Add(go);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;

            var txt = go.GetComponent<Text>();
            txt.text = "No perks available.\nClick to respawn.";
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 28;
            txt.color = Color.gray;
            txt.alignment = TextAnchor.MiddleCenter;

            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => _onSelected?.Invoke(null));
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

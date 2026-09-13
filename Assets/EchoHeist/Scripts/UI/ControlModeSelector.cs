using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EchoHeist
{
    public sealed class ControlModeSelector : MonoBehaviour
    {
        public static bool IsMobileMode { get; private set; }

        private GameObject _selectionOverlay;
        
        private GameObject _rotateDeviceOverlay;
private GameObject _mobileControls;
        private RunManager _runManager;
        private GadgetController _gadgetController;
        private GadgetSelectionUI _gadgetSelectionUI;
        private PrototypeHUD _hud;
        private GameObject _controlsHint;
        private RectTransform _gadgetText;
        private Button _abilityButton;
        private TMP_Text _abilityButtonLabel;




        private static readonly Color Dark = new Color(0.035f, 0.055f, 0.075f, 0.97f);
        private static readonly Color Panel = new Color(0.075f, 0.11f, 0.15f, 0.98f);
        private static readonly Color Cyan = new Color(0.1f, 0.82f, 0.95f, 1f);
        private static readonly Color Blue = new Color(0.11f, 0.42f, 0.68f, 1f);

        private void Awake()
        {
            _runManager = FindFirstObjectByType<RunManager>();
            _gadgetController = FindFirstObjectByType<GadgetController>();
            _gadgetSelectionUI = FindFirstObjectByType<GadgetSelectionUI>();
            _hud = FindFirstObjectByType<PrototypeHUD>();
            _controlsHint = GameObject.Find("ControlsHint");
            _gadgetText = transform.Find("GadgetText")?.GetComponent<RectTransform>();


            BuildSelectionOverlay();
            
            BuildOrientationOverlay();
BuildMobileControls();
        }

        private void Start()
        {
            _selectionOverlay.SetActive(true);
            _selectionOverlay.transform.SetAsLastSibling();
            _mobileControls.SetActive(false);
            Time.timeScale = 0f;
        }

private void Update()
        {
            RefreshOrientationOverlay();
            if (_rotateDeviceOverlay != null && _rotateDeviceOverlay.activeSelf) return;
            if (!IsMobileMode || _selectionOverlay.activeSelf) return;

            bool gameMenuOpen = _gadgetSelectionUI != null && _gadgetSelectionUI.IsOpen;
            if (_mobileControls.activeSelf == gameMenuOpen)
            {
                _mobileControls.SetActive(!gameMenuOpen);
            }

            if (!gameMenuOpen) RefreshAbilityButton();
        }

        private void ChooseDesktop()
        {
            IsMobileMode = false;
            _controlsHint?.SetActive(true);
            _mobileControls.SetActive(false);
            CloseSelection();
        }

private void ChooseMobile()
        {
            IsMobileMode = true;
            _controlsHint?.SetActive(false);
            if (_gadgetText != null)
            {
                _gadgetText.anchorMin = new Vector2(0.5f, 1f);
                _gadgetText.anchorMax = new Vector2(0.5f, 1f);
                _gadgetText.pivot = new Vector2(0.5f, 1f);
                _gadgetText.anchoredPosition = new Vector2(0f, -12f);
            }

            bool gameMenuOpen = _gadgetSelectionUI != null && _gadgetSelectionUI.IsOpen;
            _mobileControls.SetActive(!gameMenuOpen);
            CloseSelection();
            RefreshOrientationOverlay();
        }

        private void CloseSelection()
        {
            _selectionOverlay.SetActive(false);
            Time.timeScale = _gadgetSelectionUI != null && _gadgetSelectionUI.IsOpen ? 0f : 1f;
        }

private void RefreshOrientationOverlay()
        {
            if (_rotateDeviceOverlay == null) return;

            bool shouldShow = IsMobileMode && Screen.height > Screen.width;
            if (_rotateDeviceOverlay.activeSelf == shouldShow) return;

            _rotateDeviceOverlay.SetActive(shouldShow);
            if (shouldShow)
            {
                _rotateDeviceOverlay.transform.SetAsLastSibling();
                Time.timeScale = 0f;
            }
            else if (!_selectionOverlay.activeSelf &&
                     (_gadgetSelectionUI == null || !_gadgetSelectionUI.IsOpen))
            {
                Time.timeScale = 1f;
            }
        }


        private void BuildSelectionOverlay()
        {
            _selectionOverlay = CreateRect("ControlModeSelection", transform, Vector2.zero, Vector2.zero);
            StretchFullScreen(_selectionOverlay.GetComponent<RectTransform>());
            _selectionOverlay.AddComponent<Image>().color = Dark;

            GameObject card = CreateRect("Card", _selectionOverlay.transform, new Vector2(760f, 430f), Vector2.zero);
            card.AddComponent<Image>().color = Panel;

            CreateText("ECHO HEIST", card.transform, new Vector2(650f, 70f), new Vector2(0f, 135f), 42f, FontStyles.Bold);
            CreateText("CHOOSE HOW YOU WANT TO PLAY", card.transform, new Vector2(650f, 45f), new Vector2(0f, 78f), 20f, FontStyles.Bold, Cyan);
            CreateText("You can reload the page later to choose again.", card.transform, new Vector2(650f, 38f), new Vector2(0f, 38f), 16f, FontStyles.Normal, Color.white);

            CreateButton("DesktopControls", card.transform, "DESKTOP CONTROLS\nKeyboard", new Vector2(285f, 105f), new Vector2(-160f, -65f), ChooseDesktop, Blue);
            CreateButton("MobileControls", card.transform, "MOBILE CONTROLS\nTouch joystick + buttons", new Vector2(285f, 105f), new Vector2(160f, -65f), ChooseMobile, Cyan);
        }

private void BuildOrientationOverlay()
        {
            _rotateDeviceOverlay = CreateRect("RotateDeviceOverlay", transform, Vector2.zero, Vector2.zero);
            StretchFullScreen(_rotateDeviceOverlay.GetComponent<RectTransform>());
            _rotateDeviceOverlay.AddComponent<Image>().color = Dark;

            GameObject card = CreateRect("RotateCard", _rotateDeviceOverlay.transform, new Vector2(760f, 280f), Vector2.zero);
            card.AddComponent<Image>().color = Panel;

            TMP_Text title = CreateText("RotateTitle", card.transform, new Vector2(680f, 100f),
                new Vector2(0f, 36f), 34f, FontStyles.Bold, Cyan);
            title.text = "PLEASE ROTATE YOUR PHONE\nTO LANDSCAPE";

            TMP_Text message = CreateText("RotateMessage", card.transform, new Vector2(680f, 48f),
                new Vector2(0f, -62f), 18f, FontStyles.Normal, Color.white);
            message.text = "The game will continue automatically.";

            _rotateDeviceOverlay.SetActive(false);
        }


private void BuildMobileControls()
        {
            _mobileControls = CreateRect("MobileControlOverlay", transform, Vector2.zero, Vector2.zero);
            StretchFullScreen(_mobileControls.GetComponent<RectTransform>());
            _mobileControls.transform.SetAsLastSibling();

            GameObject joystickBase = CreateRect("MoveJoystick", _mobileControls.transform, new Vector2(190f, 190f), new Vector2(135f, 135f));
            RectTransform baseRect = joystickBase.GetComponent<RectTransform>();
            baseRect.anchorMin = Vector2.zero;
            baseRect.anchorMax = Vector2.zero;
            joystickBase.AddComponent<Image>().color = new Color(0.05f, 0.12f, 0.17f, 0.72f);

            GameObject knob = CreateRect("Knob", joystickBase.transform, new Vector2(78f, 78f), Vector2.zero);
            knob.AddComponent<Image>().color = new Color(Cyan.r, Cyan.g, Cyan.b, 0.9f);
            joystickBase.AddComponent<MobileJoystick>().Configure(knob.GetComponent<RectTransform>(), 62f);

            Vector2 buttonSize = new Vector2(132f, 64f);
            Vector2 bottomRight = new Vector2(1f, 0f);
            CreateAnchoredButton("CommitButton", _mobileControls.transform, "SAVE ECHO", buttonSize, new Vector2(-360f, 52f),
                () => _runManager?.CommitTimeline(), Blue, bottomRight);
            _abilityButton = CreateAnchoredButton("AbilityButton", _mobileControls.transform, "NO ABILITY", buttonSize, new Vector2(-215f, 52f),
                () => _gadgetController?.UseSelectedGadget(), Cyan, bottomRight);
            _abilityButtonLabel = _abilityButton.GetComponentInChildren<TMP_Text>();
            CreateAnchoredButton("ResetButton", _mobileControls.transform, "RESTART", buttonSize, new Vector2(-70f, 52f),
                () => _runManager?.ResetCurrentRun(), new Color(0.42f, 0.18f, 0.2f, 0.9f), bottomRight);
            RefreshAbilityButton();
        }

private void RefreshAbilityButton()
        {
            if (_abilityButton == null || _abilityButtonLabel == null || _gadgetController == null) return;

            GadgetType selected = _gadgetController.SelectedGadget;
            _abilityButton.interactable = selected != GadgetType.None;
            switch (selected)
            {
                case GadgetType.PhaseDash:
                    _abilityButtonLabel.text = "DASH";
                    break;
                case GadgetType.GhostDecoy:
                    _abilityButtonLabel.text = "DECOY";
                    break;
                case GadgetType.OpticalCloak:
                    _abilityButtonLabel.text = "CLOAK";
                    break;
                default:
                    _abilityButtonLabel.text = "NO ABILITY";
                    break;
            }
        }


        private static GameObject CreateRect(string name, Transform parent, Vector2 size, Vector2 position)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return go;
        }

        private static void StretchFullScreen(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static TMP_Text CreateText(string name, Transform parent, Vector2 size, Vector2 position,
            float fontSize, FontStyles style, Color? color = null)
        {
            GameObject go = CreateRect(name, parent, size, position);
            TMP_Text text = go.AddComponent<TextMeshProUGUI>();
            text.text = name;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color ?? Color.white;
            text.alignment = TextAlignmentOptions.Center;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 size,
            Vector2 position, UnityAction action, Color color)
        {
            GameObject go = CreateRect(name, parent, size, position);
            Image image = go.AddComponent<Image>();
            image.color = color;
            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            TMP_Text text = CreateText(label, go.transform, size - new Vector2(18f, 14f), Vector2.zero, 20f, FontStyles.Bold);
            text.text = label;
            return button;
        }

private static Button CreateAnchoredButton(string name, Transform parent, string label, Vector2 size,
            Vector2 position, UnityAction action, Color color, Vector2 anchor)
        {
            Button button = CreateButton(name, parent, label, size, position, action, color);
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;

            TMP_Text labelText = button.GetComponentInChildren<TMP_Text>();
            if (labelText != null) labelText.fontSize = 16f;
            return button;
        }
    }
}
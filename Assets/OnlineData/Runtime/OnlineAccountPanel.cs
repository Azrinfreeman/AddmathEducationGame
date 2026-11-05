using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MathFun.OnlineData
{
    /// <summary>
    /// A simple sign-in screen for the adult. It builds itself from code (no scene or prefab editing)
    /// and appears as a small "Online" button in the corner of the MainMenu scene.
    /// To change its look, edit the numbers and colours in the Build... methods below.
    /// </summary>
    public sealed class OnlineAccountPanel : MonoBehaviour
    {
        private const string SceneName = "MainMenu";

        private static readonly Color PanelColor = new Color(0.12f, 0.16f, 0.28f, 0.97f);
        private static readonly Color ButtonColor = new Color(0.25f, 0.55f, 0.95f, 1f);
        private static readonly Color DangerColor = new Color(0.85f, 0.35f, 0.35f, 1f);

        private PlayerDataService service;
        private GameObject panel;
        private Text statusText;
        private Text messageText;
        private InputField usernameField;
        private InputField passwordField;
        private Button signInButton, registerButton, uploadButton, signOutButton;
        private Text uploadLabel;
        private bool uploadArmed;
        private Font font;
        private RectTransform panelBox, gateBox;
        private Image statusDot; // small light on the Online button: green synced, amber waiting, red needs attention, grey local only

        // Parent gate: a small maths question before the panel opens (a speed bump for children, not security).
        private GameObject gate;
        private Text gateQuestion;
        private Button[] gateAnswers;

        // ------------------------------------------------------------------
        // Automatic start: create the panel when MainMenu loads.
        // ------------------------------------------------------------------

        private static bool hooked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Hook()
        {
            // Touch the service now so old data is converted and mirrored every session.
            PlayerDataService.Instance.MirrorLegacyPlayerPrefs();
            if (!hooked)
            {
                hooked = true;
                SceneManager.sceneLoaded += (scene, mode) => SpawnIfMainMenu(scene);
            }
            SpawnIfMainMenu(SceneManager.GetActiveScene());
        }

        private static void SpawnIfMainMenu(Scene scene)
        {
#if UNITY_WEBGL
            return; // Web demo mode: no Online button (see ApiConfiguration.LoadOrDisabled).
#else
            if (scene.name != SceneName || FindFirstObjectByType<OnlineAccountPanel>() != null) return;
            new GameObject("OnlineAccountPanel").AddComponent<OnlineAccountPanel>();
#endif
        }

        private void Start()
        {
            service = PlayerDataService.Instance;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            EnsureEventSystem();
            Canvas canvas = BuildCanvas();
            BuildOpenButton(canvas.transform);
            BuildPanel(canvas.transform);
            BuildGate(canvas.transform);
            panel.SetActive(false);
            gate.SetActive(false);
            service.Changed += Refresh;
        }

        private void OnDestroy()
        {
            if (service != null) service.Changed -= Refresh;
        }

        // ------------------------------------------------------------------
        // Text (Malay by default, English when the game language is English)
        // ------------------------------------------------------------------

        private static bool English { get { return PlayerPrefs.GetInt("EnglishLang", 0) == 1; } }
        private static string T(string english, string malay) { return English ? english : malay; }

        private string DescribeError(string code)
        {
            switch (code)
            {
                case "offline": return T("No internet connection.", "Tiada sambungan internet.");
                case "invalid_credentials": return T("Wrong username or password.", "Nama pengguna atau kata laluan salah.");
                case "username_taken": return T("That username is already used.", "Nama pengguna itu telah digunakan.");
                case "invalid_value": return T("Username: 3-30 letters/numbers. Password: 8+ characters.", "Nama pengguna: 3-30 huruf/nombor. Kata laluan: 8+ aksara.");
                case "rate_limited": return T("Too many tries. Wait a while.", "Terlalu banyak cubaan. Tunggu sebentar.");
                default: return T("Something went wrong (", "Ada masalah (") + code + ")";
            }
        }

        private string DescribeStatus()
        {
            if (!service.IsAccountWorkspace)
                return T("Not signed in. Progress stays on this device.", "Belum log masuk. Kemajuan kekal pada peranti ini.");

            string who = T("Account: ", "Akaun: ") + service.AccountName + "\n";
            switch (service.State)
            {
                case SyncState.Synced: return who + T("All saved online.", "Semua disimpan dalam talian.");
                case SyncState.SignInRequired:
                    return who + (service.PendingCount > 0
                        ? T("Sign in again to upload ", "Log masuk semula untuk muat naik ") + service.PendingCount
                        : T("Sign in again to sync.", "Log masuk semula untuk menyegerak."));
                case SyncState.Offline: return who + T("Offline. Waiting to upload: ", "Luar talian. Menunggu muat naik: ") + service.PendingCount;
                case SyncState.NeedsAttention: return who + T("Some changes were refused by the server: ", "Sesetengah perubahan ditolak pelayan: ") + service.RejectedCount;
                default: return who;
            }
        }

        // ------------------------------------------------------------------
        // Actions
        // ------------------------------------------------------------------

        private async void OnSignIn() { await RunAuth(false); }
        private async void OnRegister() { await RunAuth(true); }

        private async System.Threading.Tasks.Task RunAuth(bool register)
        {
            messageText.text = T("Please wait...", "Sila tunggu...");
            string user = usernameField.text.Trim();
            string password = passwordField.text;
            string error = register ? await service.RegisterAsync(user, password) : await service.SignInAsync(user, password);
            messageText.text = error == null ? T("Signed in.", "Berjaya log masuk.") : DescribeError(error);
            if (error == null) passwordField.text = "";
            Refresh();
        }

        private void OnUpload()
        {
            int waiting = LegacyPlayerPrefsImporter.ReadPlayers().Count;
            if (!uploadArmed)
            {
                uploadArmed = true;
                uploadLabel.text = T("Tap again: upload ", "Tekan lagi: muat naik ") + waiting + T(" player(s)?", " pemain?");
                return;
            }
            uploadArmed = false;
            int moved = service.TransferGuestPlayersToAccount();
            messageText.text = T("Uploading players: ", "Memuat naik pemain: ") + moved;
            Refresh();
        }

        private async void OnSignOut()
        {
            bool done = await service.SignOutAsync(false);
            if (!done)
            {
                messageText.text = T("Changes are still waiting to upload. Connect to the internet first.",
                    "Masih ada perubahan belum dimuat naik. Sambung ke internet dahulu.");
            }
            Refresh();
        }

        private void Refresh()
        {
            UpdateStatusDot();
            if (panel == null) return;
            bool signedInAccount = service.IsAccountWorkspace;
            statusText.text = DescribeStatus();
            signInButton.gameObject.SetActive(!signedInAccount || service.State == SyncState.SignInRequired);
            registerButton.gameObject.SetActive(!signedInAccount);
            usernameField.gameObject.SetActive(!signedInAccount || service.State == SyncState.SignInRequired);
            passwordField.gameObject.SetActive(!signedInAccount || service.State == SyncState.SignInRequired);
            uploadButton.gameObject.SetActive(signedInAccount && service.IsSignedIn);
            signOutButton.gameObject.SetActive(signedInAccount);
            if (!uploadArmed) uploadLabel.text = T("Upload players on this device", "Muat naik pemain pada peranti ini");
            if (signedInAccount && usernameField.text.Length == 0) usernameField.text = service.AccountName;
        }

        // ------------------------------------------------------------------
        // Building the screen
        // ------------------------------------------------------------------

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            GameObject system = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            system.transform.SetParent(null);
        }

        private Canvas BuildCanvas()
        {
            GameObject host = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            host.transform.SetParent(transform, false);
            Canvas canvas = host.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            CanvasScaler scaler = host.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private void BuildOpenButton(Transform parent)
        {
            Button open = MakeButton(parent, "OpenButton", T("Online", "Dalam talian"), ButtonColor, 30);
            RectTransform rect = (RectTransform)open.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-30, -130);
            rect.sizeDelta = new Vector2(260, 80);
            open.onClick.AddListener(ShowGate);

            GameObject dot = new GameObject("StatusDot", typeof(Image));
            dot.transform.SetParent(open.transform, false);
            statusDot = dot.GetComponent<Image>();
            statusDot.raycastTarget = false;
            RectTransform dotRect = (RectTransform)dot.transform;
            dotRect.anchorMin = dotRect.anchorMax = dotRect.pivot = new Vector2(1, 1);
            dotRect.anchoredPosition = new Vector2(-6, -6);
            dotRect.sizeDelta = new Vector2(24, 24);
            UpdateStatusDot();
        }

        private void UpdateStatusDot()
        {
            if (statusDot == null) return;
            Color color = new Color(0.6f, 0.6f, 0.6f);                        // not signed in
            if (service.IsAccountWorkspace)
            {
                switch (service.State)
                {
                    case SyncState.Synced: color = new Color(0.25f, 0.75f, 0.4f); break;
                    case SyncState.NeedsAttention: color = new Color(0.9f, 0.25f, 0.25f); break;
                    default: color = new Color(1f, 0.75f, 0.2f); break;       // offline / sign in again
                }
            }
            statusDot.color = color;
        }

        private void BuildPanel(Transform parent)
        {
            panel = new GameObject("Panel", typeof(Image));
            panel.transform.SetParent(parent, false);
            Image backdrop = panel.GetComponent<Image>();
            backdrop.color = new Color(0, 0, 0, 0.6f);
            Stretch((RectTransform)panel.transform);

            GameObject box = new GameObject("Box", typeof(Image));
            box.transform.SetParent(panel.transform, false);
            box.GetComponent<Image>().color = PanelColor;
            RectTransform boxRect = (RectTransform)box.transform;
            boxRect.anchorMin = boxRect.anchorMax = boxRect.pivot = new Vector2(0.5f, 0.5f);
            boxRect.sizeDelta = new Vector2(780, 820);
            panelBox = boxRect;

            MakeLabel(box.transform, T("Online account (for parents)", "Akaun dalam talian (untuk ibu bapa)"), 34, 355, 80, Color.white);
            statusText = MakeLabel(box.transform, "", 28, 250, 120, new Color(0.8f, 0.9f, 1f));
            usernameField = MakeInput(box.transform, T("Username", "Nama pengguna"), 110, false);
            passwordField = MakeInput(box.transform, T("Password", "Kata laluan"), 10, true);
            signInButton = MakeAction(box.transform, T("Sign in", "Log masuk"), -95, ButtonColor, OnSignIn);
            registerButton = MakeAction(box.transform, T("Create account", "Cipta akaun"), -195, ButtonColor, OnRegister);
            uploadButton = MakeAction(box.transform, "", -95, ButtonColor, OnUpload);
            uploadLabel = uploadButton.GetComponentInChildren<Text>();
            signOutButton = MakeAction(box.transform, T("Sign out", "Log keluar"), -195, DangerColor, OnSignOut);
            messageText = MakeLabel(box.transform, "", 26, -285, 80, new Color(1f, 0.9f, 0.5f));
            MakeAction(box.transform, T("Close", "Tutup"), -365, new Color(0.35f, 0.4f, 0.5f), () => panel.SetActive(false));
        }

        private void ShowGate()
        {
            int a = Random.Range(3, 9), b = Random.Range(3, 9);
            int correct = a * b;
            gateQuestion.text = T("Grown-ups only. What is ", "Untuk dewasa sahaja. Berapakah ") + a + " x " + b + "?";

            int correctSlot = Random.Range(0, gateAnswers.Length);
            for (int i = 0; i < gateAnswers.Length; i++)
            {
                int value = i == correctSlot ? correct : correct + (i + 1) * (Random.value < 0.5f ? 1 : 2) * (i % 2 == 0 ? 1 : -1);
                if (i != correctSlot && value == correct) value++;
                bool isRight = i == correctSlot;
                gateAnswers[i].GetComponentInChildren<Text>().text = value.ToString();
                gateAnswers[i].onClick.RemoveAllListeners();
                gateAnswers[i].onClick.AddListener(() =>
                {
                    gate.SetActive(false);
                    if (!isRight) return;
                    panel.SetActive(true);
                    FitToScreen(panelBox);
                    Refresh();
                });
            }
            gate.SetActive(true);
            FitToScreen(gateBox);
        }

        private void BuildGate(Transform parent)
        {
            gate = new GameObject("Gate", typeof(Image));
            gate.transform.SetParent(parent, false);
            gate.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);
            Stretch((RectTransform)gate.transform);

            GameObject box = new GameObject("Box", typeof(Image));
            box.transform.SetParent(gate.transform, false);
            box.GetComponent<Image>().color = PanelColor;
            RectTransform boxRect = (RectTransform)box.transform;
            boxRect.anchorMin = boxRect.anchorMax = boxRect.pivot = new Vector2(0.5f, 0.5f);
            boxRect.sizeDelta = new Vector2(800, 700);
            gateBox = boxRect;

            gateQuestion = MakeLabel(box.transform, "", 40, 220, 160, Color.white);
            gateAnswers = new Button[3];
            for (int i = 0; i < 3; i++)
            {
                gateAnswers[i] = MakeAction(box.transform, "", 60 - i * 110, ButtonColor, () => { });
            }
            MakeAction(box.transform, T("Cancel", "Batal"), -290, new Color(0.35f, 0.4f, 0.5f), () => gate.SetActive(false));
        }

        /// <summary>Shrinks a box that is larger than the screen (small phones, portrait) so nothing is cut off.</summary>
        private void FitToScreen(RectTransform box)
        {
            RectTransform canvasRect = (RectTransform)box.root;
            Rect screen = canvasRect.rect;
            float scale = Mathf.Min(1f, screen.width * 0.95f / box.sizeDelta.x, screen.height * 0.95f / box.sizeDelta.y);
            box.localScale = new Vector3(scale, scale, 1f);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private Text MakeLabel(Transform parent, string text, int size, float y, float height, Color color)
        {
            GameObject host = new GameObject("Label", typeof(Text));
            host.transform.SetParent(parent, false);
            Text label = host.GetComponent<Text>();
            label.font = font; label.fontSize = size; label.color = color; label.text = text;
            label.alignment = TextAnchor.MiddleCenter;
            RectTransform rect = (RectTransform)host.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, y);
            rect.sizeDelta = new Vector2(700, height);
            return label;
        }

        private Button MakeAction(Transform parent, string text, float y, Color color, UnityEngine.Events.UnityAction onClick)
        {
            Button button = MakeButton(parent, "Button", text, color, 32);
            RectTransform rect = (RectTransform)button.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, y);
            rect.sizeDelta = new Vector2(620, 80);
            button.onClick.AddListener(onClick);
            return button;
        }

        private Button MakeButton(Transform parent, string name, string text, Color color, int fontSize)
        {
            GameObject host = new GameObject(name, typeof(Image), typeof(Button));
            host.transform.SetParent(parent, false);
            host.GetComponent<Image>().color = color;
            Text label = MakeLabel(host.transform, text, fontSize, 0, 80, Color.white);
            Stretch((RectTransform)label.transform);
            return host.GetComponent<Button>();
        }

        private InputField MakeInput(Transform parent, string placeholder, float y, bool password)
        {
            GameObject host = new GameObject("Input", typeof(Image), typeof(InputField));
            host.transform.SetParent(parent, false);
            host.GetComponent<Image>().color = Color.white;
            RectTransform rect = (RectTransform)host.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, y);
            rect.sizeDelta = new Vector2(620, 80);

            Text text = MakeLabel(host.transform, "", 32, 0, 80, Color.black);
            text.alignment = TextAnchor.MiddleLeft;
            text.supportRichText = false;
            Stretch((RectTransform)text.transform);
            ((RectTransform)text.transform).offsetMin = new Vector2(20, 0);

            Text hint = MakeLabel(host.transform, placeholder, 32, 0, 80, new Color(0.5f, 0.5f, 0.5f));
            hint.alignment = TextAnchor.MiddleLeft;
            Stretch((RectTransform)hint.transform);
            ((RectTransform)hint.transform).offsetMin = new Vector2(20, 0);

            InputField field = host.GetComponent<InputField>();
            field.textComponent = text;
            field.placeholder = hint;
            field.contentType = password ? InputField.ContentType.Password : InputField.ContentType.Standard;
            field.characterLimit = 72;
            return field;
        }
    }
}

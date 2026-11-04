using System;
using System.IO;
using System.Linq;
using FunMath.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace FunMath.Editor
{
    public static class ProfileUIBuilder
    {
        private const string Root = "Assets/UI/";
        private static AdventureTheme theme;
        private static AvatarCatalog catalog;
        private static Sprite circle;
        private static Sprite triangle;

        [MenuItem("Fun Math/UI/6 - Build profile screens and avatars")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Save the open scene first.");
            theme = AssetDatabase.LoadAssetAtPath<AdventureTheme>(Root + "Resources/AdventureTheme.asset");
            Directory.CreateDirectory(Root + "Prefabs/Avatars");
            circle = Shape("AvatarCircle", false);
            triangle = Shape("AvatarTriangle", true);
            BuildCatalog();
            var dialog = BuildDialog();
            var card = BuildCard(dialog);
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (string name in new[] { "PlayerInformation", "PlayerInformationMain", "LoginAddAccountPage" })
                {
                    var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity");
                    if (scene.GetRootGameObjects().Any(g => g.name == "ProfileShell")) continue;
                    if (name == "LoginAddAccountPage") BuildAccounts(card); else BuildForm(name);
                    EditorSceneManager.SaveScene(scene);
                }
                var mainMenu = EditorSceneManager.OpenScene("Assets/Scenes/AfterLogin/MainMenu.unity");
                var safe = mainMenu.GetRootGameObjects().First(g => g.name == "AdventureShell").transform.Find("SafeArea");
                AttachMainMenuProfile(safe);
                EditorSceneManager.SaveScene(mainMenu);
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
            AssetDatabase.SaveAssets();
        }

        private static RectTransform Rect(string name, Transform parent, float left, float bottom, float right, float top)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            QuizPageLayout.Place(rect, left, bottom, right, top);
            return rect;
        }

        private static Image Image(string name, Transform parent, Sprite sprite, Color color, float l, float b, float r, float t)
        {
            var rect = Rect(name, parent, l, b, r, t);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            if (sprite == theme.roundedPanel) image.type = UnityEngine.UI.Image.Type.Sliced;
            return image;
        }

        private static TMP_Text Text(string name, Transform parent, string bm, string en, float size, float l, float b, float r, float t)
        {
            var text = Rect(name, parent, l, b, r, t).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = theme.bodyFont;
            text.fontStyle = FontStyles.Bold;
            text.color = theme.ink;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Min(size, 28);
            text.fontSizeMax = size;
            text.text = PlayerPrefs.GetInt("EnglishLang", 0) == 1 ? en : bm;
            text.raycastTarget = false;
            if (bm != en)
            {
                var label = text.gameObject.AddComponent<BilingualLabel>();
                label.malay = bm;
                label.english = en;
            }
            return text;
        }

        private static Button Button(string name, Transform parent, string bm, string en, Color color, float l, float b, float r, float t)
        {
            var image = Image(name, parent, theme.roundedPanel, color, l, b, r, t);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            image.gameObject.AddComponent<KidButton>().theme = theme;
            var outline = image.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color32(32, 58, 71, 90);
            outline.effectDistance = new Vector2(2, -3);
            Text("Label", image.transform, bm, en, 42, .05f, .08f, .95f, .92f);
            return button;
        }

        private static Canvas Canvas(string name, int order)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = .5f;
            return canvas;
        }

        private static RectTransform Safe(Canvas canvas)
        {
            var safe = Rect("SafeArea", canvas.transform, 0, 0, 1, 1);
            safe.gameObject.AddComponent<SafeAreaPanel>();
            return safe;
        }

        private static Sprite Shape(string name, bool pointed)
        {
            string path = Root + "Art/" + name + ".png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
                {
                    float edge = pointed ? (1 - y / 127f) * 63 - Mathf.Abs(x - 63.5f) : 62 - Vector2.Distance(new Vector2(x, y), Vector2.one * 63.5f);
                    texture.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(edge + .5f)));
                }
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void BuildCatalog()
        {
            string path = Root + "Resources/AvatarCatalog.asset";
            catalog = AssetDatabase.LoadAssetAtPath<AvatarCatalog>(path);
            if (catalog) return; // Existing owned art and catalogue edits are authoritative.
            catalog = ScriptableObject.CreateInstance<AvatarCatalog>();
            string[] ids = { "owl", "cat", "bear", "fox", "rabbit", "panda", "frog", "penguin" };
            string[] bm = { "Burung hantu", "Kucing", "Beruang", "Musang", "Arnab", "Panda", "Katak", "Penguin" };
            string[] en = { "Owl", "Cat", "Bear", "Fox", "Rabbit", "Panda", "Frog", "Penguin" };
            catalog.avatars = ids.Select((id, i) => new AvatarCatalog.Avatar { id = id, malay = bm[i], english = en[i], artwork = BuildAnimal(id) }).ToArray();
            AssetDatabase.CreateAsset(catalog, path);
        }

        private static RectTransform BuildAnimal(string id)
        {
            string path = Root + "Prefabs/Avatars/" + id + ".prefab";
            var existing = AssetDatabase.LoadAssetAtPath<RectTransform>(path);
            if (existing) return existing;
            var root = Rect(id, null, 0, 0, 1, 1);
            root.sizeDelta = Vector2.one * 240;
            Color cream = new Color32(255, 239, 205, 255);
            Color dark = theme.ink;
            Color orange = new Color32(234, 144, 72, 255);
            Color brown = new Color32(177, 124, 85, 255);
            Color green = new Color32(130, 195, 116, 255);
            Color face = id == "frog" ? green : id == "bear" ? brown : id == "panda" ? cream : id == "penguin" ? dark : orange;
            bool pointed = id == "cat" || id == "fox" || id == "owl";
            Image("Left ear", root, pointed ? triangle : circle, face, .12f, .59f, .4f, id == "rabbit" ? 1 : .91f);
            Image("Right ear", root, pointed ? triangle : circle, face, .6f, .59f, .88f, id == "rabbit" ? 1 : .91f);
            Image("Face", root, circle, face, .08f, .08f, .92f, .89f);
            if (id == "penguin" || id == "fox" || id == "bear") Image("Muzzle", root, circle, cream, .22f, .12f, .78f, .65f);
            if (id == "owl" || id == "panda")
            {
                Image("Left eye patch", root, circle, id == "panda" ? dark : cream, .15f, .4f, .49f, .75f);
                Image("Right eye patch", root, circle, id == "panda" ? dark : cream, .51f, .4f, .85f, .75f);
            }
            Image("Left eye", root, circle, id == "panda" ? cream : dark, .31f, .48f, .4f, .59f);
            Image("Right eye", root, circle, id == "panda" ? cream : dark, .6f, .48f, .69f, .59f);
            Image("Nose", root, id == "owl" || id == "penguin" ? triangle : circle,
                id == "owl" || id == "penguin" ? new Color32(255, 202, 75, 255) : dark, .44f, .3f, .56f, .4f);
            Image("Left cheek", root, circle, new Color32(249, 169, 148, 255), .2f, .31f, .3f, .38f);
            Image("Right cheek", root, circle, new Color32(249, 169, 148, 255), .7f, .31f, .8f, .38f);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, path);
            UnityEngine.Object.DestroyImmediate(root.gameObject);
            return prefab.GetComponent<RectTransform>();
        }

        private static AvatarPicker Picker(Transform parent)
        {
            var root = Rect("AvatarPicker", parent, 0, 0, 1, 1);
            var picker = root.gameObject.AddComponent<AvatarPicker>();
            picker.catalog = catalog;
            var grid = Rect("Choices", root, 0, .12f, 1, 1);
            grid.gameObject.AddComponent<GridLayoutGroup>();
            var adaptive = grid.gameObject.AddComponent<AdaptiveMenu>();
            adaptive.portraitColumns = adaptive.landscapeColumns = 4;
            adaptive.gap = 16;
            picker.choices = catalog.avatars.Select(avatar =>
            {
                var button = Button(avatar.id, grid, "", "", theme.cream, 0, 0, 1, 1);
                var slot = Rect("ArtworkSlot", button.transform, .08f, .23f, .92f, .97f);
                var square = Rect("Artwork", slot, 0, 0, 1, 1);
                square.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                square.GetComponent<AspectRatioFitter>().aspectRatio = 1;
                var art = (RectTransform)PrefabUtility.InstantiatePrefab(avatar.artwork, square);
                QuizPageLayout.Place(art, 0, 0, 1, 1);
                Text("Animal name", button.transform, avatar.malay, avatar.english, 30, .02f, .02f, .98f, .22f);
                var mark = Image("Selected", button.transform, circle, theme.sections[1], .76f, .77f, .98f, .98f);
                Text("Selection label", mark.transform, "OK", "OK", 24, 0, 0, 1, 1);
                mark.gameObject.SetActive(avatar.id == "owl");
                return new AvatarPicker.Choice { id = avatar.id, button = button, selectedMark = mark.gameObject };
            }).ToArray();
            picker.caption = Text("Selected animal", root, "Pilihan: Burung hantu", "Selected: Owl", 38, 0, 0, 1, .1f);
            // The caption belongs to the picker, including when the selected ID changes.
            UnityEngine.Object.DestroyImmediate(picker.caption.GetComponent<BilingualLabel>());
            return picker;
        }

        private static AvatarPickerDialog BuildDialog()
        {
            string path = Root + "Prefabs/AvatarPickerDialog.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<AvatarPickerDialog>(path);
            if (existing) return existing;
            var canvas = Canvas("AvatarPickerDialog", 200);
            Image("Dimmer", canvas.transform, null, new Color(0, 0, 0, .45f), 0, 0, 1, 1).raycastTarget = true;
            var body = Image("Body", Safe(canvas), theme.roundedPanel, theme.sky, .04f, .18f, .96f, .82f).rectTransform;
            Text("Heading", body, "Pilih avatar", "Choose an avatar", 56, .05f, .86f, .95f, .98f);
            var dialog = canvas.gameObject.AddComponent<AvatarPickerDialog>();
            dialog.picker = Picker(body);
            QuizPageLayout.Place((RectTransform)dialog.picker.transform, .04f, .19f, .96f, .84f);
            dialog.cancel = Button("Cancel", body, "Kembali", "Back", theme.cream, .05f, .03f, .46f, .15f);
            dialog.confirm = Button("Save", body, "Simpan", "Save", theme.sections[0], .54f, .03f, .95f, .15f);
            var controls = dialog.picker.choices.Select(c => c.button).Concat(new[] { dialog.cancel, dialog.confirm }).ToArray();
            for (int i = 0; i < controls.Length; i++)
            {
                var navigation = controls[i].navigation;
                navigation.mode = Navigation.Mode.Explicit;
                navigation.selectOnLeft = navigation.selectOnUp = controls[(i + controls.Length - 1) % controls.Length];
                navigation.selectOnRight = navigation.selectOnDown = controls[(i + 1) % controls.Length];
                controls[i].navigation = navigation;
            }
            var prefab = PrefabUtility.SaveAsPrefabAsset(canvas.gameObject, path);
            UnityEngine.Object.DestroyImmediate(canvas.gameObject);
            return prefab.GetComponent<AvatarPickerDialog>();
        }

        private static ProfileCard BuildCard(AvatarPickerDialog dialog)
        {
            string path = Root + "Prefabs/ProfileCard.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<ProfileCard>(path);
            if (existing) return existing;
            var body = Image("ProfileCard", null, theme.roundedPanel, theme.cream, 0, 0, 1, 1);
            var card = body.gameObject.AddComponent<ProfileCard>();
            var slot = Rect("PortraitSlot", body.transform, .2f, .55f, .8f, .98f);
            var portrait = Rect("Portrait", slot, 0, 0, 1, 1);
            portrait.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            portrait.GetComponent<AspectRatioFitter>().aspectRatio = 1;
            card.portrait = portrait.gameObject.AddComponent<AvatarPortrait>();
            card.portrait.catalog = catalog;
            card.playerName = Text("PlayerName", body.transform, "Nama", "Name", 46, .06f, .43f, .94f, .56f);
            card.age = Text("Age", body.transform, "7 tahun", "7 years", 32, .08f, .35f, .92f, .43f);
            // Runtime data is authoritative for these labels.
            foreach (var label in new[] { card.playerName, card.age })
                if (label.GetComponent<BilingualLabel>()) UnityEngine.Object.DestroyImmediate(label.GetComponent<BilingualLabel>());
            card.changeAvatar = Button("ChangeAvatar", body.transform, "Tukar avatar", "Change avatar", theme.sky, .07f, .19f, .93f, .32f);
            card.play = Button("Play", body.transform, "Main", "Play", theme.sections[0], .07f, .03f, .93f, .16f);
            card.avatarDialog = dialog;
            card.play.gameObject.AddComponent<ScrollSelectionIntoView>();
            card.changeAvatar.gameObject.AddComponent<ScrollSelectionIntoView>();
            var prefab = PrefabUtility.SaveAsPrefabAsset(body.gameObject, path);
            UnityEngine.Object.DestroyImmediate(body.gameObject);
            return prefab.GetComponent<ProfileCard>();
        }

        private static Canvas Shell()
        {
            var canvas = Canvas("ProfileShell", 10);
            Image("Background", canvas.transform, null, theme.sky, 0, 0, 1, 1);
            return canvas;
        }

        public static void AttachMainMenuProfile(Transform safe)
        {
            if (safe.Find("ActivePlayer")) return;
            theme = AssetDatabase.LoadAssetAtPath<AdventureTheme>(Root + "Resources/AdventureTheme.asset");
            catalog = AssetDatabase.LoadAssetAtPath<AvatarCatalog>(Root + "Resources/AvatarCatalog.asset");
            var dialog = AssetDatabase.LoadAssetAtPath<AvatarPickerDialog>(Root + "Prefabs/AvatarPickerDialog.prefab");
            if (!catalog || !dialog) return;
            var button = Button("ActivePlayer", safe, "", "", theme.cream, .05f, .92f, .67f, .98f);
            var active = button.gameObject.AddComponent<ActivePlayerProfile>();
            var slot = Rect("PortraitSlot", button.transform, .02f, .03f, .17f, .97f);
            var portrait = Rect("Portrait", slot, 0, 0, 1, 1);
            var aspect = portrait.gameObject.AddComponent<AspectRatioFitter>();
            aspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            aspect.aspectRatio = 1;
            active.portrait = portrait.gameObject.AddComponent<AvatarPortrait>();
            active.portrait.catalog = catalog;
            active.playerName = Text("PlayerName", button.transform, "", "", 38, .2f, .05f, .97f, .95f);
            active.dialogPrefab = dialog;
        }

        private static void BuildForm(string sceneName)
        {
            var save = UnityEngine.Object.FindFirstObjectByType<SaveInformation>();
            var legacy = save.GetComponentInParent<Canvas>();
            var submit = legacy.GetComponentsInChildren<Button>(true).First(b => b.name == "ButtonStart");
            var canvas = Shell();
            var safe = Safe(canvas);
            Text("Heading", safe, "Mari berkenalan!", "Let's meet!", 76, .24f, .88f, .96f, .97f);
            var picker = Picker(safe);
            var form = (RectTransform)save.transform;
            form.SetParent(safe, false);
            foreach (Transform child in form) if (child.name != "PlayerName" && child.name != "PlayerAge" && child.name != "PlayerGender") child.gameObject.SetActive(false);
            save.PlayerNameTMP = form.Find("PlayerName").GetComponent<TMP_InputField>();
            save.PlayerAgeTMP = form.Find("PlayerAge").GetComponent<TMP_InputField>();
            save.PlayerGenderTMP = form.Find("PlayerGender").GetComponent<TMP_Dropdown>();
            save.avatarPicker = picker;
            StyleInput(save.PlayerNameTMP);
            save.PlayerNameTMP.characterLimit = 24;
            StyleInput(save.PlayerAgeTMP);
            save.PlayerAgeTMP.readOnly = true;
            save.PlayerAgeTMP.interactable = false;
            var ageColors = save.PlayerAgeTMP.colors;
            ageColors.disabledColor = Color.white;
            save.PlayerAgeTMP.colors = ageColors;
            save.PlayerAgeTMP.text = "7";
            QuizPageLayout.Place((RectTransform)save.PlayerNameTMP.transform, 0, .72f, 1, .89f);
            QuizPageLayout.Place((RectTransform)save.PlayerAgeTMP.transform, .23f, .41f, .77f, .58f);
            QuizPageLayout.Place((RectTransform)save.PlayerGenderTMP.transform, 0, .1f, 1, .27f);
            var genderImage = save.PlayerGenderTMP.GetComponent<Image>();
            genderImage.sprite = theme.roundedPanel;
            genderImage.type = UnityEngine.UI.Image.Type.Sliced;
            genderImage.color = theme.cream;
            save.PlayerGenderTMP.captionText.font = theme.bodyFont;
            save.PlayerGenderTMP.captionText.fontSize = 44;
            save.PlayerGenderTMP.captionText.color = theme.ink;
            save.genderValues = new[] { "Unspecified", "Male", "Female" };
            var formLanguage = form.gameObject.AddComponent<ProfileFormLanguage>();
            formLanguage.namePlaceholder = save.PlayerNameTMP.placeholder as TMP_Text;
            formLanguage.gender = save.PlayerGenderTMP;
            Text("Name label", form, "Nama kamu", "Your name", 42, 0, .9f, 1, 1);
            Text("Age label", form, "Umur", "Age", 42, 0, .59f, 1, .69f);
            Text("Gender label", form, "Jantina", "Gender", 42, 0, .28f, 1, .38f);
            var stepper = form.gameObject.AddComponent<AgeStepper>();
            stepper.input = save.PlayerAgeTMP;
            stepper.decrease = Button("Younger", form, "-", "-", theme.sections[2], 0, .41f, .18f, .58f);
            stepper.increase = Button("Older", form, "+", "+", theme.sections[2], .82f, .41f, 1, .58f);
            submit.transform.SetParent(safe, false);
            submit.transform.localScale = Vector3.one;
            foreach (var behaviour in submit.GetComponents<Behaviour>())
                if (behaviour is Animator || behaviour is ButtonAnimationController) behaviour.enabled = false;
            QuizPageLayout.Place((RectTransform)submit.transform, .2f, .035f, .8f, .11f);
            var oldText = submit.GetComponentInChildren<TMP_Text>();
            oldText.text = "Mari bermain!";
            var language = oldText.GetComponent<BilingualLabel>() ?? oldText.gameObject.AddComponent<BilingualLabel>();
            language.malay = "Mari bermain!";
            language.english = "Let's play!";
            save.validationMessage = Text("Validation", safe, "", "", 34, .08f, .11f, .92f, .14f);
            var layout = canvas.gameObject.AddComponent<ProfileScreenLayout>();
            layout.avatars = (RectTransform)picker.transform;
            layout.form = form;
            var back = Button("Back", safe, "Kembali", "Back", theme.cream, .03f, .89f, .22f, .95f);
            var controller = legacy.GetComponent<SceneController>();
            if (!controller) controller = legacy.gameObject.AddComponent<SceneController>();
            UnityEventTools.AddStringPersistentListener(back.onClick, controller.loadScene, sceneName == "PlayerInformationMain" ? "LoginAddAccountPage" : "GetStarted");
            legacy.enabled = false;
            legacy.GetComponent<GraphicRaycaster>().enabled = false;
            DisableHiddenNavigation(legacy);
            canvas.gameObject.AddComponent<MenuFocus>().activities = picker.transform;
        }

        private static void StyleInput(TMP_InputField input)
        {
            var image = input.GetComponent<Image>();
            image.sprite = theme.roundedPanel;
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.color = theme.cream;
            input.textComponent.font = theme.bodyFont;
            input.textComponent.color = theme.ink;
            input.textComponent.fontSize = 48;
            if (input.placeholder is TMP_Text placeholder)
            {
                placeholder.font = theme.bodyFont;
                placeholder.color = theme.ink;
                placeholder.fontSize = 42;
            }
        }

        private static void BuildAccounts(ProfileCard prefab)
        {
            var accounts = UnityEngine.Object.FindFirstObjectByType<AccountController>();
            var legacy = accounts.GetComponentInParent<Canvas>();
            var controller = legacy.GetComponent<SceneController>();
            if (!controller) controller = legacy.gameObject.AddComponent<SceneController>();
            var canvas = Shell();
            // Account cards use the same orientation scaling, without form/picker regions.
            canvas.gameObject.AddComponent<ProfileScreenLayout>();
            var safe = Safe(canvas);
            Text("Heading", safe, "Siapa mahu bermain?", "Who's playing?", 72, .06f, .86f, .94f, .97f);
            var scroll = Rect("Players", safe, .05f, .22f, .95f, .84f).gameObject.AddComponent<ScrollRect>();
            var viewport = Rect("Viewport", scroll.transform, 0, 0, 1, 1);
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = Rect("Content", viewport, 0, 1, 1, 1);
            content.pivot = new Vector2(.5f, 1);
            content.gameObject.AddComponent<GridLayoutGroup>();
            content.gameObject.AddComponent<AccountGridLayout>();
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = content;
            scroll.viewport = viewport;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            // A transparent graphic receives drags in the gaps between cards.
            var drag = viewport.gameObject.AddComponent<Image>();
            drag.color = Color.clear;
            accounts.profileContent = content;
            accounts.profileCardPrefab = prefab;
            accounts.emptyProfileHint = Text("Empty profiles", safe, "Cipta pemain pertama kamu!", "Create your first player!", 54, .1f, .42f, .9f, .65f).gameObject;
            accounts.GetComponent<ScrollRect>().enabled = false;
            var add = Button("AddPlayer", safe, "Pemain baru", "New player", theme.sections[0], .12f, .11f, .88f, .19f);
            UnityEventTools.AddStringPersistentListener(add.onClick, controller.loadScene, "PlayerInformationMain");
            var resetSource = legacy.transform.Find("MiddleRect/Button").GetComponent<Button>();
            var reset = Button("Reset", safe, "Reset data (ibu bapa)", "Reset data (parents)", theme.cream, .2f, .025f, .8f, .08f);
            reset.onClick = resetSource.onClick;
            var prompt = legacy.transform.Find("Prompted-2");
            prompt.SetParent(safe, false);
            var modal = prompt.Find("Image");
            if (modal) modal.gameObject.AddComponent<ModalBodyFit>();
            prompt.SetAsLastSibling();
            legacy.enabled = false;
            legacy.GetComponent<GraphicRaycaster>().enabled = false;
            DisableHiddenNavigation(legacy);
            var debug = resetSource.GetComponent<DisplayKeys>();
            if (debug) debug.enabled = false;
            canvas.gameObject.AddComponent<MenuFocus>().activities = add.transform;
        }

        private static void DisableHiddenNavigation(Canvas canvas)
        {
            foreach (var control in canvas.GetComponentsInChildren<Selectable>(true))
            {
                var navigation = control.navigation;
                navigation.mode = Navigation.Mode.None;
                control.navigation = navigation;
            }
        }
    }
}


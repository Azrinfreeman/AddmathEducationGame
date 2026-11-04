using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FunMath.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FunMath.Editor
{
    /// <summary>Targeted, repeatable presentation patch. Never rebuilds a shell or changes route events.</summary>
    public static class PlaygroundUIBuilder
    {
        private const string Root = "Assets/UI/";
        private const string PrefabPath = Root + "Prefabs/Backgrounds/MathPlayground.prefab";
        private static AdventureTheme Theme => AssetDatabase.LoadAssetAtPath<AdventureTheme>(Root + "Resources/AdventureTheme.asset");
        private static T Ensure<T>(GameObject go) where T : Component
        {
            // Unity can return a destroyed-object wrapper; use its existence check.
            var component = go.GetComponent<T>();
            return component ? component : go.AddComponent<T>();
        }
        private static IEnumerable<T> All<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<T>(true));

        [MenuItem("Fun Math/UI/Playground/1 - Create missing shared scenery assets")]
        public static void CreateAssets()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            foreach (var folder in new[] { "Resources/Backgrounds", "Prefabs/Backgrounds", "Animation/Backgrounds" }) Directory.CreateDirectory(Root + folder);
            AssetDatabase.Refresh();
            foreach (var file in Directory.GetFiles(Root + "Art/Backgrounds", "*.png"))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(file);
                if (importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Single) continue;
                importer.textureType = TextureImporterType.Sprite; importer.spritePixelsPerUnit = 100;
                importer.spriteImportMode = SpriteImportMode.Single; importer.filterMode = FilterMode.Bilinear;
                importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed; importer.maxTextureSize = 512;
                importer.SaveAndReimport();
            }
            Profile("Explore", BackgroundProfile.MotionMode.Explore);
            Profile("Focus", BackgroundProfile.MotionMode.Focus);
            Profile("Still", BackgroundProfile.MotionMode.Still);
            if (!File.Exists(PrefabPath)) CreateBackdrop();
            else RepairSpriteReferences();
            AssetDatabase.SaveAssets();
        }
        private static void RepairSpriteReferences()
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            bool changed = false;
            var names = new Dictionary<string, string> { { "FarHills", "Hill" }, { "NearHills", "Hill" },
                { "CloudLeft", "Cloud" }, { "CloudRight", "Cloud" }, { "GardenLeft", "Garden" }, { "GardenRight", "Garden" }, { "Visual", "Sun" } };
            try
            {
                foreach (var image in root.GetComponentsInChildren<Image>(true))
                    if (!image.sprite && names.TryGetValue(image.name, out var name)) { image.sprite = Art(name); changed = true; }
                var presenter = root.GetComponent<AnimatedBackground>();
                if (!presenter.discoveryCaption)
                {
                    var caption = Label("Greeting", presenter.discoveryButton.transform, "Hai!", "Hello!", 34, -.1f, -.3f, 1.4f, .12f);
                    presenter.discoveryCaption = caption.gameObject; caption.gameObject.SetActive(false); changed = true;
                }
                if (changed) PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static BackgroundProfile Profile(string name, BackgroundProfile.MotionMode mode)
        {
            string path = Root + "Resources/Backgrounds/" + name + ".asset";
            var profile = AssetDatabase.LoadAssetAtPath<BackgroundProfile>(path);
            if (profile) return profile;
            profile = ScriptableObject.CreateInstance<BackgroundProfile>(); profile.mode = mode;
            if (mode != BackgroundProfile.MotionMode.Explore)
            {
                profile.farHill = new Color32(206, 230, 210, 255); profile.nearHill = new Color32(174, 211, 192, 255);
                profile.cloudTravel = 12;
            }
            AssetDatabase.CreateAsset(profile, path); return profile;
        }
        private static Sprite Art(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Art/Backgrounds/" + name + ".png");
        private static RectTransform Rect(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); Place(rect, x0, y0, x1, y1); return rect;
        }
        private static void Place(RectTransform rect, float x0, float y0, float x1, float y1)
        {
            rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        private static Image Image(string name, Transform parent, Sprite sprite, Color color, float x0, float y0, float x1, float y1)
        {
            var rect = Rect(name, parent, x0, y0, x1, y1); var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite; image.color = color; image.raycastTarget = false; return image;
        }
        private static TMP_Text Label(string name, Transform parent, string bm, string en, float size, float x0, float y0, float x1, float y1)
        {
            var rect = Rect(name, parent, x0, y0, x1, y1); var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = Theme.displayFont; text.fontSize = size; text.fontSizeMin = 28; text.fontSizeMax = size;
            text.enableAutoSizing = true; text.color = Theme.ink; text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false; text.text = bm;
            var bilingual = rect.gameObject.AddComponent<BilingualLabel>(); bilingual.malay = bm; bilingual.english = en;
            return text;
        }
        private static Button Button(string name, Transform parent, string bm, string en, float x0, float y0, float x1, float y1)
        {
            var existing = parent.Find(name); if (existing) return existing.GetComponent<Button>();
            var image = Image(name, parent, Theme.roundedPanel, Theme.cream, x0, y0, x1, y1); image.type = UnityEngine.UI.Image.Type.Sliced; image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var outline = image.gameObject.AddComponent<Outline>(); outline.effectColor = new Color32(32, 58, 71, 255); outline.effectDistance = new Vector2(2, -3);
            image.gameObject.AddComponent<KidButton>().theme = Theme;
            Label("Label", image.transform, bm, en, 38, .04f, .08f, .96f, .92f); return button;
        }
        private static void CreateBackdrop()
        {
            var root = Rect("MathPlayground", null, 0, 0, 1, 1);
            var presenter = root.gameObject.AddComponent<AnimatedBackground>();
            presenter.profile = Profile("Explore", BackgroundProfile.MotionMode.Explore);
            presenter.sky = Image("Sky", root, null, presenter.profile.sky, 0, 0, 1, 1);
            presenter.farHill = Image("FarHills", root, Art("Hill"), presenter.profile.farHill, -.1f, -.04f, 1.2f, .55f);
            presenter.nearHill = Image("NearHills", root, Art("Hill"), presenter.profile.nearHill, -.3f, -.12f, 1.15f, .42f);
            var cloud1 = Image("CloudLeft", root, Art("Cloud"), Color.white, -.15f, .58f, .12f, .72f);
            var cloud2 = Image("CloudRight", root, Art("Cloud"), Color.white, .88f, .6f, 1.15f, .74f);
            cloud1.preserveAspect = cloud2.preserveAspect = true;
            presenter.clouds = new[] { cloud1.rectTransform, cloud2.rectTransform };
            var left = Image("GardenLeft", root, Art("Garden"), Color.white, -.075f, -.02f, .18f, .2f);
            var right = Image("GardenRight", root, Art("Garden"), Color.white, .85f, -.03f, 1.1f, .19f);
            left.preserveAspect = right.preserveAspect = true;
            presenter.corners = new[] { left.rectTransform, right.rectTransform };
            var sunRoot = Rect("DiscoverySun", root, .015f, .65f, .17f, .77f);
            var sun = Image("Visual", sunRoot, Art("Sun"), Color.white, 0, 0, 1, 1); sun.preserveAspect = true;
            presenter.discovery = sun.rectTransform;
            var button = sunRoot.gameObject.AddComponent<Button>();
            // A stable target surrounds the animated visual; no moving hit rectangle.
            var hit = sunRoot.gameObject.AddComponent<Image>(); hit.color = Color.clear; hit.raycastTarget = true;
            button.targetGraphic = hit; button.transition = Selectable.Transition.None; presenter.discoveryButton = button;
            presenter.scenery = left.gameObject.AddComponent<Animator>();
            presenter.scenery.updateMode = AnimatorUpdateMode.UnscaledTime;
            presenter.scenery.runtimeAnimatorController = CreateController();
            PrefabUtility.SaveAsPrefabAsset(root.gameObject, PrefabPath); UnityEngine.Object.DestroyImmediate(root.gameObject);
        }
        private static AnimatorController CreateController()
        {
            string path = Root + "Animation/Backgrounds/Playground.controller";
            var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path); if (existing) return existing;
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Mode", AnimatorControllerParameterType.Int);
            var machine = controller.layers[0].stateMachine;
            string[] names = { "Still", "Explore", "Focus" };
            for (int i = 0; i < names.Length; i++)
            {
                var clip = new AnimationClip { name = names[i], frameRate = 30 };
                // Author a small breathing motion on a visual child; layer positions remain editable.
                var curve = i == 1 ? AnimationCurve.EaseInOut(0, 1, 3, 1.025f) : AnimationCurve.Linear(0, 1, 6, 1);
                if (i == 1) curve.AddKey(new Keyframe(6, 1, 0, 0));
                clip.SetCurve("", typeof(Transform), "m_LocalScale.y", curve);
                var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                AssetDatabase.CreateAsset(clip, Root + "Animation/Backgrounds/" + names[i] + ".anim");
                var state = machine.AddState(names[i], new Vector3(260, i * 80)); state.motion = clip;
                if (i == 0) machine.defaultState = state;
                var transition = machine.AddAnyStateTransition(state); transition.hasExitTime = false;
                transition.duration = 0; transition.canTransitionToSelf = false;
                transition.AddCondition(AnimatorConditionMode.Equals, i, "Mode");
            }
            return controller;
        }

        [MenuItem("Fun Math/UI/Playground/2 - Patch selected scene")]
        public static void PatchSelected()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            CreateAssets(); Patch(SceneManager.GetActiveScene());
        }
        [MenuItem("Fun Math/UI/Playground/3 - Patch remaining project screens")]
        public static void PatchAll()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save the current scene first.");
            CreateAssets();
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (var entry in EditorBuildSettings.scenes.Where(s => s.enabled && !s.path.Contains("/AR/")))
                {
                    var scene = EditorSceneManager.OpenScene(entry.path);
                    Patch(scene);
                }
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }
        public static void Patch(Scene scene)
        {
            if (scene.isDirty) throw new InvalidOperationException("Save the current scene before applying the presentation patch: " + scene.path);
            var before = Wiring(scene);
            string geometry = PhysicsGeometry(scene);
            bool ar = scene.path.Contains("/AR/");
            var shell = All<Canvas>(scene).FirstOrDefault(c => c.name == "AdventureShell" || c.name == "ProfileShell" || c.name == "SpecialtyShell");
            if (!ar)
            {
                var target = shell ? shell : All<Canvas>(scene).FirstOrDefault(c => c.enabled && c.isRootCanvas && c.renderMode != RenderMode.WorldSpace);
                if (target)
                {
                    var found = target.transform.Find("MathPlayground");
                    var backdrop = found ? found.gameObject : (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), target.transform);
                    backdrop.transform.SetAsFirstSibling();
                    var presenter = backdrop.GetComponent<AnimatedBackground>();
                    bool hub = shell && shell.name == "AdventureShell";
                    var mode = hub ? BackgroundProfile.MotionMode.Explore : scene.path.Contains("/Games/") || scene.path.Contains("/Quizzes/") ? BackgroundProfile.MotionMode.Still : BackgroundProfile.MotionMode.Focus;
                    if (!found) presenter.profile = Profile(mode.ToString(), mode);
                    presenter.guide = All<VoiceGuide>(scene).FirstOrDefault(); presenter.lessonVoice = All<LessonVoice>(scene).FirstOrDefault();
                    presenter.discoveryButton.gameObject.SetActive(hub);
                    if (!hub) foreach (var garden in presenter.corners) garden.gameObject.SetActive(false);
                    // Disable only known decorative fills. Never touch educational pictures or board objects.
                    foreach (var image in target.GetComponentsInChildren<Image>(true))
                    {
                        if (image.transform.IsChildOf(backdrop.transform)) continue;
                        var surface = image.GetComponent<ThemedSurface>();
                        bool fill = image.transform.parent == target.transform && (image.name == "Sky" || image.name == "Background" || image.name == "BackgroundPanel");
                        if (fill || (surface && surface.section == -2 && image.transform.parent == target.transform)) image.enabled = false;
                    }
                    if (hub) PatchHub(shell);
                    else
                    {
                        if (target.name == "SpecialtyShell") PatchRewards(target);
                        ConfigureModals(scene, target);
                    }
                    presenter.modalOwner = ModalInputOwner.ForScene(scene);
                }
            }
            // AR keeps the camera view; its existing chrome and safe-area behavior are retained.
            var after = Wiring(scene);
            foreach (var pair in before)
            {
                if (!after.TryGetValue(pair.Key, out string value) || value != pair.Value)
                    throw new InvalidOperationException("Route wiring changed for " + pair.Key + " in " + scene.path);
            }
            if (geometry != PhysicsGeometry(scene)) throw new InvalidOperationException("Physics geometry changed in " + scene.path);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            string directory = "docs/tutorial/review/playground"; Directory.CreateDirectory(directory);
            File.AppendAllText(directory + "/migration.tsv", scene.path + "\t" + before.Count + "\t" + All<AnimatedBackground>(scene).Count() + "\tcallbacks and physics preserved\n");
        }

        private static void ConfigureModals(Scene scene, Canvas host)
        {
            var owner = Ensure<ModalInputOwner>(host.gameObject);
            foreach (var focus in All<GameModalFocus>(scene))
            {
                var scope = Ensure<ModalInputScope>(focus.gameObject); scope.owner = owner;
                scope.firstControl = focus.firstAction;
                scope.allowBack = focus.GetComponentInParent<Canvas>().name == "SpecialtyShell" && focus.name == "BadgeDetails";
            }
            var scopes = All<ModalInputScope>(scene).ToArray();
            var groups = new List<CanvasGroup>();
            foreach (var canvas in All<Canvas>(scene).Where(c => c.enabled && c.isRootCanvas && c.renderMode != RenderMode.WorldSpace))
                foreach (Transform child in canvas.transform)
                {
                    if (child.name == "MathPlayground" || scopes.Any(s => s.transform == child || s.transform.IsChildOf(child))) continue;
                    groups.Add(Ensure<CanvasGroup>(child.gameObject));
                }
            owner.content = groups.ToArray();
        }

        private static void PatchHub(Canvas shell)
        {
            var safe = shell.transform.Find("SafeArea"); if (!safe) return;
            var owner = Ensure<ModalInputOwner>(shell.gameObject);
            var modalLayer = shell.transform.Find("PlaygroundModals") ?? Rect("PlaygroundModals", shell.transform, 0, 0, 1, 1);
            owner.content = new[] { Ensure<CanvasGroup>(safe.gameObject) };
            var settings = safe.Find("ParentSettings") ?? modalLayer.Find("ParentSettings");
            if (settings)
            {
                settings.SetParent(modalLayer, false);
                var scope = Ensure<ModalInputScope>(settings.gameObject); scope.owner = owner;
                scope.firstControl = settings.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name == "Close");
            }
            var owl = safe.Find("OwlGuide");
            if (owl && owl.TryGetComponent<OwlFloat>(out var floating)) floating.enabled = false;
            if (owl)
            {
                var original = owl.GetComponent<Image>();
                var visual = owl.Find("GreetingVisual");
                if (!visual)
                {
                    var image = Image("GreetingVisual", owl, original.sprite, Color.white, 0, 0, 1, 1);
                    image.preserveAspect = true; visual = image.transform;
                }
                original.enabled = false;
                shell.GetComponentInChildren<AnimatedBackground>().mascotVisual = visual as RectTransform;
            }
            if (shell.gameObject.scene.name != "MainMenu")
            {
                if (new[] { "PrasekolahLesson", "Tahap1Lesson", "Tahap2Lesson" }.Contains(shell.gameObject.scene.name)) PatchLessonPath(safe);
                foreach (var mirror in shell.GetComponentsInChildren<NavigationState>(true))
                {
                    var art = mirror.transform.Find("ActivityArt"); if (!art) continue;
                    var image = art.GetComponent<Image>();
                    string name = shell.gameObject.scene.name;
                    image.sprite = Art(name == "GameMenu" ? "ToyWorkshop" : name == "Quizzes" ? "ShapePavilion" : "BookGarden");
                    image.preserveAspect = true;
                }
                modalLayer.SetAsLastSibling(); return;
            }
            var grid = safe.Find("Activities"); if (!grid) return;
            var layout = safe.GetComponent<HubLayout>(); layout.playgroundMainMenu = true;
            var adaptive = grid.GetComponent<AdaptiveMenu>(); adaptive.portraitColumns = adaptive.landscapeColumns = 2;
            var morePanel = modalLayer.Find("MorePanel");
            if (!morePanel)
            {
                var shade = Image("MorePanel", modalLayer, null, new Color(.08f, .15f, .18f, .96f), 0, 0, 1, 1); shade.raycastTarget = true;
                morePanel = shade.transform;
                var card = Image("Card", morePanel, Theme.roundedPanel, Theme.cream, .1f, .22f, .9f, .78f); card.type = UnityEngine.UI.Image.Type.Sliced;
                Label("Title", card.transform, "Lagi untuk diteroka", "More to explore", 48, .04f, .78f, .96f, .97f);
                var routes = Rect("Routes", card.transform, .08f, .25f, .92f, .74f);
                var rows = routes.gameObject.AddComponent<VerticalLayoutGroup>(); rows.spacing = 20; rows.childForceExpandHeight = true; rows.childControlHeight = true;
                var close = Button("Close", card.transform, "Kembali", "Back", .18f, .05f, .82f, .2f);
                UnityEventTools.AddBoolPersistentListener(close.onClick, morePanel.gameObject.SetActive, false);
                var scope = morePanel.gameObject.AddComponent<ModalInputScope>(); scope.owner = owner; scope.firstControl = close;
                morePanel.gameObject.SetActive(false);
            }
            var activityButtons = shell.GetComponentsInChildren<NavigationState>(true);
            foreach (var mirror in activityButtons)
            {
                var button = mirror.GetComponent<Button>(); string route = Route(button);
                int index = Array.IndexOf(new[] { "LearningPage", "GameMenu", "Quizzes", "BadgesPage" }, route);
                if (index >= 0)
                {
                    button.transform.SetParent(grid, false); button.transform.SetSiblingIndex(index);
                    string[] bm = { "Belajar", "Permainan", "Kuiz", "Lencana" }, en = { "Learning", "Games", "Quiz", "Badges" };
                    var label = button.GetComponentInChildren<TMP_Text>(true); var bilingual = label.GetComponent<BilingualLabel>();
                    bilingual.malay = bm[index]; bilingual.english = en[index]; label.text = bm[index]; label.fontSize = label.fontSizeMax = 58; label.fontSizeMin = 48;
                    Place(label.rectTransform, .04f, .03f, .96f, .3f);
                    var image = button.transform.Find("ActivityArt").GetComponent<Image>();
                    image.sprite = Art(new[] { "BookGarden", "ToyWorkshop", "ShapePavilion", "StickerTree" }[index]);
                    Place(image.rectTransform, .13f, .29f, .87f, .95f);
                }
                else if (route == "PlayerInformationMain")
                {
                    button.transform.SetParent(safe, false); Place((RectTransform)button.transform, .36f, .025f, .65f, .09f);
                    SetCaption(button, "Tambah profil", "Add profile");
                    var art = button.transform.Find("ActivityArt"); if (art) art.gameObject.SetActive(false);
                    Place(button.GetComponentInChildren<TMP_Text>(true).rectTransform, .03f, .05f, .97f, .95f);
                }
                else if (route == "PenghargaanPage" || route == "CreditPage")
                {
                    button.transform.SetParent(morePanel.Find("Card/Routes"), false);
                    var art = button.transform.Find("ActivityArt"); if (art) art.gameObject.SetActive(false);
                    Place(button.GetComponentInChildren<TMP_Text>(true).rectTransform, .03f, .05f, .97f, .95f);
                    SetCaption(button, route == "CreditPage" ? "Kredit" : "Penghargaan", route == "CreditPage" ? "Credits" : "Appreciation");
                }
            }
            var more = Button("More", safe, "Lagi", "More", .7f, .025f, .94f, .09f);
            if (more.onClick.GetPersistentEventCount() == 0) UnityEventTools.AddBoolPersistentListener(more.onClick, morePanel.gameObject.SetActive, true);
            modalLayer.SetAsLastSibling();
        }

        private static void PatchLessonPath(Transform safe)
        {
            var area = safe.Find("Activities");
            if (!area) return;
            if (area.GetComponent<LessonPathLayout>()) return;
            var nodes = area.GetComponentsInChildren<NavigationState>(true).Select(n => (RectTransform)n.transform).ToArray();
            if (nodes.Length == 0) return;
            var grid = area.GetComponent<GridLayoutGroup>(); if (grid) grid.enabled = false;
            var adaptive = area.GetComponent<AdaptiveMenu>(); if (adaptive) adaptive.enabled = false;
            var layout = area.gameObject.AddComponent<LessonPathLayout>();
            layout.nodes = nodes; layout.steppingStones = new RectTransform[nodes.Length - 1];
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                var caption = node.GetComponentInChildren<TMP_Text>(true);
                Place(caption.rectTransform, .3f, .08f, .96f, .92f);
                caption.fontSizeMin = 34; caption.fontSizeMax = caption.fontSize = 46;
                var art = node.Find("ActivityArt");
                if (art) Place((RectTransform)art, .02f, .16f, .26f, .84f);
                // Numbering communicates route order, never completion or a prerequisite.
                Label("PathNumber", node, (i + 1).ToString(), (i + 1).ToString(), 30, .015f, .01f, .16f, .42f);
                if (i == 0) continue;
                var stone = Image("TrailStone" + i, area, Theme.roundedPanel, Theme.ink, 0, 0, 0, 0);
                stone.type = UnityEngine.UI.Image.Type.Sliced;
                stone.transform.SetAsFirstSibling(); layout.steppingStones[i - 1] = stone.rectTransform;
            }
        }

        private static void PatchRewards(Canvas shell)
        {
            var safe = shell.transform.Find("SafeArea"); if (!safe) return;
            bool badges = shell.gameObject.scene.name == "BadgesPage";
            var content = safe.Find(badges ? "BadgeCards" : "ReadingArea") as RectTransform;
            if (!content) return;
            var layout = safe.GetComponent<RewardScreenLayout>();
            if (!layout)
            {
                layout = safe.gameObject.AddComponent<RewardScreenLayout>();
                var art = Image("CollectionIllustration", safe, Art(badges ? "StickerTree" : "BookGarden"), Color.white, 0, 0, 1, 1);
                art.preserveAspect = true;
                layout.illustration = art.rectTransform; layout.content = content; layout.badgeCollection = badges;
                var details = safe.Find("BadgeDetails");
                if (details)
                {
                    var surface = details.GetComponent<ThemedSurface>();
                    if (surface) UnityEngine.Object.DestroyImmediate(surface);
                    details.GetComponent<Image>().color = new Color(.08f, .15f, .18f, .7f);
                    details.GetComponent<Image>().enabled = true;
                }
            }
            var shelf = safe.GetComponent<BadgeShelf>();
            if (shelf)
            {
                foreach (var entry in shelf.entries)
                    if (!entry.artwork)
                    {
                        entry.artwork = entry.button.transform.Find("Artwork").GetComponent<Image>();
                        Place(entry.status.rectTransform, .04f, .03f, .96f, .36f);
                        Place((RectTransform)entry.button.transform.Find("Name"), .04f, .36f, .96f, .55f);
                        Place(entry.artwork.rectTransform, .25f, .56f, .75f, .96f);
                    }
                // Keep the modal outside content so lower controls can be suspended.
                if (shelf.details.transform.parent == safe) shelf.details.transform.SetParent(shell.transform, false);
            }
        }
        private static void SetCaption(Button button, string bm, string en)
        {
            var label = button.GetComponentInChildren<TMP_Text>(true); var bilingual = label.GetComponent<BilingualLabel>();
            bilingual.malay = bm; bilingual.english = en; label.text = bm; label.fontSize = label.fontSizeMax = 44; label.fontSizeMin = 38;
        }
        private static string Route(Button button)
        {
            var calls = new SerializedObject(button).FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            for (int i = 0; i < calls.arraySize; i++)
            {
                var call = calls.GetArrayElementAtIndex(i);
                if (call.FindPropertyRelative("m_MethodName").stringValue == "loadScene") return call.FindPropertyRelative("m_Arguments.m_StringArgument").stringValue;
            }
            return "";
        }
        private static Dictionary<Button, string> Wiring(Scene scene)
        {
            return All<Button>(scene).ToDictionary(b => b, b =>
            {
                var calls = new SerializedObject(b).FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
                var rows = new List<string>();
                for (int i = 0; i < calls.arraySize; i++)
                {
                    var call = calls.GetArrayElementAtIndex(i); var args = call.FindPropertyRelative("m_Arguments");
                    var target = call.FindPropertyRelative("m_Target").objectReferenceValue;
                    rows.Add((target ? GlobalObjectId.GetGlobalObjectIdSlow(target).ToString() : "null") + ":" + call.FindPropertyRelative("m_MethodName").stringValue + ":" +
                        call.FindPropertyRelative("m_Mode").intValue + ":" + args.FindPropertyRelative("m_StringArgument").stringValue + ":" + args.FindPropertyRelative("m_IntArgument").intValue + ":" + args.FindPropertyRelative("m_BoolArgument").boolValue);
                }
                return string.Join("|", rows);
            });
        }
        private static string PhysicsGeometry(Scene scene) => string.Join("\n", All<Collider2D>(scene).Select(c => GlobalObjectId.GetGlobalObjectIdSlow(c) + ":" + EditorJsonUtility.ToJson(c) + ":" + c.transform.position + ":" + c.transform.lossyScale));
    }
}


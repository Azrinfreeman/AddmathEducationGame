using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using FunMath.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FunMath.Editor
{
    // Owns generated assets only. Existing controls keep their identity and callbacks.
    public static class AdventureUIBuilder
    {
        private const string Root = "Assets/UI/";
        private static AdventureTheme theme;
        private static ParentGateDialog gate;
        private static readonly string[] HubPaths = {
            "Assets/Scenes/GetStarted.unity", "Assets/Scenes/AfterLogin/MainMenu.unity",
            "Assets/Scenes/Lessons/LearningPage.unity", "Assets/Scenes/Lessons/PrasekolahLesson.unity",
            "Assets/Scenes/Lessons/Tahap1Lesson.unity", "Assets/Scenes/Lessons/Tahap2Lesson.unity",
            "Assets/Scenes/Quizzes/Quizzes.unity", "Assets/Scenes/GameMenu.unity"
        };

        [MenuItem("Fun Math/UI/1 - Build shared assets")]
        public static void BuildAssets()
        {
            Directory.CreateDirectory(Root + "Prefabs");
            theme = AssetDatabase.LoadAssetAtPath<AdventureTheme>(Root + "Resources/AdventureTheme.asset");
            if (!theme) { theme = ScriptableObject.CreateInstance<AdventureTheme>(); AssetDatabase.CreateAsset(theme, Root + "Resources/AdventureTheme.asset"); }
            theme.displayFont = FontAsset("Fredoka");
            theme.bodyFont = FontAsset("Nunito");
            theme.roundedPanel = RoundedSprite();
            var importer = (TextureImporter)AssetImporter.GetAtPath(Root + "Art/OwlGuide.png");
            if (importer) { importer.textureType = TextureImporterType.Sprite; importer.alphaIsTransparency = true; importer.maxTextureSize = 1024; importer.SaveAndReimport(); }
            theme.owl = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Art/OwlGuide.png");
            theme.tap = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/clickSound.wav");
            EditorUtility.SetDirty(theme);
            BuildGate();
            var button = MakeButton("KidButton", null, "Mari cuba!", "Let's try!", 0);
            PrefabUtility.SaveAsPrefabAsset(button.gameObject, Root + "Prefabs/KidButton.prefab");
            UnityEngine.Object.DestroyImmediate(button.gameObject);
            AssetDatabase.SaveAssets();
        }

        private static TMP_FontAsset FontAsset(string name)
        {
            string path = Root + "Fonts/" + name + " SDF.asset";
            var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (asset) return asset;
            var font = AssetDatabase.LoadAssetAtPath<Font>(Root + "Fonts/" + name + ".ttf");
            asset = TMP_FontAsset.CreateFontAsset(font, 64, 6, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024, TMPro.AtlasPopulationMode.Dynamic);
            asset.name = name + " SDF";
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.AddObjectToAsset(asset.material, asset);
            foreach (var texture in asset.atlasTextures) AssetDatabase.AddObjectToAsset(texture, asset);
            asset.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 +-=!?.,:;/()'’éÉ%×÷");
            EditorUtility.SetDirty(asset);
            return asset;
        }

        [MenuItem("Fun Math/UI/3 - Rebuild generated hub layouts")]
        public static void RebuildHubs()
        {
            if(EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            BuildAssets();
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach(var path in HubPaths)
                {
                    var scene=EditorSceneManager.OpenScene(path);
                    var shell=scene.GetRootGameObjects().FirstOrDefault(g=>g.name=="AdventureShell");
                    if(shell) UnityEngine.Object.DestroyImmediate(shell);
                    BuildShell(scene); EditorSceneManager.SaveScene(scene);
                }
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }

        private static Sprite RoundedSprite()
        {
            string path = Root + "Art/RoundedPanel.png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
                {
                    var p = new Vector2(Mathf.Max(24 - x, 0, x - 103), Mathf.Max(24 - y, 0, y - 103));
                    float alpha = 1 - Mathf.Clamp01(p.magnitude - 23);
                    texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
                texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteBorder = Vector4.one * 28;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent, false); rt.anchorMin = min; rt.anchorMax = max;
            rt.offsetMin = rt.offsetMax = Vector2.zero; return rt;
        }
        private static Image Surface(string name, Transform parent, int section, Vector2 min, Vector2 max)
        {
            var rt = Rect(name, parent, min, max); var image = rt.gameObject.AddComponent<Image>();
            image.sprite = theme.roundedPanel; image.type = Image.Type.Sliced; image.raycastTarget = false;
            var themed = rt.gameObject.AddComponent<ThemedSurface>(); themed.theme = theme; themed.section = section; themed.Apply(); return image;
        }
        private static TMP_Text Label(string name, Transform parent, string bm, string en, float size, Vector2 min, Vector2 max)
        {
            var rt = Rect(name, parent, min, max); var text = rt.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = theme.displayFont; text.text = bm; text.fontSize = size; text.enableAutoSizing = true;
            text.fontStyle = FontStyles.Bold;
            text.fontSizeMin = Mathf.Min(size, 26); text.fontSizeMax = size;
            text.color = theme.ink; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
            var language = rt.gameObject.AddComponent<BilingualLabel>(); language.malay = bm; language.english = en; return text;
        }
        private static Button MakeButton(string name, Transform parent, string bm, string en, int section)
        {
            var image = Surface(name, parent, section, Vector2.zero, Vector2.one); image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var outline = image.gameObject.AddComponent<Outline>(); outline.effectColor = new Color32(47, 88, 95, 100); outline.effectDistance = new Vector2(2, -3);
            var feedback = image.gameObject.AddComponent<KidButton>(); feedback.theme = theme;
            var label = Label("Label", image.transform, bm, en, 48, new Vector2(.07f, .08f), new Vector2(.93f, .92f));
            return button;
        }
        private static Canvas MakeCanvas(string name, int order)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = order;
            var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1080); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            return canvas;
        }
        private static void BuildGate()
        {
            var canvas = MakeCanvas("ParentGate", 30000);
            var shade = Surface("InputBlocker", canvas.transform, -1, Vector2.zero, Vector2.one); shade.color = new Color(.08f, .18f, .22f, .96f); shade.raycastTarget = true;
            UnityEngine.Object.DestroyImmediate(shade.GetComponent<ThemedSurface>());
            var card = Surface("ParentChallenge", shade.transform, -1, new Vector2(.12f,.18f), new Vector2(.88f,.82f));
            var dialog = canvas.gameObject.AddComponent<ParentGateDialog>();
            dialog.question = Label("Question", card.transform, "Untuk ibu bapa", "Grown-ups only", 48, new Vector2(.06f,.62f), new Vector2(.94f,.94f));
            UnityEngine.Object.DestroyImmediate(dialog.question.GetComponent<BilingualLabel>());
            dialog.answers = new Button[3];
            for (int i=0;i<3;i++) { var b=MakeButton("Answer"+i,card.transform,"0","0",i); UnityEngine.Object.DestroyImmediate(b.GetComponentInChildren<BilingualLabel>()); var rt=(RectTransform)b.transform; rt.anchorMin=new Vector2(.06f+i*.3f,.34f); rt.anchorMax=new Vector2(.32f+i*.3f,.57f); dialog.answers[i]=b; }
            dialog.cancel=MakeButton("Cancel",card.transform,"Kembali","Back",-1); UnityEngine.Object.DestroyImmediate(dialog.cancel.GetComponentInChildren<BilingualLabel>());
            var cancelRect=(RectTransform)dialog.cancel.transform; cancelRect.anchorMin=new Vector2(.15f,.06f); cancelRect.anchorMax=new Vector2(.85f,.23f);
            var prefab=PrefabUtility.SaveAsPrefabAsset(canvas.gameObject,Root+"Prefabs/ParentGate.prefab"); gate=prefab.GetComponent<ParentGateDialog>(); UnityEngine.Object.DestroyImmediate(canvas.gameObject);
        }

        [MenuItem("Fun Math/UI/2 - Apply to project scenes")]
        public static void ApplyAll()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildAssets();
            var setup = EditorSceneManager.GetSceneManagerSetup();
            var report = new List<string> { "scene\tbuttons\tpersistentCallsBefore\tpersistentCallsAfter\tmissingScripts" };
            try
            {
                foreach (string guid in AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"}))
                {
                    string path=AssetDatabase.GUIDToAssetPath(guid); var scene=EditorSceneManager.OpenScene(path);
                    int before=Calls(scene);
                    ApplyScene(scene);
                    int after=Calls(scene);
                    // New shell forwards existing events; extra copies are deliberate. No old event may disappear.
                    if(after<before) throw new InvalidOperationException("Lost persistent actions in "+path);
                    int missing=scene.GetRootGameObjects().Sum(r=>r.GetComponentsInChildren<Transform>(true).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)));
                    report.Add(path+"\t"+Components<Button>(scene).Count()+"\t"+before+"\t"+after+"\t"+missing);
                    EditorSceneManager.SaveScene(scene);
                }
                PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;
                PlayerSettings.allowedAutorotateToPortrait=true; PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
                PlayerSettings.allowedAutorotateToLandscapeLeft=PlayerSettings.allowedAutorotateToLandscapeRight=true;
                File.WriteAllLines("docs/tutorial/ui-migration-audit.tsv",report);
                AssetDatabase.SaveAssets();
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
            Debug.Log("Adventure UI applied to "+(report.Count-1)+" scenes. See docs/tutorial/ui-migration-audit.tsv.");
        }
        private static IEnumerable<T> Components<T>(Scene scene) where T:Component => scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<T>(true));
        private static int Calls(Scene scene) => Components<Button>(scene).Sum(b=>b.onClick.GetPersistentEventCount());

        [MenuItem("Fun Math/UI/5 - Audit scene wiring")]
        public static void AuditScenes()
        {
            if(EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            var setup=EditorSceneManager.GetSceneManagerSetup();
            var rows=new List<string>{"scene\tbuttons\twithoutFeedback\tmissingFonts\tmissingScripts\tmissingActionTargets\tresponsiveShell"};
            try
            {
                foreach(var guid in AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"}))
                {
                    var scene=EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guid)); var buttons=Components<Button>(scene).ToArray();
                    int noFeedback=buttons.Count(b=>!b.GetComponent<KidButton>());
                    int noFonts=Components<TMP_Text>(scene).Count(t=>!t.font);
                    int missing=Components<Transform>(scene).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
                    int targets=buttons.Sum(b=>Enumerable.Range(0,b.onClick.GetPersistentEventCount()).Count(i=>!b.onClick.GetPersistentTarget(i)));
                    bool shell=Components<Canvas>(scene).Any(c=>c.name=="AdventureShell");
                    rows.Add(scene.path+"\t"+buttons.Length+"\t"+noFeedback+"\t"+noFonts+"\t"+missing+"\t"+targets+"\t"+shell);
                }
                File.WriteAllLines("docs/tutorial/ui-scene-audit.tsv",rows);
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
            Debug.Log("Scene audit written to docs/tutorial/ui-scene-audit.tsv.");
        }

        private static void ApplyScene(Scene scene)
        {
            if (Components<Canvas>(scene).Any(c=>c.name=="AdventureShell")) return;
            foreach(var scaler in Components<CanvasScaler>(scene))
            { if(scaler.GetComponent<Canvas>().renderMode==RenderMode.WorldSpace) continue; scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand; }
            foreach(var text in Components<TMP_Text>(scene))
            { text.font=text.fontSize>=60 ? theme.displayFont : theme.bodyFont; text.color=theme.ink; }
            foreach(var button in Components<Button>(scene).ToArray())
            {
                var feedback=button.GetComponent<KidButton>(); if(!feedback) feedback=button.gameObject.AddComponent<KidButton>(); feedback.theme=theme;
                // Text-bearing navigation surfaces can be recoloured. Image-only learning/game content retains its art.
                var image=button.targetGraphic as Image;
                if(image && button.GetComponentInChildren<TMP_Text>(true) && !button.GetComponentInParent<TMP_Dropdown>())
                { image.sprite=theme.roundedPanel; image.type=Image.Type.Sliced; var surface=image.GetComponent<ThemedSurface>() ?? image.gameObject.AddComponent<ThemedSurface>(); surface.theme=theme; surface.section=0; surface.Apply(); }
                bool protect=false;
                for(int i=0;i<button.onClick.GetPersistentEventCount();i++)
                { string method=button.onClick.GetPersistentMethodName(i); if(method=="ClearAllKeys" || method=="ShowMenuOrHide" || method=="ChangeProfile" || method=="LogOut") protect=true; }
                if(protect) { var guard=button.GetComponent<ParentGate>() ?? button.gameObject.AddComponent<ParentGate>(); guard.dialogPrefab=gate; }
            }
            if(HubPaths.Contains(scene.path)) BuildShell(scene);
        }

        [MenuItem("Fun Math/UI/4 - Update content surfaces")]
        public static void UpdateContentSurfaces()
        {
            if(EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            BuildAssets(); var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach(var guid in AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"}))
                {
                    var scene=EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guid));
                    bool ar=scene.path.Contains("/AR/");
                    foreach(var image in Components<Image>(scene))
                    {
                        if(image.transform.root.name=="AdventureShell") continue;
                        bool panel=image.sprite && image.sprite.name=="BigPanel";
                        bool owl=image.sprite && image.sprite.name.IndexOf("owl",StringComparison.OrdinalIgnoreCase)>=0;
                        bool background=!ar && image.transform.parent && image.transform.parent.GetComponent<Canvas>() && image.rectTransform.rect.width>=800 && image.rectTransform.rect.height>=1000 && image.transform.childCount==0;
                        if(panel || background)
                        {
                            image.sprite=background ? null : theme.roundedPanel; image.type=background?Image.Type.Simple:Image.Type.Sliced;
                            var surface=image.GetComponent<ThemedSurface>() ?? image.gameObject.AddComponent<ThemedSurface>(); surface.theme=theme; surface.section=background?-2:-1; surface.Apply();
                            if(background) image.raycastTarget=false;
                        }
                        if(owl && theme.owl) { image.sprite=theme.owl; image.color=Color.white; image.preserveAspect=true; }
                        if(!ar && image.name=="BackgroundPanel") { image.color=Color.clear; image.raycastTarget=false; }
                    }
                    EditorSceneManager.SaveScene(scene);
                }
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }

        private static void BuildShell(Scene scene)
        {
            var sources=Components<Button>(scene).Where(b=>Enumerable.Range(0,b.onClick.GetPersistentEventCount()).Any(i=>b.onClick.GetPersistentMethodName(i)=="loadScene")).ToArray();
            foreach(var canvas in Components<Canvas>(scene).ToArray()) { canvas.enabled=false; var ray=canvas.GetComponent<GraphicRaycaster>(); if(ray) ray.enabled=false; }
            var shell=MakeCanvas("AdventureShell",100);
            var sky = Surface("Sky",shell.transform,-2,Vector2.zero,Vector2.one); sky.sprite = null;
            var safe=Rect("SafeArea",shell.transform,Vector2.zero,Vector2.one); safe.gameObject.AddComponent<SafeAreaPanel>();
            Label("Brand",safe,"FUN MATH","FUN MATH",34,new Vector2(.05f,.91f),new Vector2(.3f,.98f));
            Label("Title",safe,scene.name=="GetStarted" ? "Jom teroka matematik!" : scene.name=="MainMenu" ? "Hari ini, kita nak apa?" : "Jom belajar!",scene.name=="GetStarted" ? "A little maths. A big adventure!" : scene.name=="MainMenu" ? "What shall we explore?" : "Let's learn!",64,new Vector2(.06f,.76f),new Vector2(.94f,.9f));
            var owl=Surface("OwlGuide",safe,-1,new Vector2(.05f,.23f),new Vector2(.36f,.72f));
            UnityEngine.Object.DestroyImmediate(owl.GetComponent<ThemedSurface>()); owl.sprite=theme.owl; owl.color=Color.white; owl.type=Image.Type.Simple; owl.preserveAspect=true; owl.gameObject.AddComponent<OwlFloat>();
            var grid=Rect("Activities",safe,new Vector2(.4f,.23f),new Vector2(.94f,.72f)); grid.gameObject.AddComponent<GridLayoutGroup>(); var adaptive=grid.gameObject.AddComponent<AdaptiveMenu>(); adaptive.portraitColumns=scene.name=="MainMenu"?2:1; adaptive.landscapeColumns=scene.name=="MainMenu"?3:1;
            var layout=safe.gameObject.AddComponent<HubLayout>(); layout.owl=owl.rectTransform; layout.activities=grid;
            shell.gameObject.AddComponent<MenuFocus>().activities=grid;
            int index=0;
            foreach(var source in sources.OrderBy(b=>b.name))
            {
                var texts=source.GetComponentsInChildren<TMP_Text>(true); string bm=texts.FirstOrDefault()?.text ?? source.name; string en=texts.LastOrDefault()?.text ?? bm;
                if(texts.Length==0 && scene.name!="GetStarted")
                {
                    var back=MakeButton("Back",safe,"Kembali","Back",-1); back.onClick=source.onClick;
                    var backRect=(RectTransform)back.transform; backRect.anchorMin=new Vector2(.7f,.025f); backRect.anchorMax=new Vector2(.94f,.09f);
                    continue;
                }
                var button=MakeButton("Activity_"+source.name,grid,bm,en,index++ % theme.sections.Length);
                button.onClick=source.onClick;
                button.gameObject.AddComponent<NavigationState>().source=source;
                var art = source.GetComponentsInChildren<Image>(true).FirstOrDefault(i=>i.sprite && i != source.targetGraphic && i.rectTransform.rect.width > 60 && i.rectTransform.rect.height > 60);
                Sprite iconSprite = art ? art.sprite : null;
                if (scene.name=="MainMenu")
                {
                    string[] icons={"learning.png","games.png","math-class.png","achievement.png","piala.png","lightbulb.png","archive.png"};
                    iconSprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/MainMenu/"+icons[Mathf.Min(index-1,icons.Length-1)]);
                }
                if (iconSprite)
                {
                    var icon=Surface("ActivityArt",button.transform,-1,new Vector2(.32f,.4f),new Vector2(.68f,.92f));
                    UnityEngine.Object.DestroyImmediate(icon.GetComponent<ThemedSurface>()); icon.sprite=iconSprite; icon.color=Color.white; icon.type=Image.Type.Simple; icon.preserveAspect=true;
                    var label=button.GetComponentInChildren<TMP_Text>(); var rt=(RectTransform)label.transform; rt.anchorMin=new Vector2(.04f,.03f); rt.anchorMax=new Vector2(.96f,.36f);
                }
            }
            if(sources.Length==0) Debug.LogWarning("No navigation found for "+scene.path);
            var voice=shell.gameObject.AddComponent<VoiceGuide>(); var audio=shell.gameObject.AddComponent<AudioSource>(); audio.playOnAwake=false;
            voice.englishRecording=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"Audio/Welcome-en.wav");
            voice.status=Label("Instruction",safe,voice.malay,voice.english,30,new Vector2(.08f,.12f),new Vector2(.92f,.21f));
            // Replay text is owned by VoiceGuide after user interaction.
            UnityEngine.Object.DestroyImmediate(voice.status.GetComponent<BilingualLabel>());
            var replay=MakeButton("Listen",safe,"Dengar","Listen",2); var rr=(RectTransform)replay.transform; rr.anchorMin=new Vector2(.72f,.92f); rr.anchorMax=new Vector2(.94f,.98f); UnityEventTools.AddPersistentListener(replay.onClick,voice.Speak);
            BuildSettings(safe,shell);
            if (scene.name == "MainMenu") ProfileUIBuilder.AttachMainMenuProfile(safe);
        }

        private static void BuildSettings(Transform safe,Canvas canvas)
        {
            var panel=Surface("ParentSettings",safe,-1,new Vector2(.08f,.12f),new Vector2(.92f,.89f)); panel.raycastTarget=true;
            var settings=panel.gameObject.AddComponent<AdventureSettings>();
            Label("Heading",panel.transform,"Tetapan ibu bapa","Parent settings",54,new Vector2(.05f,.82f),new Vector2(.95f,.97f));
            settings.music=SliderRow(panel.transform,"Muzik / Music",.66f);
            settings.effects=SliderRow(panel.transform,"Bunyi / Sound",.48f);
            settings.reduceMotion=ToggleRow(panel.transform,"Kurangkan gerakan / Reduce motion",.33f);
            settings.voice=ToggleRow(panel.transform,"Panduan suara / Voice guidance",.2f);
            var language=MakeButton("Language",panel.transform,"Bahasa / Language","Bahasa / Language",2); var lr=(RectTransform)language.transform; lr.anchorMin=new Vector2(.06f,.04f); lr.anchorMax=new Vector2(.57f,.16f); UnityEventTools.AddPersistentListener(language.onClick,settings.ChangeLanguage);
            var close=MakeButton("Close",panel.transform,"Kembali","Back",1); var cr=(RectTransform)close.transform; cr.anchorMin=new Vector2(.62f,.04f); cr.anchorMax=new Vector2(.94f,.16f); UnityEventTools.AddBoolPersistentListener(close.onClick,panel.gameObject.SetActive,false);
            panel.gameObject.SetActive(false);
            var open=MakeButton("Parents",safe,"Ibu bapa","Parents",-1); var rect=(RectTransform)open.transform; rect.anchorMin=new Vector2(.05f,.025f); rect.anchorMax=new Vector2(.32f,.09f);
            UnityEventTools.AddBoolPersistentListener(open.onClick,panel.gameObject.SetActive,true); var guard=open.gameObject.AddComponent<ParentGate>(); guard.dialogPrefab=gate;
            panel.transform.SetAsLastSibling();
        }
        private static Slider SliderRow(Transform parent,string text,float y)
        {
            Label(text,parent,text,text,32,new Vector2(.06f,y+.06f),new Vector2(.94f,y+.14f));
            var track=Surface(text+"Slider",parent,2,new Vector2(.08f,y),new Vector2(.92f,y+.045f)); track.raycastTarget=true;
            var slider=track.gameObject.AddComponent<Slider>();
            var handle=Surface("Handle",track.transform,0,new Vector2(0,.5f),new Vector2(0,.5f)); handle.rectTransform.sizeDelta=new Vector2(64,64); handle.raycastTarget=true;
            slider.handleRect=handle.rectTransform; slider.targetGraphic=handle; slider.minValue=0; slider.maxValue=1; slider.value=1; return slider;
        }
        private static Toggle ToggleRow(Transform parent,string text,float y)
        {
            var image=Surface(text,parent,-2,new Vector2(.06f,y),new Vector2(.94f,y+.1f)); image.raycastTarget=true;
            Label("Label",image.transform,text,text,30,new Vector2(.12f,.05f),new Vector2(.98f,.95f));
            var check=Surface("Check",image.transform,1,new Vector2(.02f,.18f),new Vector2(.1f,.82f));
            var toggle=image.gameObject.AddComponent<Toggle>(); toggle.targetGraphic=image; toggle.graphic=check; return toggle;
        }
    }
}

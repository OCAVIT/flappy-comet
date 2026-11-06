using System;
using System.IO;
using CometSystem;
using GameSystem;
using SpawnSystem;
using UISystem;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BuildSystem
{
    // Собирает игру из кода: спрайты, материалы, префабы, сцену и настройки проекта, а также WebGL-билд.
    // Результат уже лежит в репозитории; пункт меню нужен, чтобы всё пересоздать.
    // Экран рассчитан на 16:10 и 10 единиц по высоте, размеры переведены из пикселей референса (953×595, 59,5 px на единицу).
    public static class GameBuilder
    {
        const string SpritesFolder = "Assets/_Presentation/Sprites";
        const string MaterialsFolder = "Assets/_Presentation/Materials";
        const string PrefabsFolder = "Assets/_Presentation/Prefabs";
        const string SettingsPath = "Assets/_Presentation/Settings/GameSettings.asset";
        const string ScenePath = "Assets/_Presentation/Scenes/Game.unity";
        const string WebGLOutput = "Builds/WebGL";

        const float CometDiameter = 0.706f;
        const float CometX = -5.15f;
        const float StarHeight = 0.47f;
        const float TopBand = 0.975f;
        const float BottomBand = 0.958f;

        static readonly Color BandColor = Hex("244811");
        static readonly Color CometColor = Hex("41B12B");
        static readonly Color MeteorColor = Hex("8E4141");
        static readonly Color StarColor = Hex("FFBB00");

        [MenuItem("Flappy Comet/Rebuild Scene and Prefabs", priority = 1)]
        public static void Rebuild()
        {
            ApplySettings();

            // Сцена создаётся первой: новая сцена выгружает из памяти уже загруженные ассеты, и ссылки на префабы обнулились бы.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Sprite circle = MakeSprite("Circle", 256, Disc);
            Sprite star = MakeSprite("Star", 256, Hexagram);
            Sprite square = MakeSprite("Square", 8, (x, y) => 1f);
            Material trailMaterial = MakeParticleMaterial("Trail", MakeTexture("SoftCircle", 128, SoftDisc));
            Material sparkMaterial = MakeParticleMaterial("Spark", MakeTexture("SparkDot", 64, Glow));

            Meteor meteorPrefab = MakeMeteor(square, trailMaterial);
            Star starPrefab = MakeStar(star);

            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            var view = cameraObject.GetComponent<Camera>();
            view.orthographic = true;
            view.orthographicSize = 5f;
            view.clearFlags = CameraClearFlags.SolidColor;
            view.backgroundColor = Color.black;

            MakeBand("Top Band", 5f - TopBand / 2f, TopBand, square);
            MakeBand("Bottom Band", -5f + BottomBand / 2f, BottomBand, square);

            Comet comet = MakeComet(circle, trailMaterial);
            SparkField sparks = MakeSparks(sparkMaterial);

            GameHudView hud = MakeHud();

            // Сцена хранит только ссылки корня композиции: кому что нужно, решает контейнер.
            var scope = new GameObject("Game Lifetime Scope").AddComponent<GameLifetimeScope>();
            Set(scope, "settings", LoadOrCreateSettings());
            Set(scope, "gameCamera", view);
            Set(scope, "comet", comet);
            Set(scope, "sparks", sparks);
            Set(scope, "hud", hud);
            Set(scope, "meteorPrefab", meteorPrefab);
            Set(scope, "starPrefab", starPrefab);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log($"[GameBuilder] Scene saved: {ScenePath}");
        }

        [MenuItem("Flappy Comet/Build WebGL", priority = 20)]
        public static void BuildWebGL()
        {
            ApplySettings();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = WebGLOutput,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });
            var summary = report.summary;
            Debug.Log($"[GameBuilder] WebGL build {summary.result}: {summary.outputPath}, " +
                      $"{summary.totalSize / 1048576f:0.0} MB, {summary.totalTime.TotalSeconds:0} s");
            if (Application.isBatchMode && summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
        }

        static void ApplySettings()
        {
            PlayerSettings.companyName = "KTSHKI";
            PlayerSettings.productName = "Flappy Comet";
            PlayerSettings.bundleVersion = "1.0";
            PlayerSettings.defaultWebScreenWidth = 960;
            PlayerSettings.defaultWebScreenHeight = 600;
            PlayerSettings.runInBackground = false;
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.High);
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.WebGL, Il2CppCodeGeneration.OptimizeSize);
            PlayerSettings.WebGL.template = "PROJECT:FlappyComet";
            // GitHub Pages не отдаёт заголовок Content-Encoding для готовых .br, поэтому файлы распаковывает сам загрузчик.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.nameFilesAsHashes = true;
            PlayerSettings.WebGL.dataCaching = true;
        }

        // Настройки баланса переживают пересборку сцены: существующий ассет не перезаписывается.
        static GameSettings LoadOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<GameSettings>(SettingsPath);
            if (settings != null)
            {
                // Файл ассета переписывается по текущей структуре настроек, значения остаются.
                EditorUtility.SetDirty(settings);
                return settings;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
            settings = ScriptableObject.CreateInstance<GameSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
            return settings;
        }

        // ---------- Объекты сцены ----------

        static void MakeBand(string name, float y, float height, Sprite square)
        {
            var band = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Hazard));
            band.transform.position = new Vector3(0f, y);
            // С запасом по ширине: в окне редактора экран бывает шире 16:10.
            band.transform.localScale = new Vector3(60f, height, 1f);
            var sprite = band.GetComponent<SpriteRenderer>();
            sprite.sprite = square;
            sprite.color = BandColor;
            sprite.sortingOrder = 10;
            // Размер задаём явно: коллайдер добавлен раньше спрайта и подстроился под пустой спрайт почти в ноль.
            var collider = band.GetComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;
        }

        static Comet MakeComet(Sprite circle, Material trailMaterial)
        {
            var root = new GameObject("Comet");
            root.transform.position = new Vector3(CometX, 0f);

            var body = new GameObject("Body", typeof(SpriteRenderer));
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = Vector3.one * CometDiameter;
            var sprite = body.GetComponent<SpriteRenderer>();
            sprite.sprite = circle;
            sprite.color = CometColor;
            sprite.sortingOrder = 4;

            var rigidbody = root.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Kinematic;
            var collider = root.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = CometDiameter * 0.46f;

            var comet = root.AddComponent<Comet>();
            Set(comet, "trail", MakeCometTrail(root.transform, trailMaterial));
            return comet;
        }

        // Общий вид следа: широкая лента, которая к хвосту сужается и темнеет.
        static ParticleSystem MakeTrail(Transform parent, Material material, Color color, float width)
        {
            var go = new GameObject("Trail");
            go.transform.SetParent(parent, false);
            var trail = go.AddComponent<ParticleSystem>();
            trail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = trail.main;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startSpeed = 0f;
            main.startLifetime = 0.72f;
            main.startSize = new ParticleSystem.MinMaxCurve(width * 0.86f, width * 0.96f);
            main.startColor = color;
            main.maxParticles = 300;

            var emission = trail.emission;
            emission.rateOverTime = 110f;
            var shape = trail.shape;
            shape.enabled = false;

            var size = trail.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.36f));

            var fade = trail.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.68f, 0.68f, 0.68f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.45f, 1f) });
            fade.color = gradient;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortMode = ParticleSystemSortMode.YoungestInFront;
            renderer.sortingOrder = 1;
            return trail;
        }

        // След кометы уносит влево со скоростью мира, поэтому он повторяет её путь. Скорость выставляет Comet.
        static ParticleSystem MakeCometTrail(Transform parent, Material material)
        {
            ParticleSystem trail = MakeTrail(parent, material, CometColor, CometDiameter);
            var main = trail.main;
            main.playOnAwake = false;
            var velocity = trail.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = -5f;
            velocity.y = 0f;
            velocity.z = 0f;
            return trail;
        }

        // След метеорита остаётся там, где он пролетел. Ширина следует за размером метеорита.
        static ParticleSystem MakeMeteorTrail(Transform parent, Material material)
        {
            ParticleSystem trail = MakeTrail(parent, material, MeteorColor, 1f);
            var main = trail.main;
            main.playOnAwake = true;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            return trail;
        }

        static SparkField MakeSparks(Material material)
        {
            var go = new GameObject("Sparks");
            var particles = go.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = particles.main;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startSpeed = 0f;
            main.startColor = StarColor;
            main.maxParticles = 200;
            var emission = particles.emission;
            emission.enabled = false;
            var shape = particles.shape;
            shape.enabled = false;

            var color = particles.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.85f, 0.15f), new GradientAlphaKey(0f, 1f) });
            color.color = gradient;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingOrder = 2;

            var field = go.AddComponent<SparkField>();
            Set(field, "field", particles);
            return field;
        }

        static Meteor MakeMeteor(Sprite square, Material trailMaterial)
        {
            var go = new GameObject("Meteor", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D));
            var sprite = go.GetComponent<SpriteRenderer>();
            sprite.sprite = square;
            sprite.color = MeteorColor;
            sprite.sortingOrder = 3;
            var collider = go.GetComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one * 0.94f;
            go.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            go.AddComponent<Hazard>();
            var meteor = go.AddComponent<Meteor>();
            Set(meteor, "trail", MakeMeteorTrail(go.transform, trailMaterial));
            return SavePrefab(go).GetComponent<Meteor>();
        }

        static Star MakeStar(Sprite star)
        {
            var go = new GameObject("Star", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D));
            go.transform.localScale = Vector3.one * StarHeight;
            var sprite = go.GetComponent<SpriteRenderer>();
            sprite.sprite = star;
            sprite.color = StarColor;
            sprite.sortingOrder = 2;
            var collider = go.GetComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.7f;
            go.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            go.AddComponent<Scroller>();
            go.AddComponent<Star>();
            return SavePrefab(go).GetComponent<Star>();
        }

        static GameObject SavePrefab(GameObject go)
        {
            Directory.CreateDirectory(PrefabsFolder);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabsFolder}/{go.name}.prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }

        static GameHudView MakeHud()
        {
            var canvasObject = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(953f, 595f);
            scaler.matchWidthOrHeight = 0.5f;

            Text score = MakeText(canvasObject.transform, "Score", new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -31f), 28, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            Text message = MakeText(canvasObject.transform, "Message", new Vector2(0.279f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(0f, 1.5f), 20, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft);
            Text hint = MakeText(canvasObject.transform, "Controls Hint", new Vector2(0.279f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(0f, -25f), 15, FontStyle.Normal, new Color(0.62f, 0.68f, 0.6f), TextAnchor.MiddleLeft);
            hint.text = "Hold the left mouse button to fly up";

            var hud = canvasObject.AddComponent<GameHudView>();
            Set(hud, "score", score);
            Set(hud, "message", message);
            Set(hud, "controlsHint", hint);
            return hud;
        }

        static Text MakeText(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 position, int size,
            FontStyle style, Color color, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(640f, 40f);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        static void Set(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- Текстуры: формы заданы функцией покрытия от координат -1..1 ----------

        static float Disc(float x, float y)
        {
            return Mathf.Clamp01((1f - Mathf.Sqrt(x * x + y * y)) * 96f);
        }

        static float SoftDisc(float x, float y)
        {
            return 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.82f, 1f, Mathf.Sqrt(x * x + y * y)));
        }

        static float Glow(float x, float y)
        {
            float k = Mathf.Clamp01(1f - Mathf.Sqrt(x * x + y * y));
            return k * k;
        }

        // Шестиконечная звезда: два равносторонних треугольника, остриём вверх и вниз.
        static float Hexagram(float x, float y)
        {
            int inside = 0;
            const int Samples = 4;
            const float Pixel = 2f / 256f;
            for (int i = 0; i < Samples; i++)
            {
                for (int j = 0; j < Samples; j++)
                {
                    float sx = x + (i + 0.5f - Samples / 2f) / Samples * Pixel;
                    float sy = y + (j + 0.5f - Samples / 2f) / Samples * Pixel;
                    if (InTriangle(sx, sy) || InTriangle(sx, -sy)) inside++;
                }
            }
            return inside / (float)(Samples * Samples);
        }

        static bool InTriangle(float x, float y)
        {
            // Треугольник остриём вверх, вписанный в окружность радиуса 1: основание на y = -0.5.
            const float Slope = 1.7320508f;
            return y >= -0.5f && y <= 1f - Slope * Mathf.Abs(x);
        }

        static Sprite MakeSprite(string name, int size, Func<float, float, float> coverage)
        {
            string path = WritePng(name, size, coverage);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = size;
            Configure(importer);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static Texture2D MakeTexture(string name, int size, Func<float, float, float> coverage)
        {
            string path = WritePng(name, size, coverage);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            Configure(importer);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static void Configure(TextureImporter importer)
        {
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        static string WritePng(string name, int size, Func<float, float, float> coverage)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int py = 0; py < size; py++)
            {
                for (int px = 0; px < size; px++)
                {
                    float x = (px + 0.5f) / size * 2f - 1f;
                    float y = (py + 0.5f) / size * 2f - 1f;
                    texture.SetPixel(px, py, new Color(1f, 1f, 1f, coverage(x, y)));
                }
            }
            byte[] png = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);

            Directory.CreateDirectory(SpritesFolder);
            string path = $"{SpritesFolder}/{name}.png";
            File.WriteAllBytes(path, png);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            return path;
        }

        static Material MakeParticleMaterial(string name, Texture2D texture)
        {
            Directory.CreateDirectory(MaterialsFolder);
            string path = $"{MaterialsFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Mobile/Particles/Alpha Blended"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.mainTexture = texture;
            EditorUtility.SetDirty(material);
            return material;
        }

        static Color Hex(string rgb)
        {
            ColorUtility.TryParseHtmlString("#" + rgb, out Color color);
            return color;
        }
    }
}

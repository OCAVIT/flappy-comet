using System.Collections.Generic;
using GameSystem;
using CometSystem;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using WorldSystem;

namespace SpawnSystem
{
    // Выпускает из-за правого края экрана метеориты, каждый на высоте кометы, и звёзды на случайной высоте.
    // Промежутки считаются в пройденном пути, а с ростом сложности метеориты идут чаще.
    public class Spawner : MonoBehaviour
    {
        [SerializeField] Meteor meteorPrefab;
        [SerializeField] Star starPrefab;

        readonly List<GameObject> spawned = new List<GameObject>();
        IObjectResolver resolver;
        IGameSession session;
        IWorldSpeed world;
        IComet comet;
        SparkField sparks;
        SpawnSettings settings;
        bool running;
        float untilMeteor;
        float untilStar;

        float HalfWidth => Camera.main.orthographicSize * Camera.main.aspect;

        [Inject]
        public void Construct(IObjectResolver objectResolver, IGameSession gameSession, IWorldSpeed worldSpeed, IComet cometInScene, SparkField sparkField, SpawnSettings spawnSettings)
        {
            resolver = objectResolver;
            session = gameSession;
            world = worldSpeed;
            comet = cometInScene;
            sparks = sparkField;
            settings = spawnSettings;
            session.RunStarted += Begin;
            session.RunEnded += Stop;
        }

        void OnDestroy()
        {
            session.RunStarted -= Begin;
            session.RunEnded -= Stop;
        }

        void Begin()
        {
            foreach (var item in spawned)
                if (item != null) Destroy(item);
            spawned.Clear();
            sparks.Clear();
            running = true;
            untilMeteor = settings.firstMeteorAfter;
            untilStar = settings.firstStarAfter;
        }

        void Stop()
        {
            running = false;
        }

        void Update()
        {
            if (!running) return;

            float moved = world.Current * Time.deltaTime;
            untilMeteor -= moved;
            untilStar -= moved;

            float x = HalfWidth + settings.offscreenMargin;
            if (untilMeteor <= 0f)
            {
                SpawnMeteor(x, comet.Height);
                Vector2 gap = Vector2.Lerp(settings.meteorGapEasy, settings.meteorGapHard, world.Difficulty);
                untilMeteor += Random.Range(gap.x, gap.y);
            }
            if (untilStar <= 0f)
            {
                SpawnStar(x, Random.Range(-settings.fieldHalfHeight, settings.fieldHalfHeight));
                untilStar += Random.Range(settings.starGap.x, settings.starGap.y);
            }
        }

        public Meteor SpawnMeteor(float x, float y)
        {
            Meteor meteor = resolver.Instantiate(meteorPrefab, new Vector3(x, y), Quaternion.identity);
            meteor.Launch(world.Current + Random.Range(settings.meteorExtraSpeed.x, settings.meteorExtraSpeed.y), -HalfWidth - settings.offscreenMargin);
            spawned.RemoveAll(item => item == null);
            spawned.Add(meteor.gameObject);
            return meteor;
        }

        public Star SpawnStar(float x, float y)
        {
            Star star = resolver.Instantiate(starPrefab, new Vector3(x, y), Quaternion.identity);
            star.GetComponent<Scroller>().RemoveAt(-HalfWidth - settings.offscreenMargin);
            spawned.RemoveAll(item => item == null);
            spawned.Add(star.gameObject);
            return star;
        }
    }
}

using System.Collections.Generic;
using GameSystem;
using UnityEngine;

namespace SpawnSystem
{
    // Выпускает из-за правого края экрана метеориты, каждый на высоте кометы, и звёзды на случайной высоте.
    // Промежутки считаются в пройденном пути, а с ростом сложности метеориты идут чаще.
    public class Spawner : MonoBehaviour
    {
        [SerializeField] GameController game;
        [SerializeField] Transform comet;
        [SerializeField] Meteor meteorPrefab;
        [SerializeField] Star starPrefab;
        [SerializeField] SparkField sparks;

        [Tooltip("Звезда появляется не дальше этого от центра по вертикали")]
        [SerializeField] float fieldHalfHeight = 3.55f;
        [Tooltip("Запас за краем экрана: объект появляется и исчезает невидимо")]
        [SerializeField] float offscreenMargin = 1.5f;
        [Tooltip("На сколько метеорит быстрее мира, единиц в секунду")]
        [SerializeField] Vector2 meteorExtraSpeed = new Vector2(2f, 4f);

        [Header("Промежутки, единиц пути")]
        [SerializeField] Vector2 meteorGapEasy = new Vector2(5.5f, 7.5f);
        [SerializeField] Vector2 meteorGapHard = new Vector2(4.5f, 6f);
        [SerializeField] Vector2 starGap = new Vector2(7f, 11f);

        readonly List<GameObject> spawned = new List<GameObject>();
        bool running;
        float untilMeteor;
        float untilStar;

        float HalfWidth => Camera.main.orthographicSize * Camera.main.aspect;

        public void Begin()
        {
            foreach (var item in spawned)
                if (item != null) Destroy(item);
            spawned.Clear();
            sparks.Clear();
            running = true;
            untilMeteor = 3f;
            untilStar = 4f;
        }

        public void Stop()
        {
            running = false;
        }

        void Update()
        {
            if (!running) return;

            float moved = game.WorldSpeed * Time.deltaTime;
            untilMeteor -= moved;
            untilStar -= moved;

            float x = HalfWidth + offscreenMargin;
            if (untilMeteor <= 0f)
            {
                SpawnMeteor(x, comet.position.y);
                Vector2 gap = Vector2.Lerp(meteorGapEasy, meteorGapHard, game.Difficulty);
                untilMeteor += Random.Range(gap.x, gap.y);
            }
            if (untilStar <= 0f)
            {
                SpawnStar(x, Random.Range(-fieldHalfHeight, fieldHalfHeight));
                untilStar += Random.Range(starGap.x, starGap.y);
            }
        }

        public Meteor SpawnMeteor(float x, float y)
        {
            Meteor meteor = Instantiate(meteorPrefab, new Vector3(x, y), Quaternion.identity);
            meteor.Launch(game.WorldSpeed + Random.Range(meteorExtraSpeed.x, meteorExtraSpeed.y), -HalfWidth - offscreenMargin);
            spawned.RemoveAll(item => item == null);
            spawned.Add(meteor.gameObject);
            return meteor;
        }

        public Star SpawnStar(float x, float y)
        {
            Star star = Instantiate(starPrefab, new Vector3(x, y), Quaternion.identity);
            star.GetComponent<Scroller>().Init(game, -HalfWidth - offscreenMargin);
            star.Init(sparks);
            spawned.RemoveAll(item => item == null);
            spawned.Add(star.gameObject);
            return star;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SpawnSystem
{
    public interface IObstacleFactory
    {
        Meteor CreateMeteor(Vector2 position, float speed);
        Star CreateStar(Vector2 position);
        void Clear();
    }

    // Создаёт объекты через контейнер, поэтому зависимости префабов приходят сами, а фабрика о них не знает.
    public sealed class ObstacleFactory : IObstacleFactory
    {
        readonly IObjectResolver resolver;
        readonly Meteor meteorPrefab;
        readonly Star starPrefab;
        readonly List<GameObject> spawned = new List<GameObject>();

        [Inject]
        public ObstacleFactory(IObjectResolver resolver, Meteor meteorPrefab, Star starPrefab)
        {
            this.resolver = resolver;
            this.meteorPrefab = meteorPrefab;
            this.starPrefab = starPrefab;
        }

        public Meteor CreateMeteor(Vector2 position, float speed)
        {
            Meteor meteor = resolver.Instantiate(meteorPrefab, position, Quaternion.identity);
            meteor.Launch(speed);
            Track(meteor.gameObject);
            return meteor;
        }

        public Star CreateStar(Vector2 position)
        {
            Star star = resolver.Instantiate(starPrefab, position, Quaternion.identity);
            Track(star.gameObject);
            return star;
        }

        public void Clear()
        {
            foreach (GameObject item in spawned)
                if (item != null) Object.Destroy(item);
            spawned.Clear();
        }

        void Track(GameObject item)
        {
            spawned.RemoveAll(existing => existing == null);
            spawned.Add(item);
        }
    }
}

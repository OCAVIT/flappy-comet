using UnityEngine;
using VContainer;
using WorldSystem;

namespace SpawnSystem
{
    // Несёт объект влево со скоростью мира и удаляет его за левым краем экрана.
    public class Scroller : MonoBehaviour
    {
        IWorldSpeed world;
        float leftEdge;

        [Inject]
        public void Construct(IWorldSpeed worldSpeed)
        {
            world = worldSpeed;
        }

        public void RemoveAt(float destroyAtX)
        {
            leftEdge = destroyAtX;
        }

        void Update()
        {
            transform.position += Vector3.left * (world.Current * Time.deltaTime);
            if (transform.position.x < leftEdge) Destroy(gameObject);
        }
    }
}

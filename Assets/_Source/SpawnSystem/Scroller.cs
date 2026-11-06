using GameSystem;
using UnityEngine;
using VContainer;

namespace SpawnSystem
{
    // Несёт объект влево со скоростью мира и удаляет его за левым краем экрана.
    public class Scroller : MonoBehaviour
    {
        GameController game;
        float leftEdge;

        [Inject]
        public void Construct(GameController controller)
        {
            game = controller;
        }

        public void RemoveAt(float destroyAtX)
        {
            leftEdge = destroyAtX;
        }

        void Update()
        {
            transform.position += Vector3.left * (game.WorldSpeed * Time.deltaTime);
            if (transform.position.x < leftEdge) Destroy(gameObject);
        }
    }
}

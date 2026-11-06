using GameSystem;
using UnityEngine;

namespace SpawnSystem
{
    // Несёт объект влево со скоростью мира и удаляет его за левым краем экрана.
    public class Scroller : MonoBehaviour
    {
        GameController game;
        float leftEdge;

        public void Init(GameController controller, float destroyAtX)
        {
            game = controller;
            leftEdge = destroyAtX;
        }

        void Update()
        {
            transform.position += Vector3.left * (game.WorldSpeed * Time.deltaTime);
            if (transform.position.x < leftEdge) Destroy(gameObject);
        }
    }
}

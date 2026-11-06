using UnityEngine;
using VContainer;
using WorldSystem;

namespace SpawnSystem
{
    // Несёт объект влево со скоростью мира и удаляет его за левым краем экрана.
    public class Scroller : MonoBehaviour
    {
        IWorldSpeed world;
        IPlayField field;

        [Inject]
        public void Construct(IWorldSpeed worldSpeed, IPlayField playField)
        {
            world = worldSpeed;
            field = playField;
        }

        void Update()
        {
            transform.position += Vector3.left * (world.Current * Time.deltaTime);
            if (transform.position.x < field.ExitX) Destroy(gameObject);
        }
    }
}

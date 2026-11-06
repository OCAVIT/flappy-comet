using UnityEngine;

namespace SpawnSystem
{
    public interface ISparks
    {
        void Shed(Vector3 position);
        void Burst(Vector3 position);
        void Clear();
    }
}

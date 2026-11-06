using UnityEngine;
using VContainer;

namespace SpawnSystem
{
    // Где объекты появляются и где их пора убирать.
    public interface IPlayField
    {
        float SpawnX { get; }
        float ExitX { get; }
    }

    // Границы считаются по камере: так они верны при любом соотношении сторон окна.
    public sealed class CameraPlayField : IPlayField
    {
        readonly Camera view;
        readonly SpawnSettings settings;

        [Inject]
        public CameraPlayField(Camera view, SpawnSettings settings)
        {
            this.view = view;
            this.settings = settings;
        }

        float HalfWidth => view.orthographicSize * view.aspect;

        public float SpawnX => HalfWidth + settings.offscreenMargin;
        public float ExitX => -HalfWidth - settings.offscreenMargin;
    }
}

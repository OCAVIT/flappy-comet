using UnityEngine;
using VContainer;

namespace CometSystem
{
    // Один вопрос к игроку: зажата ли кнопка. Откуда берётся ответ, знает только реализация.
    public interface IFlightInput
    {
        bool Held { get; }
    }

    // Зажатая ЛКМ или пробел. На телефоне первое касание Unity тоже считает левой кнопкой мыши.
    public sealed class MouseAndSpaceFlightInput : IFlightInput
    {
        [Inject]
        public MouseAndSpaceFlightInput()
        {
        }

        public bool Held => Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);
    }
}

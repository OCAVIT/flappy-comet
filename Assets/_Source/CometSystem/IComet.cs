using System;

namespace CometSystem
{
    // Что остальной игре нужно от кометы. Как она летит и рисует след, снаружи не видно.
    public interface IComet
    {
        event Action StarCollected;
        event Action Crashed;

        float Height { get; }

        void Launch();
    }
}

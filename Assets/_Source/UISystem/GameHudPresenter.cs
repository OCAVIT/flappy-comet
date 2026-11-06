using System;
using GameSystem;
using VContainer;
using VContainer.Unity;

namespace UISystem
{
    // Показывает состояние игры: счёт, надпись на старте и после столкновения.
    public sealed class GameHudPresenter : IInitializable, IDisposable
    {
        const string StartText = "Press SPACEBAR to start";
        const string RestartText = "Game over. Press SPACEBAR to restart";

        readonly IGameSession session;
        readonly IGameHudView view;

        [Inject]
        public GameHudPresenter(IGameSession session, IGameHudView view)
        {
            this.session = session;
            this.view = view;
        }

        public void Initialize()
        {
            session.Changed += Refresh;
            Refresh();
        }

        public void Dispose()
        {
            session.Changed -= Refresh;
        }

        void Refresh()
        {
            view.ShowScore(session.Score);
            view.ShowControlsHint(session.State == GameState.Ready);
            switch (session.State)
            {
                case GameState.Ready:
                    view.ShowMessage(StartText);
                    break;
                case GameState.GameOver:
                    view.ShowMessage(RestartText);
                    break;
                default:
                    view.HideMessage();
                    break;
            }
        }
    }
}

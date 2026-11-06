using GameSystem;
using NUnit.Framework;
using UISystem;

namespace FlappyCometTests
{
    public class GameHudPresenterTests
    {
        GameSession session;
        FakeHudView view;
        GameHudPresenter presenter;

        [SetUp]
        public void SetUp()
        {
            session = new GameSession();
            view = new FakeHudView();
            presenter = new GameHudPresenter(session, view);
            presenter.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            presenter.Dispose();
        }

        [Test]
        public void OnTheStartScreen_ShowsTheReferencePrompt_AndTheControlsHint()
        {
            Assert.That(view.Score, Is.EqualTo(0));
            Assert.That(view.Message, Is.EqualTo("Press SPACEBAR to start"));
            Assert.That(view.HintVisible, Is.True);
        }

        [Test]
        public void DuringTheRun_ShowsOnlyTheScore()
        {
            session.StartRun();
            session.AddPoint();

            Assert.That(view.Score, Is.EqualTo(1));
            Assert.That(view.Message, Is.Null);
            Assert.That(view.HintVisible, Is.False);
        }

        [Test]
        public void AfterTheCrash_OffersARestart_AndKeepsTheScore()
        {
            session.StartRun();
            session.AddPoint();
            session.EndRun();

            Assert.That(view.Score, Is.EqualTo(1));
            Assert.That(view.Message, Is.EqualTo("Game over. Press SPACEBAR to restart"));
            Assert.That(view.HintVisible, Is.False);
        }

        [Test]
        public void AfterDispose_NoLongerListens()
        {
            presenter.Dispose();

            session.StartRun();

            Assert.That(view.Message, Is.EqualTo("Press SPACEBAR to start"));
        }
    }
}

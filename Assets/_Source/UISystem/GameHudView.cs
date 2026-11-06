using UnityEngine;
using UnityEngine.UI;

namespace UISystem
{
    public interface IGameHudView
    {
        void ShowScore(int score);
        void ShowMessage(string text);
        void HideMessage();
        void ShowControlsHint(bool visible);
    }

    // Тексты на экране: счёт в верхней полосе и надпись рядом с кометой. Что показывать, решает GameHudPresenter.
    public class GameHudView : MonoBehaviour, IGameHudView
    {
        [SerializeField] Text score;
        [SerializeField] Text message;
        [SerializeField] Text controlsHint;

        public void ShowScore(int value)
        {
            score.text = value.ToString();
        }

        public void ShowMessage(string text)
        {
            message.text = text;
            message.enabled = true;
        }

        public void HideMessage()
        {
            message.enabled = false;
        }

        public void ShowControlsHint(bool visible)
        {
            controlsHint.enabled = visible;
        }
    }
}

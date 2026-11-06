using GameSystem;
using UnityEngine;
using UnityEngine.UI;

namespace UISystem
{
    // Счёт в верхней полосе и надпись рядом с кометой: на старте и после столкновения.
    public class GameHud : MonoBehaviour
    {
        const string StartText = "Press SPACEBAR to start";
        const string RestartText = "Game over. Press SPACEBAR to restart";

        [SerializeField] GameController game;
        [SerializeField] Text score;
        [SerializeField] Text message;
        [SerializeField] Text controlsHint;

        void OnEnable()
        {
            game.Changed += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            game.Changed -= Refresh;
        }

        void Refresh()
        {
            score.text = game.Score.ToString();
            message.enabled = game.State != GameState.Playing;
            message.text = game.State == GameState.GameOver ? RestartText : StartText;
            controlsHint.enabled = game.State == GameState.Ready;
        }
    }
}

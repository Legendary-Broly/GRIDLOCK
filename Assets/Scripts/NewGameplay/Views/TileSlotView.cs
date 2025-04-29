// Assets/Scripts/Views/TileSlotView.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NewGameplay.Models;
using NewGameplay.Controllers;
using NewGameplay.Services;
using NewGameplay.Interfaces;


namespace NewGameplay.Views
{
    public class TileSlotView : MonoBehaviour
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        private GridViewNew gridView;
        [SerializeField] private Button tileButton;
        [SerializeField] private TextMeshProUGUI symbolText;

        public void Initialize(GridViewNew gridView, int x, int y)
        {
            this.gridView = gridView;
            this.X = x;
            this.Y = y;

            tileButton.onClick.AddListener(OnTileClicked);
        }

        private void OnTileClicked()
        {
            gridView.OnTileClicked(X, Y);
        }

        public void RefreshTile(string symbol, bool isRevealed)
        {
            symbolText.text = isRevealed ? symbol : "?";
            symbolText.color = isRevealed ? Color.white : Color.cyan;
        }
    }
}

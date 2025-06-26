using UnityEngine;
using UnityEngine.UI;
using NewGameplay.SplitGrid.Interfaces;
using NewGameplay.SplitGrid.Services;
using NewGameplay.SplitGrid.Views;
using NewGameplay.SplitGrid.Data;
using NewGameplay.SplitGrid.Controllers;
using System;
using TMPro;

namespace NewGameplay.SplitGrid.Controllers
{
    public class SplitRoundPopupManager : MonoBehaviour
    {
        [Header("Popups")]
        [SerializeField] private GameObject roundStartPopup;
        [SerializeField] private GameObject roundCompletePopup;
        [SerializeField] private GameObject extractPopup;
        [SerializeField] private GameObject gameOverPopup;

        [Header("Round Start UI")]
        [SerializeField] private TextMeshProUGUI roundStartTitle;
        [SerializeField] private Button roundStartConfirmButton;

        [Header("Round Complete UI")]
        [SerializeField] private TextMeshProUGUI roundCompleteTitle;
        [SerializeField] private Button roundCompleteConfirmButton;

        [Header("Extract UI")]
        [SerializeField] private Button extractConfirmButton;

        [Header("Game Over UI")]
        [SerializeField] private TextMeshProUGUI gameOverTitle;
        [SerializeField] private Button gameOverConfirmButton;

        public event Action OnRoundStartConfirmed;
        public event Action OnRoundCompleteConfirmed;

        private void Awake()
        {
            HideAllPopups();
            SetupButtonListeners();
        }

        private void SetupButtonListeners()
        {
            if (roundStartConfirmButton != null)
                roundStartConfirmButton.onClick.AddListener(() => OnRoundStartConfirmed?.Invoke());

            if (roundCompleteConfirmButton != null)
                roundCompleteConfirmButton.onClick.AddListener(() => OnRoundCompleteConfirmed?.Invoke());

            if (extractConfirmButton != null)
                extractConfirmButton.onClick.AddListener(() => HideAllPopups());

            if (gameOverConfirmButton != null)
                gameOverConfirmButton.onClick.AddListener(() => HideAllPopups());
        }

        public void ShowRoundStartPopup(int roundNumber, Action onConfirm)
        {
            if (roundStartPopup != null)
            {
                roundStartPopup.SetActive(true);
                if (roundStartTitle != null)
                    roundStartTitle.text = $"Round {roundNumber}";
            }
        }

        public void ShowRoundCompletePopup(int roundNumber, Action onConfirm)
        {
            if (roundCompletePopup != null)
            {
                roundCompletePopup.SetActive(true);
                if (roundCompleteTitle != null)
                    roundCompleteTitle.text = $"Round {roundNumber} Complete!";
            }
        }

        public void ShowExtractPopup(Action onConfirm)
        {
            if (extractPopup != null)
                extractPopup.SetActive(true);
        }

        public void ShowGameOverPopup(bool isVictory, Action onConfirm)
        {
            if (gameOverPopup != null)
            {
                gameOverPopup.SetActive(true);
                if (gameOverTitle != null)
                    gameOverTitle.text = isVictory ? "Victory!" : "Game Over";
            }
        }

        public void HideAllPopups()
        {
            if (roundStartPopup != null) roundStartPopup.SetActive(false);
            if (roundCompletePopup != null) roundCompletePopup.SetActive(false);
            if (extractPopup != null) extractPopup.SetActive(false);
            if (gameOverPopup != null) gameOverPopup.SetActive(false);
        }

        public bool IsAnyPopupVisible()
        {
            return (roundStartPopup != null && roundStartPopup.activeSelf) ||
                   (roundCompletePopup != null && roundCompletePopup.activeSelf) ||
                   (extractPopup != null && extractPopup.activeSelf) ||
                   (gameOverPopup != null && gameOverPopup.activeSelf);
        }

        public void UpdateRoundInfo(int roundNumber, int totalRounds)
        {
            if (roundStartTitle != null)
                roundStartTitle.text = $"Round {roundNumber} of {totalRounds}";
        }

        public void UpdateProgressInfo(GridID gridId, int revealedFragments, int requiredFragments)
        {
            // Update progress UI if needed
        }
    }
} 
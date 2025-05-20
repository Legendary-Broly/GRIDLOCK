using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using NewGameplay.Enums;
using NewGameplay.Controllers;
using NewGameplay.Models;
using NewGameplay.Services;
using NewGameplay.ScriptableObjects;
using System.Linq;

namespace NewGameplay.Controllers
{
    public class RoundPopupManager : MonoBehaviour
    {
        [SerializeField] private GameObject popupPanel;

        [Header("Buttons")]
        [SerializeField] private Button integrityButton;
        [SerializeField] private Button tileElementButton;
        [SerializeField] private Button payloadButton;

        [Header("Tile Element UI")]
        [SerializeField] private TextMeshProUGUI tileElementNameText;
        [SerializeField] private TextMeshProUGUI tileElementDescText;

        [Header("Payload UI")]
        [SerializeField] private TextMeshProUGUI payloadNameText;
        [SerializeField] private TextMeshProUGUI payloadDescText;

        private TileElementType selectedTileElement;
        private PayloadType selectedPayload;
        private Action onIntegritySelected;
        private Action<TileElementType> onTileElementSelected;
        private Action<PayloadType> onPayloadSelected;
        private PayloadManager payloadManager;
        private TileElementService tileElementService;
        private readonly HashSet<TileElementType> chosenTileElements = new();
        private readonly HashSet<PayloadType> chosenPayloads = new();


        private void Awake()
        {
            popupPanel.SetActive(false);

            integrityButton.onClick.AddListener(() =>
            {
                popupPanel.SetActive(false);
                onIntegritySelected?.Invoke();
            });

            tileElementButton.onClick.AddListener(() =>
            {
                popupPanel.SetActive(false);
                onTileElementSelected?.Invoke(selectedTileElement);
            });

            payloadButton.onClick.AddListener(() =>
            {
                popupPanel.SetActive(false);
                onPayloadSelected?.Invoke(selectedPayload);
            });
        }

        public void ShowOptions(
            Action onIntegritySelected,
            Action<TileElementType> onTileElementSelected,
            Action<PayloadType> onPayloadSelected,
            TileElementType selectedTileElement)
        {
            this.onIntegritySelected = onIntegritySelected;
            this.onTileElementSelected = onTileElementSelected;
            this.onPayloadSelected = onPayloadSelected;

            popupPanel.SetActive(true);

            // Integrity button setup (assumes wired in inspector)
            integrityButton.onClick.RemoveAllListeners();
            integrityButton.onClick.AddListener(() =>
            {
                popupPanel.SetActive(false);
                this.onIntegritySelected?.Invoke();
            });

            // Tile Element reward UI
            var elementSO = tileElementService.GetElementSO(selectedTileElement);
            if (elementSO != null)
            {
                tileElementNameText.text = elementSO.displayName;
                tileElementDescText.text = elementSO.description;
            }
            else
            {
                tileElementNameText.text = selectedTileElement.ToString();
                tileElementDescText.text = $"Adds {selectedTileElement} to future rounds.";
            }

            tileElementButton.onClick.RemoveAllListeners();
            tileElementButton.onClick.AddListener(() =>
            {
                popupPanel.SetActive(false);
                this.onTileElementSelected?.Invoke(selectedTileElement);
            });

            // Payload reward UI (choose randomly for now)
            var randomPayload = GetRandomPayload();
            var payload = payloadManager.GetPayloadDetails(randomPayload);

            payloadNameText.text = payload.Name;
            payloadDescText.text = payload.Description;

            payloadButton.onClick.RemoveAllListeners();
            payloadButton.onClick.AddListener(() =>
            {
                popupPanel.SetActive(false);
                chosenPayloads.Add(randomPayload);
                this.onPayloadSelected?.Invoke(randomPayload);
            });
        }

        private TileElementType GetRandomNonDefaultElement()
        {
            TileElementType[] options = new[]
            {
                TileElementType.Warp,
                TileElementType.FlagPop,
                TileElementType.JunkPile,
                TileElementType.CodeShardPlus,
                TileElementType.SystemIntegrityIncreasePlus
            };

            return options[UnityEngine.Random.Range(0, options.Length)];
        }
        public void SetTileElementService(TileElementService service)
        {
            tileElementService = service;
        }

        private PayloadType GetRandomPayload()
        {
            var availablePayloads = Enum.GetValues(typeof(PayloadType))
                .Cast<PayloadType>()
                .Where(p => !chosenPayloads.Contains(p))
                .ToList();

            if (availablePayloads.Count == 0)
            {
                // If all payloads have been chosen, reset the chosen payloads
                chosenPayloads.Clear();
                availablePayloads = Enum.GetValues(typeof(PayloadType))
                    .Cast<PayloadType>()
                    .ToList();
            }

            var selectedPayload = availablePayloads[UnityEngine.Random.Range(0, availablePayloads.Count)];
            chosenPayloads.Add(selectedPayload);
            return selectedPayload;
        }

        public void SetPayloadManager(PayloadManager manager) => payloadManager = manager;
    }
}
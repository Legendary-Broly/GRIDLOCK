using UnityEngine;
using System.Collections.Generic;
using NewGameplay.ScriptableObjects;
using TMPro;
using UnityEngine.UI;

namespace NewGameplay.Controllers
{
    public class TutorialManualController : MonoBehaviour
    {
        [SerializeField] private Transform contentRoot;
        [SerializeField] private GameObject sectionPrefab;
        [SerializeField] private List<ManualSectionSO> manualSections;

        private void Start()
        {
            foreach (var section in manualSections)
            {
                var entry = Instantiate(sectionPrefab, contentRoot);
                var texts = entry.GetComponentsInChildren<TextMeshProUGUI>();
                texts[0].text = section.title;
                texts[1].text = section.body;

                var image = entry.GetComponentInChildren<Image>(true);
                if (section.optionalImage != null)
                {
                    image.sprite = section.optionalImage;
                    image.gameObject.SetActive(true);
                }
                else
                {
                    image.gameObject.SetActive(false);
                }
            }
        }
    }
}

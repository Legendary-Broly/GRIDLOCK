using UnityEngine;
using UnityEngine.EventSystems;

namespace NewGameplay.Utility
{
    public class CommanderToggleController : MonoBehaviour
    {
        [SerializeField] private GameObject commanderApp;
        [SerializeField] private CanvasGroup gameplayUIGroup;

        void Update()
        {
            // If UI is NOT currently holding focus, allow toggle
            if (Input.GetKeyDown(KeyCode.Tab) && !EventSystem.current.alreadySelecting)
            {
                bool isActive = commanderApp.activeSelf;
                commanderApp.SetActive(!isActive);

                if (gameplayUIGroup != null)
                {
                    gameplayUIGroup.interactable = isActive;
                    gameplayUIGroup.blocksRaycasts = isActive;
                }
            }
        }
    }
}

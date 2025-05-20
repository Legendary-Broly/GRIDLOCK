using UnityEngine;
using UnityEngine.UI;

namespace NewGameplay.Controllers
{
    public class StartupPopupController : MonoBehaviour
    {
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            Time.timeScale = 0;
            popupRoot.SetActive(true);
            closeButton.onClick.AddListener(HidePopup);
        }

        private void HidePopup()
        {
            popupRoot.SetActive(false);
            Time.timeScale = 1;
        }
    }
}

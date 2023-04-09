using UnityEngine;

namespace Assets.Scripts.PlayerInput
{
    public class PlayerInputBehaviour : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
                OnEscapePressed();
        }

        public void OnEscapePressed()
        {
            if (DarkestDungeonManager.Instanse.mainMenu.gameObject.activeSelf)
                DarkestDungeonManager.Instanse.mainMenu.WindowClosed();
            else
                DarkestDungeonManager.Instanse.mainMenu.OpenMenu();
        }

    }
}

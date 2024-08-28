namespace AF.Tutorial
{
    using UnityEngine;

    public class TutorialSection : MonoBehaviour
    {
        public UIDocumentPlayerHUDV2 uIDocumentPlayerHUDV2;

        public UIDocumentPlayerHUDV2.ControlKey controlKeyToHighlight = UIDocumentPlayerHUDV2.ControlKey.None;

        public PlayerManager playerManager;

        public TutorialSpawnRef tutorialSpawnRef;

        private void Awake()
        {
            if (tutorialSpawnRef == null)
            {
                Debug.LogError("Tutorial spawn ref not assigned!");
            }
        }

        public void Activate()
        {
            playerManager.playerComponentManager.TeleportPlayer(tutorialSpawnRef.transform);

            if (controlKeyToHighlight != UIDocumentPlayerHUDV2.ControlKey.None)
            {
                uIDocumentPlayerHUDV2.HighlightKey(controlKeyToHighlight);
            }
            else
            {
                uIDocumentPlayerHUDV2.DisableHighlights();
            }
        }
    }
}

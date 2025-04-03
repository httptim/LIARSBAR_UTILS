using MelonLoader;
using UnityEngine;
using System.Collections.Generic;
using Il2Cpp;
using Il2CppInterop.Runtime;
using System.Linq;

[assembly: MelonInfo(typeof(LIARSBAR_UTILS.Core), "LIARSBAR_UTILS", "1.0.0", "thultz", null)]
[assembly: MelonGame("Curve Animation", "Liar's Bar")]

namespace LIARSBAR_UTILS
{
    public class Core : MelonMod
    {
        // Module instances
        private CardTracker cardTracker;
        private UIManager uiManager;
        private PlayerController playerController;

        // Toggle key for position control
        private KeyCode positionControlToggleKey = KeyCode.Semicolon;

        public override void OnApplicationStart()
        {
            MelonLogger.Msg("LIARSBAR_UTILS mod has started!");

            // Initialize modules
            cardTracker = new CardTracker();
            uiManager = new UIManager();
            playerController = new PlayerController();
        }

        public override void OnUpdate()
        {
            // Update player object reference in all modules
            GameObject playerObject = FindLocalPlayerObject();

            // Update card tracker
            cardTracker.Update(playerObject);

            // Update player controller and check for toggle
            if (Input.GetKeyDown(positionControlToggleKey))
            {
                playerController.TogglePositionControl();
                MelonLogger.Msg($"Position control is now {(playerController.IsPositionControlEnabled ? "enabled" : "disabled")}");
            }

            // Update player controller if there is a valid player object
            if (playerObject != null)
            {
                playerController.Update(playerObject);
            }

            // Check for card type change hotkeys
            CheckCardTypeHotkeys(playerObject);

            // Check for clear played cards hotkey
            if (Input.GetKeyDown(KeyCode.F5))
            {
                cardTracker.ClearPlayedCards();
            }
        }

        private GameObject FindLocalPlayerObject()
        {
            foreach (var playerStats in GameObject.FindObjectsOfType<PlayerStats>())
            {
                if (playerStats != null && playerStats.isOwned)
                {
                    return playerStats.gameObject;
                }
            }
            return null;
        }

        private void CheckCardTypeHotkeys(GameObject playerObject)
        {
            if (playerObject == null) return;

            if (Input.GetKeyDown(KeyCode.F1)) cardTracker.ChangePlayerCards(playerObject, 1);
            if (Input.GetKeyDown(KeyCode.F2)) cardTracker.ChangePlayerCards(playerObject, 2);
            if (Input.GetKeyDown(KeyCode.F3)) cardTracker.ChangePlayerCards(playerObject, 3);
            if (Input.GetKeyDown(KeyCode.F4)) cardTracker.ChangePlayerCards(playerObject, 4);
        }

        public override void OnGUI()
        {
            // Draw UI with current card tracker data
            uiManager.DrawGUI(cardTracker.GetPlayerInfo());
        }

        public override void OnApplicationQuit()
        {
            MelonLogger.Msg("LIARSBAR_UTILS mod is quitting!");
        }
    }
}
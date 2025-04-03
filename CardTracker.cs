using UnityEngine;
using System.Collections.Generic;
using Il2Cpp;
using Il2CppInterop.Runtime;
using System.Linq;

namespace LIARSBAR_UTILS
{
    public class PlayerInfo
    {
        public string PlayerName;
        public string CardInfo;
        public int NetworkSlot;
    }

    public class CardTracker
    {
        private List<PlayerInfo> sortedPlayerInfo = new List<PlayerInfo>();
        private Dictionary<string, Dictionary<GameObject, bool>> playerCardLastStatus = new Dictionary<string, Dictionary<GameObject, bool>>();
        private Dictionary<string, Dictionary<GameObject, int>> inactiveCardTimestamps = new Dictionary<string, Dictionary<GameObject, int>>();
        private int timestampCounter = 0;
        private Dictionary<string, List<List<GameObject>>> playedCardGroups = new Dictionary<string, List<List<GameObject>>>();
        private Dictionary<string, List<GameObject>> newlyInactiveCards = new Dictionary<string, List<GameObject>>();
        private Dictionary<string, List<GameObject>> cardsThatBecameActive = new Dictionary<string, List<GameObject>>();

        private HashSet<string> ignoredComponents = new HashSet<string>
        {
            "Transform", "PlayerStats", "NetworkIdentity", "NetworkTransformReliable",
            "AudioSource", "FaceAnimator", "NetworkAnimator", "VoiceChat", "SkinSetter"
        };

        public void Update(GameObject playerObject)
        {
            sortedPlayerInfo.Clear();

            foreach (var key in newlyInactiveCards.Keys.ToList())
            {
                newlyInactiveCards[key].Clear();
            }

            foreach (var key in cardsThatBecameActive.Keys.ToList())
            {
                cardsThatBecameActive[key].Clear();
            }

            HashSet<string> currentPlayers = new HashSet<string>();
            timestampCounter++;

            foreach (var playerStats in GameObject.FindObjectsOfType<PlayerStats>())
            {
                if (playerStats == null) continue;

                GameObject player = playerStats.gameObject;
                if (player == null) continue;

                string playerName = playerStats.isOwned ? (playerStats.PlayerName + " (You)") : playerStats.PlayerName;
                int networkSlot = playerStats.NetworkSlot;

                currentPlayers.Add(playerName);

                InitializePlayerDataStructures(playerName);
                string cardInfo = TrackPlayerCards(player, playerName);

                PlayerInfo info = new PlayerInfo
                {
                    PlayerName = playerName,
                    CardInfo = cardInfo,
                    NetworkSlot = networkSlot
                };

                sortedPlayerInfo.Add(info);
            }

            // Sort by NetworkSlot (0-3)
            sortedPlayerInfo = sortedPlayerInfo.OrderBy(p => p.NetworkSlot).ToList();

            CleanupRemovedPlayers(currentPlayers);
        }

        private void InitializePlayerDataStructures(string playerName)
        {
            if (!playerCardLastStatus.ContainsKey(playerName))
            {
                playerCardLastStatus[playerName] = new Dictionary<GameObject, bool>();
            }

            if (!inactiveCardTimestamps.ContainsKey(playerName))
            {
                inactiveCardTimestamps[playerName] = new Dictionary<GameObject, int>();
            }

            if (!playedCardGroups.ContainsKey(playerName))
            {
                playedCardGroups[playerName] = new List<List<GameObject>>();
            }

            if (!newlyInactiveCards.ContainsKey(playerName))
            {
                newlyInactiveCards[playerName] = new List<GameObject>();
            }

            if (!cardsThatBecameActive.ContainsKey(playerName))
            {
                cardsThatBecameActive[playerName] = new List<GameObject>();
            }
        }

        private string TrackPlayerCards(GameObject player, string playerName)
        {
            string cardInfo = "No Cards";

            foreach (var component in player.GetComponents<Component>())
            {
                if (component == null) continue;
                string componentName = component.GetIl2CppType().Name;

                if (ignoredComponents.Contains(componentName)) continue;

                var cardsField = component.GetIl2CppType().GetField("Cards");
                if (cardsField != null)
                {
                    var cardsObject = cardsField.GetValue(component);
                    if (cardsObject != null)
                    {
                        var cardsList = cardsObject.TryCast<Il2CppSystem.Collections.Generic.List<UnityEngine.GameObject>>();
                        if (cardsList != null)
                        {
                            cardInfo = ProcessPlayerCards(cardsList, playerName);
                        }
                    }
                    break;
                }
            }

            return cardInfo;
        }

        private string ProcessPlayerCards(Il2CppSystem.Collections.Generic.List<UnityEngine.GameObject> cardsList, string playerName)
        {
            List<string> activeCards = new List<string>();
            Dictionary<GameObject, string> inactiveCardsDict = new Dictionary<GameObject, string>();

            foreach (var cardObject in cardsList)
            {
                if (cardObject != null)
                {
                    var cardComponent = cardObject.GetComponent<Il2Cpp.Card>();
                    if (cardComponent != null)
                    {
                        TrackCardStatusChange(cardObject, cardComponent, playerName);

                        var cardType = cardComponent.cardtype;
                        string cardName = GetCardTypeName(cardType);
                        bool isActive = cardComponent.isActiveAndEnabled;

                        if (isActive)
                        {
                            activeCards.Add(cardName);
                        }
                        else
                        {
                            inactiveCardsDict[cardObject] = cardName;

                            if (!inactiveCardTimestamps[playerName].ContainsKey(cardObject))
                            {
                                inactiveCardTimestamps[playerName][cardObject] = 0;
                            }
                        }
                    }
                }
            }

            ProcessCardActivationChanges(playerName);

            if (newlyInactiveCards[playerName].Count > 0)
            {
                playedCardGroups[playerName].Add(new List<GameObject>(newlyInactiveCards[playerName]));
            }

            return FormatCardInfo(activeCards, playerName);
        }

        private void TrackCardStatusChange(GameObject cardObject, Card cardComponent, string playerName)
        {
            bool isActive = cardComponent.isActiveAndEnabled;

            if (playerCardLastStatus[playerName].ContainsKey(cardObject))
            {
                bool wasActive = playerCardLastStatus[playerName][cardObject];

                if (wasActive && !isActive)
                {
                    inactiveCardTimestamps[playerName][cardObject] = timestampCounter;
                    newlyInactiveCards[playerName].Add(cardObject);
                }
                else if (!wasActive && isActive)
                {
                    cardsThatBecameActive[playerName].Add(cardObject);
                }
            }

            playerCardLastStatus[playerName][cardObject] = isActive;
        }

        private void ProcessCardActivationChanges(string playerName)
        {
            if (cardsThatBecameActive[playerName].Count > 0)
            {
                foreach (var card in cardsThatBecameActive[playerName])
                {
                    foreach (var group in playedCardGroups[playerName].ToList())
                    {
                        if (group.Contains(card))
                        {
                            group.Remove(card);
                            if (group.Count == 0)
                            {
                                playedCardGroups[playerName].Remove(group);
                            }
                        }
                    }

                    if (inactiveCardTimestamps[playerName].ContainsKey(card))
                    {
                        inactiveCardTimestamps[playerName].Remove(card);
                    }
                }
            }
        }

        private string FormatCardInfo(List<string> activeCards, string playerName)
        {
            string activeCardsStr = string.Join(", ", activeCards);

            List<KeyValuePair<List<GameObject>, int>> groupsWithTimestamps = new List<KeyValuePair<List<GameObject>, int>>();
            foreach (var group in playedCardGroups[playerName])
            {
                int mostRecentTimestamp = 0;
                bool hasValidCards = false;

                foreach (var card in group)
                {
                    if (card != null && inactiveCardTimestamps[playerName].ContainsKey(card))
                    {
                        int cardTimestamp = inactiveCardTimestamps[playerName][card];
                        mostRecentTimestamp = Mathf.Max(mostRecentTimestamp, cardTimestamp);
                        hasValidCards = true;
                    }
                }

                if (hasValidCards)
                {
                    groupsWithTimestamps.Add(new KeyValuePair<List<GameObject>, int>(group, mostRecentTimestamp));
                }
            }

            groupsWithTimestamps.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

            List<string> formattedGroups = new List<string>();
            foreach (var pair in groupsWithTimestamps)
            {
                var group = pair.Key;
                List<string> cardNames = new List<string>();
                foreach (var card in group)
                {
                    if (card != null)
                    {
                        var cardComponent = card.GetComponent<Il2Cpp.Card>();
                        if (cardComponent != null)
                        {
                            cardNames.Add(GetCardTypeName(cardComponent.cardtype));
                        }
                    }
                }

                if (cardNames.Count > 0)
                {
                    formattedGroups.Add("[" + string.Join(", ", cardNames) + "]");
                }
            }

            string inactiveCardsStr = string.Join(", ", formattedGroups);

            string cardInfo = activeCardsStr;
            if (!string.IsNullOrEmpty(inactiveCardsStr))
            {
                cardInfo += " | " + inactiveCardsStr;
            }

            return cardInfo;
        }

        private void CleanupRemovedPlayers(HashSet<string> currentPlayers)
        {
            List<string> playersToRemove = new List<string>();
            foreach (var key in playerCardLastStatus.Keys)
            {
                if (!currentPlayers.Contains(key))
                {
                    playersToRemove.Add(key);
                }
            }

            foreach (var player in playersToRemove)
            {
                playerCardLastStatus.Remove(player);
                inactiveCardTimestamps.Remove(player);
                playedCardGroups.Remove(player);
                newlyInactiveCards.Remove(player);
                cardsThatBecameActive.Remove(player);
            }
        }

        public void ChangePlayerCards(GameObject playerObject, int cardType)
        {
            if (playerObject == null) return;

            var playerStats = playerObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                foreach (var component in playerObject.GetComponents<Component>())
                {
                    if (component == null) continue;

                    var cardsField = component.GetIl2CppType().GetField("Cards");
                    if (cardsField != null)
                    {
                        var cardsObject = cardsField.GetValue(component);
                        if (cardsObject != null)
                        {
                            var cardsList = cardsObject.TryCast<Il2CppSystem.Collections.Generic.List<UnityEngine.GameObject>>();
                            if (cardsList != null)
                            {
                                foreach (var cardObject in cardsList)
                                {
                                    if (cardObject != null)
                                    {
                                        var cardComponent = cardObject.GetComponent<Il2Cpp.Card>();
                                        if (cardComponent != null)
                                        {
                                            cardComponent.cardtype = cardType;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ClearPlayedCards()
        {
            foreach (var playerInfo in sortedPlayerInfo)
            {
                string playerName = playerInfo.PlayerName;
                if (playerName.Contains("(You)") && playedCardGroups.ContainsKey(playerName))
                {
                    playedCardGroups[playerName].Clear();
                }
            }
        }

        public string GetCardTypeName(int cardType)
        {
            switch (cardType)
            {
                case 1: return "King";
                case 2: return "Queen";
                case 3: return "Ace";
                case 4: return "Joker";
                default: return "Special";
            }
        }

        public List<PlayerInfo> GetPlayerInfo()
        {
            return sortedPlayerInfo;
        }
    }
}
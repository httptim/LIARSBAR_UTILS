using UnityEngine;
using System.Collections.Generic;

namespace LIARSBAR_UTILS
{
    public class UIManager
    {
        // Simple GUI positioning
        private Rect windowRect = new Rect(20, 20, 400, 300);
        private Vector2 scrollPosition = Vector2.zero;
        private bool isDragging = false;
        private Vector2 dragOffset = Vector2.zero;

        public void DrawGUI(List<PlayerInfo> playerInfo)
        {
            // Update window height based on number of players
            windowRect.height = 25 + (playerInfo.Count * 60) + 10;

            // Draw background
            GUI.Box(windowRect, "");

            // Draw title bar
            GUI.Box(new Rect(windowRect.x, windowRect.y, windowRect.width, 20), "Liar's Bar Card Tracker");

            // Handle dragging
            HandleWindowDragging();

            // Content area
            Rect contentRect = new Rect(windowRect.x, windowRect.y + 20, windowRect.width, windowRect.height - 20);
            Rect viewRect = new Rect(0, 0, contentRect.width - 20, playerInfo.Count * 60);

            // Start scrollable area
            scrollPosition = GUI.BeginScrollView(contentRect, scrollPosition, viewRect);

            float yPos = 5;

            for (int i = 0; i < playerInfo.Count; i++)
            {
                PlayerInfo player = playerInfo[i];

                // Player name without slot number
                GUI.Label(new Rect(10, yPos, 380, 20), player.PlayerName);
                yPos += 20;

                // Cards information
                GUI.Label(new Rect(15, yPos, 370, 30), player.CardInfo);

                yPos += 35;
            }

            GUI.EndScrollView();
        }

        private void HandleWindowDragging()
        {
            Rect dragRect = new Rect(windowRect.x, windowRect.y, windowRect.width, 20);
            if (Event.current.type == EventType.MouseDown && dragRect.Contains(Event.current.mousePosition))
            {
                isDragging = true;
                dragOffset = Event.current.mousePosition - new Vector2(windowRect.x, windowRect.y);
                Event.current.Use();
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                isDragging = false;
            }

            if (isDragging && Event.current.type == EventType.MouseDrag)
            {
                windowRect.x = Event.current.mousePosition.x - dragOffset.x;
                windowRect.y = Event.current.mousePosition.y - dragOffset.y;
                Event.current.Use();
            }
        }
    }
}
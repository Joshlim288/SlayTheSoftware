using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Map
{
    /// <summary>
    /// MODIFIED 
    /// Handles the tracking of player position on the level map
    /// 
    /// Reused Component originally created by Vladimir Limarchenko (Silverua) on Github
    /// Source Code: https://github.com/silverua/slay-the-spire-map-in-unity
    /// 
    /// Author has allowed free reuse and modification as stated in the License
    /// This file has largely remained as is, with any modifications demarcated with MODIFICATION comments
    /// </summary>
    public class MapPlayerTracker : MonoBehaviour
    {
        public bool lockAfterSelecting = false;
        public float enterNodeDelay = 1f;
        public MapManager mapManager;
        public MapView view;

        public static MapPlayerTracker Instance;

        public bool Locked { get; set; }

        private void Awake()
        {
            Instance = this;
        }

        public void SelectNode(MapNode mapNode)
        {
            if (Locked) return;

            if (mapManager.CurrentMap.path.Count == 0)
            {
                // player has not selected the node yet, he can select any of the nodes with y = 0
                if (mapNode.Node.point.y == 0)
                    SendPlayerToNode(mapNode);
                else
                    PlayWarningThatNodeCannotBeAccessed();
            }
            else
            {
                var currentPoint = mapManager.CurrentMap.path[mapManager.CurrentMap.path.Count - 1];
                var currentNode = mapManager.CurrentMap.GetNode(currentPoint);

                if (currentNode != null && currentNode.outgoing.Any(point => point.Equals(mapNode.Node.point)))
				{
                    SendPlayerToNode(mapNode);
				}
                else
                    PlayWarningThatNodeCannotBeAccessed();
            }
        }

        private void SendPlayerToNode(MapNode mapNode)
        {
            Locked = lockAfterSelecting;
            mapManager.CurrentMap.path.Add(mapNode.Node.point);
            view.SetAttainableNodes();
            view.SetLineColors();
            mapNode.ShowSwirlAnimation();
            DOTween.Sequence().AppendInterval(enterNodeDelay).OnComplete(() => EnterNode(mapNode));
        }

        private static void EnterNode(MapNode mapNode)
        {
            // MODIFICATION load appropriate scene with context based on nodeType:
            switch (mapNode.Node.nodeType)
            {
                case NodeType.MinorEnemy:
                    SceneManager.LoadScene("QuizMode");
                    break;
                case NodeType.EliteEnemy:
                    SceneManager.LoadScene("QuizMode");
                    break;
                case NodeType.Boss:
                    SceneManager.LoadScene ("QuizBossMode2");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            // MODIFICATION END 
        }

        private void PlayWarningThatNodeCannotBeAccessed()
        {
            Debug.Log("Selected node cannot be accessed");
        }
    }
}
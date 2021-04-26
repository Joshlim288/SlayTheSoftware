using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Map
{
    /// <summary>
    /// MODIFIED 
    /// Handles LevelMap logic, and loading of maps
    /// 
    /// Reused Component originally created by Vladimir Limarchenko (Silverua) on Github
    /// Source Code: https://github.com/silverua/slay-the-spire-map-in-unity
    /// Author has allowed free reuse and modification as stated in the License
    /// 
    /// This file has been modified and refactored to better integrate into the system
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        public MapConfig config;
        public MapView view;
        public TextManager textObject;

        public Map CurrentMap { get; private set; }

        /// <summary>
        /// called when the scene is loaded
        /// Checks what map has been selected and loads the map
        /// </summary>
        private void Start()
        {
            int mapChoice;
            if (GameData.bgMap < 3) 
            {
                // Change map background based on selected world
                mapChoice = GameData.bgMap;
            }
            else 
            {
                // Challenge and Assignment world use the same background
                mapChoice = 3;
            }
            // Filepath for maps in editor
            string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets\\Maps\\Map"+mapChoice.ToString()+".txt");
            if (!File.Exists(FilePath))
            {
                // Filepath for maps in build
                FilePath = Path.Combine(Directory.GetCurrentDirectory(), "Slay The Software - Final_Data\\Resources\\Maps\\Map"+mapChoice.ToString()+".txt");
            }
            // check whichever filepath exists
            if (File.Exists(FilePath))
            {
                var mapJson = System.IO.File.ReadAllText(FilePath);
                var map = JsonConvert.DeserializeObject<Map>(mapJson);
                CurrentMap = map;
                view.ShowMap(map);
            }
            else
            {
                Debug.Log("Filepath not found");
            }
        }
    }
}

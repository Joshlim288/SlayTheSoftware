using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Map
{
    public class MapManager : MonoBehaviour
    {
        public MapConfig config;
        public MapView view;

        public Map CurrentMap { get; private set; }

        private void Start()
        {
            string mapChoice = CurrentUser.getChosenMap().ToString();
            string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/Maps/Map"+mapChoice+".txt");
            if (File.Exists(FilePath))
            {
                //var mapJson = PlayerPrefs.GetString("Map");
                var mapJson = System.IO.File.ReadAllText(FilePath);
                var map = JsonConvert.DeserializeObject<Map>(mapJson);
                // using this instead of .Contains()
                if (map.path.Any(p => p.Equals(map.GetBossNode().point)))
                {
                    // payer has already reached the boss, generate a new map
                    GenerateNewMap();
                }
                else
                {
                    CurrentMap = map;
                    // player has not reached the boss yet, load the current map
                    view.ShowMap(map);
                }
            }
            else
            {
                GenerateNewMap();
            }
        }

        public void GenerateNewMap()
        {
            var map = MapGenerator.GetMap(config);
            CurrentMap = map;
            Debug.Log(map.ToJson());
            view.ShowMap(map);
        }

        public async Task SaveMap()
        {
            if (CurrentMap == null) return;

            var json = JsonConvert.SerializeObject(CurrentMap);
            //PlayerPrefs.SetString("Map", json);
            //PlayerPrefs.Save();
            // Set a variable to the Documents path.
            string docPath = Directory.GetCurrentDirectory();

            // Write the specified text asynchronously to a new file
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "Assets/Maps/Map.txt")))
            {
                await outputFile.WriteAsync(json);
            }
        }

        private void OnApplicationQuit()
        {
            //SaveMap();
        }
    }
}

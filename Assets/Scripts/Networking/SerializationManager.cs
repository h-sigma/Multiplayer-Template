using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Networking
{
    public static class SerializationManager
    {
        private static BinaryFormatter GetBinaryFormatter()
        {
            var formatter = new BinaryFormatter();
            
            SurrogateSelector selector = new SurrogateSelector();
            
            formatter.SurrogateSelector = selector;
            
            return formatter;
        }
        
        public static bool Save(string path, string filename, object saveData)
        {
            var formatter = GetBinaryFormatter();

            var p = Path.Combine(Application.persistentDataPath, path);

            if (!Directory.Exists(p))
            {
                Directory.CreateDirectory(p);
            }

            p = Path.Combine(p, filename);

            using (var fs = new FileStream(p, FileMode.Create))
            {
                formatter.Serialize(fs, saveData);
                return true;
            }

#pragma warning disable 162
            return false;
#pragma warning restore 162
        }

        public static object Load(string path, string filename) 
        {
            var finalPath = Path.Combine(Application.persistentDataPath, path, filename);
            if (!File.Exists(finalPath))
            {
                return null;
            }
            else
            {
                try
                {
                    using (var fs = new FileStream(finalPath, FileMode.Open))
                    {
                        var formatter  = GetBinaryFormatter();
                        var loadedData = formatter.Deserialize(fs);
                        return loadedData;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception caught when loading data at path \"" + finalPath + "\".  Exception:" +
                                   e.Message);
                }
            }
#if UNITY_EDITOR
            Debug.Log("No save data exists at Path \"" + finalPath + "\".");
#endif
            return null;
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StringToGameObjectMap : MonoBehaviour
{
   [System.Serializable]
   public class StringToGameObjectMapEntry
   {
       public string key;
       public GameObject value;
   }

   public StringToGameObjectMapEntry[] gameObjects;

   private Dictionary<string, GameObject> map;
   public Dictionary<string, GameObject> Map
   {
       get {
           if (map == null) { setup(); }
           return map; 
       }
   }

   private void setup()
   {
       map = new Dictionary<string, GameObject>(gameObjects.Length);
       foreach (var entry in gameObjects)
       {
           map[entry.key] = entry.value;
       }
   }
}
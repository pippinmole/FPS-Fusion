using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 1, fileName = "New Map List", menuName = "Maps/New Map Collection")]
public class MapList : ScriptableObject {

    [SerializeField] private List<int> _maps = new();

    public List<int> GetAllMaps() => _maps;
    public int GetMap(int index) => _maps[index];
}

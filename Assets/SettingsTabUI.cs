using System.Collections.Generic;
using UnityEngine;
using Zenvin.Settings.Framework;

public class SettingsTabUI : MonoBehaviour {
    
    public class TabData {
        public int Index { get; }
        public SettingsGroup Group { get; }
        public RectTransform ContentParent { get; }

        public TabData(int index, SettingsGroup group, RectTransform contentParent) {
            Index = index;
            Group = group;
            ContentParent = contentParent;
        }
    }

    private static readonly List<TabData> Tabs = new();

    [SerializeField] private RectTransform _topParent;
    [SerializeField] private SettingsTab _topPrefab; 

    [SerializeField] private Transform _middleParent;
    [SerializeField] private RectTransform _middlePrefab;

    public RectTransform AddTab(SettingsGroup group) {
        if ( group == null )
            return null;

        var isFirst = _middleParent.childCount == 0;
        var top = Instantiate(_topPrefab, _topParent);
        
        top.Setup(this, group, Tabs.Count);

        var middle = Instantiate(_middlePrefab, _middleParent);
        middle.gameObject.SetActive(isFirst);
        
        Tabs.Add(new TabData(Tabs.Count, group, middle));

        return middle;
    }

    public void EnablePanel(int index) {
        for ( var i = 0; i < Tabs.Count; i++ ) {
            var t = Tabs[i];
            t.ContentParent.gameObject.SetActive(index == i);
        }
    }
}
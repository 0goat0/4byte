using System;
using UnityEngine;

[Serializable]
public class UnitData
{
    [SerializeField] private string _name;
    public string Name => _name;

    public UnitData(string name)
    {
        _name = name;
    }
}

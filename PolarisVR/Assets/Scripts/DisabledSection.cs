using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DisabledSection
{
    public Vector3Int startCell; // Starting cell coordinates of the disabled section
    public Vector3Int size; // Size of the disabled section in cells
}

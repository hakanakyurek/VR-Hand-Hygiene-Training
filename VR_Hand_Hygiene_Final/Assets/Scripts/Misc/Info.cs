using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName ="Info",menuName ="InfoFile")]
public class Info : ScriptableObject
{
    [SerializeField,TextArea(3,20)] private string Description="Here comes the info/description/notes";
}

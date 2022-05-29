using UnityEngine;
using System.Collections;

public class LevelScript : MonoBehaviour
{
    public int experience;

    [SerializeField]
    public GameObject particlePrefab;

    public int Level
    {
        get { return experience / 750; }
    }

    void Start() { }
}

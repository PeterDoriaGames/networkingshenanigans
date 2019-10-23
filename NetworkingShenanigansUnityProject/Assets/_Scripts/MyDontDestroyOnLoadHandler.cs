using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyDontDestroyOnLoadHandler : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _DontDestroyGameObjects; 
    private void Awake()
    {
        for (int i = 0; i < _DontDestroyGameObjects.Count; i++)
        {
            DontDestroyOnLoad(_DontDestroyGameObjects[i]);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject MonsterReference;

    // Start is called before the first frame update
    void Start()
    {
        /*we are going to create a ton on monsters, give them random locations
         * then read the static MonsterCount int to observe the results of a static variable*/
        
        for (int i = 0; i < 10; i++)
        {
            //this method or instantiation creates the same result
            //therefore the MonsterCount at the end of Start is simply behind
            Camera.main.AddComponent<MonsterBase>();

            Debug.Log("new Monster created");
        }
        //testing static function call, still returns inaccurate 
        Debug.Log("Count of Monsters & Enemy Status: " + MonsterBase.GetMCWithEnemyStatus());
    }

    // Update is called once per frame
    void Update()
    {
        //calling the function works in both cases, works here for accurate count
        Debug.Log("Count of Monsters & Enemy Status: " + MonsterBase.GetMCWithEnemyStatus());

    }
}

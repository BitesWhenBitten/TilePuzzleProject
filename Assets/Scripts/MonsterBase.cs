using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBase : MonoBehaviour
{
    public static int MonsterCount;

    public static bool IsEnemy = true;

    // Start is called before the first frame update
    void Start()
    {
        //everybody gets start called on first active frame of the object
        Debug.Log("hit start");
        MonsterCount++;
      
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static (int MC, bool IsEnemy) GetMCWithEnemyStatus()
    {
        return (MonsterCount, IsEnemy);
    }
}

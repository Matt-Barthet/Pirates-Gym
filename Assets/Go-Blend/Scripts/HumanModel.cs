using System.Collections;
using System.Collections.Generic;
using Go_Blend.Scripts;
using UnityEngine;

public class HumanModel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StateSaveLoad.Init();
        StartCoroutine(Test());
    }
    
    private IEnumerator Test()
    {
        yield return new WaitForEndOfFrame();
        StateSaveLoad.SaveEnvironment("Seed");
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

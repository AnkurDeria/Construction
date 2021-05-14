using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewScript2 : MonoBehaviour
{
    [SerializeField] GameObject cube;
    [SerializeField] string name;
    private float random;
    public int random2;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            random = UnityEngine.Random.Range(0f, 100f);
            random2 = UnityEngine.Random.Range(0, 10);
            Instantiate(cube, new Vector3(random, random2, 0), Quaternion.identity);
        }
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }
}

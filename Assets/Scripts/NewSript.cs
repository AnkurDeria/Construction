using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField] NewScript2 newScript2;
    private void Awake()
    {
        Debug.Log("Awake is called");
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start is called");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("Pressed A");
        }
    }
}

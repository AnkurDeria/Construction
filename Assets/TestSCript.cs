using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSCript : MonoBehaviour
{
    [SerializeField] NewScript2 newScript2;
    [SerializeField] GameObject instanceGameobject;
    // Start is called before the first frame update
    void Start()
    {
    }
    private void OnCollisionEnter(Collision collision)
    {
        gameObject.SetActive(false);
        Instantiate(instanceGameobject, new Vector3(5, 5, 5), Quaternion.identity);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

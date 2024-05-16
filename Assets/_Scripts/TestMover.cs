using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMover : MonoBehaviour
{
    private Rigidbody myBody;
    [SerializeField] private float speed = 1;
    // Start is called before the first frame update
    void Start()
    {
        myBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if(myBody.velocity.magnitude < speed)
        {
            float val = Input.GetAxis("Vertical");
            if (val != 0)
                myBody.AddForce(0, 0, val * Time.fixedDeltaTime * 1000f);
        }
    }

}

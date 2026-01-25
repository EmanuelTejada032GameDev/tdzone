using UnityEngine;

public class TestScript : MonoBehaviour
{
    public Rigidbody rb;

    void Start()
    {
      rb = GetComponent<Rigidbody>();

    }

    void Update()
    {

        if(Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("adding force on space");
            rb.AddForce(Vector3.up * 100f);
        }
    }
}

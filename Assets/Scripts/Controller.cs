using UnityEngine;

public class Controller : MonoBehaviour
{
    Rigidbody rb;
    Camera viewCamera;
    Vector3 input;
    public float moveSpeed = 6;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        viewCamera = Camera.main;
    }
    void Update()
    {
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        Vector3 mousePos = viewCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, viewCamera.transform.position.y));
        transform.LookAt(mousePos + Vector3.up * transform.position.y);
    }
    void FixedUpdate()
    {
        rb.MovePosition(transform.position + input * Time.deltaTime * moveSpeed);
    }
}
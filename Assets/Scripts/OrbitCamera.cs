using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    public Transform Target
    {
        get => Target;
        set => target = value;
    }

    private readonly float rotationSpeed = 1.5f;
    private float rotationY;
    private Vector3 offset;
    private readonly float rotationSpeedMouseFactor = 3;
    [SerializeField] private Transform target;

    public void InitializePosition()
    {
        rotationY = transform.eulerAngles.y;
        offset = target.position - transform.position;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        var horizontalInput = Input.GetAxis("Horizontal");
        if (horizontalInput != 0f)
            rotationY += horizontalInput * rotationSpeed;
        else
            rotationY += Input.GetAxis("Mouse X") * rotationSpeed * rotationSpeedMouseFactor;
        
        var rotation = Quaternion.Euler(0, rotationY, 0);
        transform.position = target.position - (rotation * offset);
        transform.LookAt(target);
    }
}

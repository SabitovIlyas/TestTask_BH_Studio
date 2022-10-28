using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    public Transform Target { set => target = value; }
    public Logger Logger { set => logger = value; }

    private Logger logger = NullLogger.Create();
    private readonly float rotationSpeed = 1.5f;
    private float rotationY;
    private float rotationX;
    private Vector3 offset;
    private readonly float rotationSpeedMouseFactor = 3;
    [SerializeField] private Transform target;

    public void InitializePosition()
    {
        rotationY = transform.eulerAngles.y;
        rotationX = transform.eulerAngles.x;

        offset = target.position - transform.position;
        logger.Log("Привязали камеру к игроку " + target.name);
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
        
        rotationX += -Input.GetAxis("Mouse Y") * rotationSpeed * rotationSpeedMouseFactor;
        
        var rotation = Quaternion.Euler(rotationX, rotationY, 0);
        transform.position = target.position - (rotation * offset);
        transform.LookAt(target);
    }
}

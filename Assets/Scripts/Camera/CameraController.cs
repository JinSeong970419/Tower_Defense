using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] public float cameraSensitivity = 90;
    [SerializeField] public float normalMoveSpeed = 10;
    [SerializeField] public float fastMoveFactor = 3;
    [SerializeField] public float elevationSpeed = 2;

    public Vector2 posRangeX = new Vector2(0, 0);
    public Vector2 posRangeY = new Vector2(0, 0);
    public Vector2 posRangeZ = new Vector2(0, 0);

    private float rotationX;
    private float rotationY;
    private float rotationZ;

    // 카메라 고정 회전
    private float minAngleX = -70F;
    private float maxAngleX = 90F;
    private float minAngleY = -360.0F;
    private float maxAngleY = 360.0F;

    public Rigidbody rb;
    bool middleMousePressed = false;
    float movementCooldownTimer = 0;


    void Start() {
        // 카메라 회전 초기화
        rotationX = transform.rotation.eulerAngles.x;
        rotationY = transform.rotation.eulerAngles.y;
        rotationZ = transform.rotation.eulerAngles.z;
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    /**
     * Event Listeners
     */
    void MouseEventListener()
    {
        // 마우스 가운데 버튼 
        middleMousePressed = buttonToggle(KeyCode.Mouse2, middleMousePressed);
        //middleMousePressed = buttonToggle(KeyCode.G, middleMousePressed);
    }

    // 토글 방식
    private bool buttonToggle(KeyCode key, bool current)
    {
        if (Input.GetKeyDown(key))
            return true;
        if (Input.GetKeyUp(key))
            return false;
        return current;
    }

    void Update() 
    {
        bool resetDir = false;

        Vector3 currentPos = transform.position;
        if (posRangeX != null && posRangeX.x != posRangeX.y) { 
            currentPos.x = Mathf.Clamp(currentPos.x, posRangeX.x, posRangeX.y);
        }
        if (posRangeY != null && posRangeY.x != posRangeY.y) {
            currentPos.y = Mathf.Clamp(currentPos.y, posRangeY.x, posRangeY.y);
        }
        if (posRangeZ != null && posRangeZ.x != posRangeZ.y) {
            currentPos.z = Mathf.Clamp(currentPos.z, posRangeZ.x, posRangeZ.y);
        }
        transform.position = currentPos;

        movementCooldownTimer += Time.deltaTime;

        MouseEventListener();
       
        bool isMovementIntended = false;

        // 마우스 가운데 버튼 처리
        if (middleMousePressed)
        {
            // 회전할때 마우스 숨기기
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // 키 입력
            rotationY += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
            rotationX -= Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;

            // Clamp 회전
            rotationY = Mathf.Clamp(SanitizeAngle(rotationY), minAngleY, maxAngleY);
            rotationX = Mathf.Clamp(SanitizeAngle(rotationX), minAngleX, maxAngleX);

            // 회전 적용
            transform.localRotation = Quaternion.Euler(rotationX, rotationY, rotationZ);
            resetDir = true;
        }
        else
        {
            // 회전이 아닐경우 마우스 잠김 해제
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        forward.y = 0;

        float speed = normalMoveSpeed;
        
        // shift 속도 빠르게
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            speed *= fastMoveFactor;
        }

        KeyCode k;
        Vector3 dir;
        Vector3 summedDir = new Vector3(0, 0, 0);
        float forceScaler = 100F;

        if (Mathf.Abs(Input.mouseScrollDelta.y) > 0)
        {
            rb.AddForce(Vector3.up * -Input.mouseScrollDelta.y * elevationSpeed * forceScaler);
            isMovementIntended = true;
        }

        // Speed
        k = KeyCode.LeftShift;
        if (Input.GetKeyDown(k))
            resetDir = true;
        if (Input.GetKeyUp(k))
            resetDir = true;

        // Forward
        k = KeyCode.W;
        dir = forward;
        if (Input.GetKey(k))
        {
            summedDir += dir;
            isMovementIntended = true;
        }
        if (Input.GetKeyDown(k))
            resetDir = true;
        if (Input.GetKeyUp(k))
            resetDir = true;

        // Backward
        k = KeyCode.S;
        dir = -forward;
        if (Input.GetKey(k)) {
            summedDir += dir;
            isMovementIntended = true;
        }
        if (Input.GetKeyDown(k))
            resetDir = true;
        if (Input.GetKeyUp(k))
            resetDir = true;

        // Right
        k = KeyCode.D;
        dir = right;
        if (Input.GetKey(k)) {
            summedDir += dir;
            isMovementIntended = true;
        }
        if (Input.GetKeyDown(k))
            resetDir = true;
        if (Input.GetKeyUp(k))
            resetDir = true;

        // Left
        k = KeyCode.A;
        dir = -right;
        if (Input.GetKey(k))
        {
            summedDir += dir;
            isMovementIntended = true;
        }
        if (Input.GetKeyDown(k))
            resetDir = true;
        if (Input.GetKeyUp(k))
            resetDir = true;


        // 재설정
        if (resetDir)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(summedDir.normalized * speed * forceScaler);
        }
        if (isMovementIntended) { movementCooldownTimer = 0; }
        if (movementCooldownTimer > 0.1F)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public static float SanitizeAngle(float angle)
    {
        angle = angle % 360;

        if (angle < -360F)
            angle += 360F;

        if (angle > 360F)
            angle -= 360F;

        return angle;
    }
}

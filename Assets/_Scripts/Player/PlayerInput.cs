using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Vector2 PlayerDirection;
    public Vector2 LastMoveDirection = Vector2.right;
    public KeyCode UpKey = KeyCode.W;
    public KeyCode DownKey = KeyCode.S;
    public KeyCode LeftKey = KeyCode.A;
    public KeyCode RightKey = KeyCode.D;
    // Update is called once per frame
    void Update()
    {
        float x = 0f;
        float y = 0f;

        if (Input.GetKey(RightKey)) x += 1f;
        if (Input.GetKey(LeftKey))  x -= 1f;
        if (Input.GetKey(UpKey))    y += 1f;
        if (Input.GetKey(DownKey))  y -= 1f;

        PlayerDirection = new Vector2(x, y).normalized;

        if (PlayerDirection.sqrMagnitude > 0.01f)
        {
            LastMoveDirection = PlayerDirection;
        }
    }
    /// <summary>
    /// Hàm này cho phép người chơi tự custome các phím điều khiển di chuyển của nhân vật. Người chơi có thể truyền vào các phím mong muốn cho các hướng di chuyển (lên, xuống, trái, phải) và hàm sẽ cập nhật các phím này cho nhân vật.
    /// </summary>
    /// <param name="upKey"></param>
    /// <param name="downKey"></param>
    /// <param name="leftKey"></param>
    /// <param name="rightKey"></param>
    /// <returns></returns>
    public KeyCode CustomeInput(KeyCode upKey, KeyCode downKey, KeyCode leftKey, KeyCode rightKey)
    {
        UpKey = upKey;
        DownKey = downKey;
        LeftKey = leftKey;
        RightKey = rightKey;
        return UpKey;
    }
}

using UnityEngine;

[System.Serializable]
public struct FrameInput
{
    public float Timestamp;
    public Vector2 Move;
    public bool JumpPressed;
    public bool InteractPressed;
}

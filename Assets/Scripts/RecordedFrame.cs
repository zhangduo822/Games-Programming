using UnityEngine;

[System.Serializable]
public struct RecordedFrame
{
    public float Time;
    public Vector3 Position;
    public Vector2 Velocity;
    public float MoveX;
    public bool JumpPressed;
}

using UnityEngine;

public class DualPlateDoor2D : MonoBehaviour
{
    [SerializeField] private PressurePlate2D leftPlate;
    [SerializeField] private PressurePlate2D rightPlate;
    [SerializeField] private Door2D door;

    public void Configure(PressurePlate2D left, PressurePlate2D right, Door2D targetDoor)
    {
        leftPlate = left;
        rightPlate = right;
        door = targetDoor;
    }

    private void Update()
    {
        if (leftPlate == null || rightPlate == null || door == null)
        {
            return;
        }

        if (leftPlate.IsPressed && rightPlate.IsPressed)
        {
            door.Open();
        }
        else
        {
            door.Close();
        }
    }
}

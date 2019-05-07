using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private Animation animation;
    private bool isActive = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (!isActive)
            {
                animation.Play("ShowInv");
                isActive = true;
            }
            else
            {
                animation.Play("CloseInv");
                isActive = false;
            }
        }
    }
}

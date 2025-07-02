using System;
using UnityEngine;
using UnityEngine.UI;

public class PortalCrosshair : MonoBehaviour
{
    [SerializeField] private Image blueFill;
    [SerializeField] private Image orangeFill;

    private void Start()
    {
        ResetCrosshair();
    }

    public void ToggleBlueFill(bool toggle)
    {
        blueFill.enabled = toggle;
    }

    public void ToggleOrangeFill(bool toggle)
    {
        orangeFill.enabled = toggle;
    }

    public void ResetCrosshair()
    {
        blueFill.enabled = false;
        orangeFill.enabled = false;
    }
}

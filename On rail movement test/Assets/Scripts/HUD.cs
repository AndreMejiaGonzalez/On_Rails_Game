using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public RectTransform hoverbar;
    public RectTransform shotBar;
    public RectTransform crosshair;
    public RectTransform crosshairOverlay;

    public PlayerObject player;

    void Start()
    {
        player = GameObject.Find("Player-Camera Setup").GetComponentInChildren<PlayerObject>();
        Cursor.visible = false;
    }

    void Update()
    {
        hoverbar.localScale = new Vector3(hoverbar.localScale.x,player.hoverCharge.normalizedChargeValue,0);
        shotBar.localScale = new Vector3(player.shotCharge.normalizedChargeValue,shotBar.localScale.y,0);
        crosshair.position = Input.mousePosition;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class PlayerObject : MonoBehaviour
{
    private Transform playerModel;

    [SerializeField]
    private float xySpeed;
    [SerializeField]
    private float lookSpeed;
    [SerializeField]
    private float forwardSpeed;
    [SerializeField]
    private float leanLimit;

    public Transform aimTarget;
    public CinemachineDollyCart dollyCart;
    public Transform cameraParent;

    public ParticleSystem trail;
    public ParticleSystem circle;
    public ParticleSystem barrel;

    void Start()
    {
        playerModel = transform.GetChild(0);
        setSpeed(forwardSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);

        playerMovement(movement);
        clampPosition();
        rotationLook(new Vector3(movement.x, movement.y , 1));
        horizontalLean(playerModel, movement.x, leanLimit, .1f);

        if(Input.GetKeyDown("k"))
        {
            doBreak(true);
        }

        if(Input.GetKeyUp("k"))
        {
            doBreak(false);
        }

        if(Input.GetKeyDown("l"))
        {
            boost(true);
        }

        if(Input.GetKeyUp("l"))
        {
            boost(false);
        }

        if(Input.GetKeyDown("o") || Input.GetKeyDown("p"))
        {
            int direction = Input.GetKeyDown("o") ? 1 : -1;
            barrelRoll(direction);
        }
    }

    void playerMovement(Vector3 movement)
    {
        transform.localPosition += movement * xySpeed * Time.deltaTime;
    }

    void clampPosition()
    {
        Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
        pos.x = Mathf.Clamp01(pos.x);
        pos.y = Mathf.Clamp01(pos.y);
        transform.position = Camera.main.ViewportToWorldPoint(pos);
    }

    void rotationLook(Vector3 movement)
    {
        aimTarget.parent.position = Vector3.zero;
        aimTarget.localPosition = movement;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(aimTarget.position), Mathf.Deg2Rad * lookSpeed * Time.deltaTime);
    }

    void horizontalLean(Transform target, float axis, float leanLimit, float lerpTime)
    {
        Vector3 targetEulerAngles = target.localEulerAngles;
        target.localEulerAngles = new Vector3(targetEulerAngles.x, targetEulerAngles.y, Mathf.LerpAngle(targetEulerAngles.z, -axis * leanLimit, lerpTime));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(aimTarget.position, .5f);
        Gizmos.DrawSphere(aimTarget.position, .15f);
    }

    public void barrelRoll(int direction)
    {
        if(!DOTween.IsTweening(playerModel))
        {
            playerModel.DOLocalRotate(new Vector3(playerModel.localEulerAngles.x, playerModel.localEulerAngles.y, 360 * direction), .4f, RotateMode.LocalAxisAdd).SetEase(Ease.OutSine);
            barrel.Play();
        }
    }

    void setSpeed(float speed)
    {
        dollyCart.m_Speed = speed;
    }

    void setCameraZoom(float zoom, float duration)
    {
        cameraParent.DOLocalMove(new Vector3(0, 0, zoom), duration);
    }

    void boost(bool state)
    {
        if(state)
        {
            cameraParent.GetComponentInChildren<CinemachineImpulseSource>().GenerateImpulse();
            trail.Play();
            circle.Play();
        }

        float speed = state ? forwardSpeed * 2 : forwardSpeed;
        float zoom = state ? -7 : 0;

        DOVirtual.Float(dollyCart.m_Speed, speed, .15f, setSpeed);
        setCameraZoom(zoom, .4f);
    }

    void doBreak(bool state)
    {
        float speed = state ? forwardSpeed / 3 : forwardSpeed;
        float zoom = state ? 3 : 0;

        DOVirtual.Float(dollyCart.m_Speed, speed, .15f, setSpeed);
        setCameraZoom(zoom, .4f);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class PlayerObject : MonoBehaviour
{
    public struct Charge
    {
        public float min;
        public float max;

        private float _chargeValue;
        public float chargeValue
        {
            get
            {
                return this._chargeValue;
            }

            set
            {
                _chargeValue = value >= min && value <= max ? value : _chargeValue;
            }
        }

        public float normalizedChargeValue
        {
            get
            {
                return this.chargeValue / max;
            }
        }

        public void chargeUp(float gain)
        {
            this.chargeValue = chargeValue + gain > max ? max : this.chargeValue + gain;
        }

        public void chargeDown(float loss)
        {
            this.chargeValue = chargeValue - loss < min ? min : this.chargeValue - loss;
        }

        public void deplete()
        {
            this.chargeValue = min;
        }
    }

    private Transform playerModel;

    [SerializeField]
    private float xySpeed;
    [SerializeField]
    private float lookSpeed;
    [SerializeField]
    private float forwardSpeed;
    [SerializeField]
    private float leanLimit;

    private Charge shotCharge;

    public Transform aimTarget;
    public CinemachineDollyCart dollyCart;
    public Transform cameraParent;

    public ParticleSystem trail;
    public ParticleSystem circle;
    public ParticleSystem barrel;

    void Start()
    {
        shotCharge.max = 1;
        shotCharge.min = 0;
        playerModel = transform.GetChild(0);
        setSpeed(forwardSpeed);
    }

    void Update()
    {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);

        playerMovement(movement);
        clampPosition();
        rotationLook(new Vector3(movement.x, movement.y , 1));
        horizontalLean(playerModel, movement.x, leanLimit, .1f);

        shotCharge.chargeUp(Time.deltaTime);

        if(Input.GetButtonDown("Fire1"))
        {
            doBreak(true);
        }

        if(Input.GetButtonUp("Fire1"))
        {
            doBreak(false);
        }

        if(Input.GetButtonDown("Fire2"))
        {
            boost(true);
        }

        if(Input.GetButtonUp("Fire2"))
        {
            boost(false);
        }

        if(Input.GetButtonDown("BumperLeft") || Input.GetButtonDown("BumperRight") || Input.GetKeyDown(KeyCode.O))
        {
            int direction = Input.GetButtonDown("BumperLeft") ? 1 : -1;
            barrelRoll(direction);
        }

        if(Input.GetMouseButtonDown(1))
        {
        }

        if(Input.GetMouseButtonDown(0))
        {
            if(shotCharge.chargeValue == shotCharge.max)
            {
                shotCharge.deplete();
                Debug.Log("fire");
            }
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

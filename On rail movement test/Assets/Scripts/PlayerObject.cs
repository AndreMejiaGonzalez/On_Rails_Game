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

    [SerializeField]
    private float xySpeed;
    [SerializeField]
    private float forwardSpeed;
    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float leanLimit;

    private bool jumpFlag
    {
        get
        {
            return Input.GetKeyDown(KeyCode.W);
        }
    }
    private bool jumpHeldFlag
    {
        get
        {
            return Input.GetKey(KeyCode.W);
        }
    }

    private Rigidbody rb;

    private Charge shotCharge;

    public Transform aimTarget;
    public CinemachineDollyCart dollyCart;
    public Transform cameraParent;

    public ParticleSystem circle;

    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        shotCharge.max = 1;
        shotCharge.min = 0;
        setSpeed(forwardSpeed);
    }

    void FixedUpdate()
    {

    }

    void Update()
    {
        JumpFunc();
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, 0);

        playerMovement(movement);
        clampPosition();
        horizontalLean(this.transform, movement.x, leanLimit, .1f);

        shotCharge.chargeUp(Time.deltaTime);

        if(Input.GetKeyDown(KeyCode.Space))
        {
            doBreak(true);
        }

        if(Input.GetKeyUp(KeyCode.Space))
        {
            doBreak(false);
        }

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            boost(true);
        }

        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            boost(false);
        }

        if(Input.GetMouseButtonDown(1))
        {
            Debug.Log("meelee goes here");
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
        this.transform.localPosition += movement * xySpeed * Time.deltaTime;
    }

    void JumpFunc()
    {
        if(jumpFlag)
        {
            rb.velocity += Vector3.up * jumpForce * Time.deltaTime;
        }
    }

    void clampPosition()
    {
        Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
        pos.x = Mathf.Clamp01(pos.x);
        pos.y = Mathf.Clamp01(pos.y);
        transform.position = Camera.main.ViewportToWorldPoint(pos);
    }

    void horizontalLean(Transform target, float axis, float leanLimit, float lerpTime)
    {
        Vector3 targetEulerAngles = target.localEulerAngles;
        target.localEulerAngles = new Vector3(targetEulerAngles.x, targetEulerAngles.y, Mathf.LerpAngle(targetEulerAngles.z, -axis * leanLimit, lerpTime));
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

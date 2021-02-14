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
    [SerializeField]
    private float fallMultiplier;
    [SerializeField]
    private float lowJumpMultiplier;
    private bool isJumping;
    private bool jumpHeldFlag;
    
    private Ray ray;
    private RaycastHit hit;
    private bool didHit;

    private Rigidbody rb;

    public Charge shotCharge;
    public Charge hoverCharge;

    public Transform aimTarget;
    public CinemachineDollyCart dollyCart;
    public Transform cameraParent;

    public HUD hud;

    public ParticleSystem circle;

    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        shotCharge.max = 1;
        shotCharge.min = 0;
        hoverCharge.max = 3;
        hoverCharge.min = 0;
        shotCharge.chargeValue = shotCharge.max;
        hoverCharge.chargeValue = hoverCharge.max;
        setSpeed(forwardSpeed);
    }

    void FixedUpdate()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        didHit = Physics.Raycast(ray, out hit);
        if(didHit)
        {
            hud.crosshairOverlay.gameObject.SetActive(true);
        } else {
            hud.crosshairOverlay.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, 0);

        playerMovement(movement);
        clampPosition();
        horizontalLean(this.transform, movement.x, leanLimit, .1f);
        jumpCurve();

        shotCharge.chargeUp(Time.deltaTime);
        if(!isJumping)
        {
            hoverCharge.chargeUp(Time.deltaTime);
        }

        if(Input.GetKeyDown(KeyCode.W))
        {
            if(!isJumping)
            {
                isJumping = true;
                jumpHeldFlag = true;
                rb.AddForce(new Vector3(0,jumpForce,0), ForceMode.Impulse);
            }
        }

        if(Input.GetKeyUp(KeyCode.W))
        {
            jumpHeldFlag = false;
        }

        if(Input.GetKey(KeyCode.W))
        {
            if(!jumpHeldFlag && hoverCharge.chargeValue > hoverCharge.min)
            {
                rb.velocity = new Vector3(rb.velocity.x,0,rb.velocity.z);
                hoverCharge.chargeDown(Time.deltaTime);
            }
        }

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
        }

        if(Input.GetMouseButtonDown(0))
        {
            if(shotCharge.chargeValue == shotCharge.max)
            {
                laserEffect();
                shotCharge.deplete();
            }
        }
    }

    void playerMovement(Vector3 movement)
    {
        this.transform.localPosition += movement * xySpeed * Time.deltaTime;
    }

    void jumpCurve()
    {
        if(rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        } else if(rb.velocity.y > 0 && !jumpHeldFlag)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    void laserEffect()
    {
        Vector3[] pos;
        GameObject laser = Resources.Load<GameObject>("Prefabs/Laser");
        if(didHit)
        {
            pos = new Vector3[] {this.transform.position, hit.point};
        } else {
            pos = new Vector3[] {this.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0,0,500))};
        }
        LineRenderer lr = laser.GetComponent<LineRenderer>();
        lr.SetPositions(pos);
        Instantiate(laser);
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            isJumping = false;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorController : MonoBehaviour
{

    public GameObject model; //抓取游戏对象，对象为playr
    public PlayerInput pi;   //抓取当前对象的playerinput组件并设置为pi
    public float walkSpeed = 1.4f;
    public float runMultiplier = 2.7f;
    public float jumpVelocity = 5.0f;
    public float rollVelocity = 3.0f;

    [Space(10)]
    [Header("===== Friction Settings =====")]
    public PhysicMaterial frictionOne;
    public PhysicMaterial frictionZero;

    private Animator anim;
    private Rigidbody rb;
    private Vector3 planarVec;
    private Vector3 thrustVec;// 冲量向量
    private bool canAttack;
    private bool lockPlanar = false;//是否锁死平面移动
    private CapsuleCollider coll;
    private float lerpTarget;
    private Vector3 deltaPos;


    void Awake()
    {
        pi = GetComponent<PlayerInput>();  //加载当前对象的playerinput脚本
        anim = model.GetComponent<Animator>(); //抓取当前对象的animator组件
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        //print(pi);
        float targetRunMulti = ((pi.run) ? 2.0f : 1.0f);
        anim.SetFloat("forward",pi.Dmag * Mathf.Lerp (anim.GetFloat("forward"),targetRunMulti,0.5f));//设置移动树的forward的值
        anim.SetBool("defense", pi.defense);

        if (rb.velocity.magnitude > 1.0f)//根据下落速度进行播放roll动画
        {
            anim.SetTrigger("roll");
        }
        if (pi.jump)
        {
            anim.SetTrigger("jump");
            canAttack = false;
        }
        if (pi.attack && CheckState("ground")&& canAttack)
        {
            anim.SetTrigger("attack");
        }
        if (pi.Dmag > 0.1f)
        {
            model.transform.forward = Vector3.Slerp(model.transform.forward, pi.Dvec, 0.3f); //当移动输入的值小于0.1f是禁止旋转 ++ //线性插值球形线性法
        }
        if (lockPlanar == false)
        {
            planarVec = pi.Dmag * model.transform.forward * walkSpeed * ((pi.run) ? runMultiplier : 1.0f); //移动
        }
    }

    private void FixedUpdate()
    {
        rb.position += deltaPos;
        rb.velocity = new Vector3(planarVec.x, rb.velocity.y, planarVec.z) + thrustVec ;
        thrustVec = Vector3.zero;
        deltaPos = Vector3.zero;
    }

    private bool CheckState(string stateName,string layerName = "Base Layer")
    {
        return anim.GetCurrentAnimatorStateInfo(anim.GetLayerIndex(layerName)).IsName(stateName);
    }

     /////
     ///// Message processing block
     /////
    public void OnJumpEnter()
    {
        pi.inputEnbled = false;
        lockPlanar = true;
        thrustVec = new Vector3(0, jumpVelocity, 0);//跳跃力
    }
    public void IsGround()
    {
        anim.SetBool("isGround",true);
    }

    public void IsNotGround()
    {
        anim.SetBool("isGround", false);
    }

    public void OnGroundEnter() //进入地面
    {
        pi.inputEnbled = true;
        lockPlanar = false;
        canAttack = true;
        coll.material = frictionOne;
    }
    public void OnFallEnter()
    {
        print("Is fall");
        pi.inputEnbled = false;
        lockPlanar =true;
    }
    public void OnGroundExit()
    {
        coll.enabled = frictionZero;
    }
    public void OnRollEnter()
    {
        thrustVec = new Vector3(0, rollVelocity, 0);
        pi.inputEnbled = false;
        lockPlanar = true;
    }
    public void OnJabEnter()
    {
        pi.inputEnbled = false;
        lockPlanar = true;
    }
    public void OnJabUpdate()
    {
        thrustVec = model.transform.forward * anim.GetFloat("jabVelocity");
    }
    public void OnAttack1hAEnter()  //Weight Switching
    {
        pi.inputEnbled = false;
        lerpTarget = 1.0f;
    }
    public void OnAttack1hUpdate()
    {
        thrustVec = model.transform.forward * anim.GetFloat("attack1hAVelocity");
        anim.SetLayerWeight(anim.GetLayerIndex("attack"), Mathf.Lerp(anim.GetLayerWeight(anim.GetLayerIndex("attack")), lerpTarget, 0.4f));
    }
    public void OnAttackIdleEnter()  //Weight Switching
    {
        pi.inputEnbled = true;
        //lockPlanar = false;
        //anim.SetLayerWeight(anim.GetLayerIndex("attack"), 0);
        lerpTarget = 0f;
    }
    public void OnAttackIdleUpdate()
    {
        anim.SetLayerWeight(anim.GetLayerIndex("attack"), Mathf.Lerp(anim.GetLayerWeight(anim.GetLayerIndex("attack")), lerpTarget, 0.4f));
    }

    public void OnUpdateRM(object _deltaPos)
    {
        if (CheckState("attack1hC","attack"))
        {
            deltaPos += (Vector3)_deltaPos;
        }
    }
}
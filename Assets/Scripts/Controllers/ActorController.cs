using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorController : MonoBehaviour
{
    // 游戏对象，对象为palyer
    public GameObject model;
    // 当前对象的 PlayerInput 组件
    public PlayerInput pi;
    public float walkSpeed = 1.4f;          // 行走速度
    public float runMultiplier = 2.7f;      // 跑步速度倍率
    public float jumpVelocity = 5.0f;       // 跳跃速度
    public float rollVelocity = 3.0f;       // 翻滚速度

    [Space(10)]
    [Header("===== Friction Settings =====")]
    public PhysicMaterial frictionOne;
    public PhysicMaterial frictionZero;

    private Animator anim;                  // 动画组件控制器
    private Rigidbody rb;                   // 刚体组件
    private Vector3 planarVec;              // 平面移动向量
    private Vector3 thrustVec;              // 冲量向量
    private bool canAttack;                 // 是否可以攻击
    private bool lockPlanar = false;        // 是否锁死平面移动
    private CapsuleCollider coll;           // 胶囊碰撞器组件
    private float lerpTarget;               // 插值目标
    private Vector3 deltaPos;               // 位置变化量


    void Awake()
    {
        pi = GetComponent<PlayerInput>();           // 获取当前对象的 PlayerInput 脚本
        anim = model.GetComponent<Animator>();      // 获取当前对象的 Animator 脚本
        rb = GetComponent<Rigidbody>();             // 获取当前对象的 Rigidbody 组件
        coll = GetComponent<CapsuleCollider>();     // 获取当前对象的 CapsuleCollider 组件
    }

    // Update is called once per frame
    void Update()
    {
        // 根据玩家输入设置奔跑速度倍率
        float targetRunMulti = ((pi.run) ? 2.0f : 1.0f);
        // 设置移动动画前进值
        anim.SetFloat("forward",pi.Dmag * Mathf.Lerp (anim.GetFloat("forward"),targetRunMulti,0.5f));//设置移动树的forward的值
        // 设置防御动画的状态
        anim.SetBool("defense", pi.defense);

        // 根据下落速度进行播放roll动画
        if (rb.velocity.magnitude > 1.0f)
        {
            anim.SetTrigger("roll");
        }

        // 如果玩家按下跳跃按钮，播放跳跃动画并停止攻击
        if (pi.jump)
        {
            anim.SetTrigger("jump");
            canAttack = false;
        }

        // 如果玩家按下攻击按钮且当前状态为ground且可以攻击，播放攻击动画
        if (pi.attack && CheckState("ground")&& canAttack)
        {
            anim.SetTrigger("attack");
        }

        // 当移动输入的值大于0.1f时，使用球形线性插值旋转模型
        if (pi.Dmag > 0.1f)
        {
            model.transform.forward = Vector3.Slerp(model.transform.forward, pi.Dvec, 0.3f);
        }

        // 如果锁定平面移动为false，则计算平面移动向量
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
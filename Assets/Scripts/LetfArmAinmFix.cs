using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetfArmAinmFix : MonoBehaviour
{
    private Animator anim;

    public Vector3 a;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    //// Start is called before the first frame update
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}
    private void OnAnimatorIK()
    {
        if (anim.GetBool("defense") == false)
        {
        Transform leftLowerArm = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        leftLowerArm.localEulerAngles += 0.75f * a;
        anim.SetBoneLocalRotation(HumanBodyBones.LeftLowerArm,Quaternion.Euler(leftLowerArm.localEulerAngles));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script : MonoBehaviour {


    

    [System.Serializable]
    public class FaceAnimation
    {
        public string name;
        public float value;
        public int layer;
        public float length;

        public void Update(Animator animator)
        {
            float animationValue = value;
            animationValue = Mathf.Clamp01(value);
            animationValue *= length;

            animator.SetFloat("faceAnimationSpeed", 1);

            animator.PlayInFixedTime(name, layer, animationValue);
            animator.Update(0f);

            animator.SetFloat("faceAnimationSpeed", 0);
        }
    }

    public FaceAnimation[] anim = new FaceAnimation[0];
    public Animator animator;

    public Transform[] attach;

    void Start ()
    {
        SkinnedMeshRenderer meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < attach.Length; i++)
        {
            SkinnedMeshRenderer attachRenderer = attach[i].GetComponentInChildren<SkinnedMeshRenderer>();
            attachRenderer.bones = meshRenderer.bones;
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        for (int i = 0; i < anim.Length; i++)
        {
            anim[i].Update(animator);
        }
    }
}

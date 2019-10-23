using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Photon.Realtime;
using Photon.Pun;

public class MyPlayerAnimatorManager : MonoBehaviourPun
{
    #region PRIVATE FIELDS 

    private Animator _Animator;
    [SerializeField]
    private float _MoveSpeed = 10f, _TurnScalar = 1, _DirectionDampTime = .25f;

    private float _horiz, _vert;
    private bool _ShouldJump;

    #endregion


    #region MONOBEHAVIOUR CALLBACKS

    private void Start()
    {
        _Animator = GetComponent<Animator>();
        if (!_Animator)
        {
            Debug.LogError("PlayerAnimatorManager is missing animator component. ", this);
        }
    }

    private void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

        if (!_Animator)
        {
            Debug.LogError("PlayerAnimatorManager is missing animator component. ", this);
            return;
        }

        _horiz = Input.GetAxis("Horizontal") * _MoveSpeed;
        _vert = Input.GetAxis("Vertical") * _MoveSpeed;
        if (_vert < 0)
        {
            _vert = 0;
        }

        // deal with Jumping
        AnimatorStateInfo stateInfo = _Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Base Layer.Run"))
        {
            if (Input.GetButtonDown("Jump"))
            {
                _ShouldJump = true;
            }
        }
    }

    private void FixedUpdate()
    {
        _Animator.SetFloat("Speed", (_horiz * _horiz + _vert * _vert) * _MoveSpeed);
        _Animator.SetFloat("Direction", _horiz, _DirectionDampTime, Time.deltaTime);
        if (_ShouldJump)
        {
            _Animator.SetTrigger("Jump");
            _ShouldJump = false;
        }
    }

    #endregion
}

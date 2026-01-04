using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public GameObject[] FPS_Hands_ChildGameobjects;
    public GameObject[] Soldier_ChildGameobjects;

    public GameObject playerUIPrefab;
    private PlayerMovementController playerMovementController;

    public Camera FPSCamera;

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        playerMovementController = GetComponent<PlayerMovementController>();

        if(photonView.IsMine)
        {
            //activate FPS hands, deactivate soldier
            foreach (GameObject gameObject in FPS_Hands_ChildGameobjects)
            {
                gameObject.SetActive(true);
            }
            foreach (GameObject gameObject in Soldier_ChildGameobjects)
            {
                gameObject.SetActive(false);
            }

            //Instantiate PlayerUI
            GameObject playerUIGameobject = Instantiate(playerUIPrefab);
            playerMovementController.joystick = playerUIGameobject.transform.Find("Fixed Joystick").GetComponent<Joystick>();
            playerMovementController.fixedTouchField = playerUIGameobject.transform.Find("RotationTouchField").GetComponent<FixedTouchField>();

            FPSCamera.enabled = true;
            animator.SetBool("IsSoldier", false);
        }
        else
        {
            //activate soldier, deactivate FPS hands
            foreach (GameObject gameObject in FPS_Hands_ChildGameobjects)
            {
                gameObject.SetActive(false);
            }
            foreach (GameObject gameObject in Soldier_ChildGameobjects)
            {
                gameObject.SetActive(true);
            }

            playerMovementController.enabled = false;
            GetComponent<RigidbodyFirstPersonController>().enabled = false;

            FPSCamera.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

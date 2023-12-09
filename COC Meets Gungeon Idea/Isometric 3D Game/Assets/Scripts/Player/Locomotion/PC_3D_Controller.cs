using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cinemachine.Utility;
using UnityEditor.Experimental.GraphView;

namespace Main.Player_Locomotion
{
    public class PC_3D_Controller : MonoBehaviour
    {
        public Rigidbody player_RB;
        private BoxCollider player_col;

        public Transform FadeObjectTransform;
        public Transform exitTransform;
        public float moveSpeed = 5f;
        public float fadeDuration = 2f;
        public bool isMovingTowardsTownHall = false;
        public bool isMovingAwayFromTownHall = false;
        private float startTime;
        private Color originalColor;
        public Material playerMaterial;

        public float movement_Velocity;
        private float dampVelocity;
        Vector3 InputVec;
        GM gameManager;

        public float GravityStrength;
        public bool isGrounded;
        public float jumpHeight;

        public bool insideEventArea = false;

        public TextMeshPro interactiveText;

        // animations
        public Animator animator;

        // Enum to represent movement states
        public enum MovementState
        {
            Idle,
            WalkingSlow,
            WalkingQuick,
            Running
        }

        public MovementState currentMovementState = MovementState.Idle;

        public float movementInputValue = 0f; // The input value to control movement animations
        public float walkSpeed = 2f; // Adjust the speed for walking
        public float jogSpeed = 3f;
        public float runSpeed = 5f; // Adjust the speed for running

        void Start()
        {
            playerMaterial = transform.GetChild(0).transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material; // with mesh
            animator = transform.GetChild(0).GetComponent<Animator>();
            //playerMaterial = GetComponent<Renderer>().material; // before mesh
            originalColor = playerMaterial.color;

            interactiveText.enabled = false;
            gameManager = FindObjectOfType<GM>();
            Vector3 gravityS = new Vector3(0, GravityStrength, 0);
            Physics.gravity = gravityS;
            isGrounded = true;

            player_RB = gameObject.AddComponent<Rigidbody>();
            player_col = gameObject.AddComponent<BoxCollider>();
            player_RB.constraints = RigidbodyConstraints.FreezeRotation;
        }

        public void EnteringTownHall(bool isFading, Vector3 DesiredPosition)
        {
            if(isFading)
            {
                float journeyLength = Vector3.Distance(transform.position, DesiredPosition);
                float journeyTime = Time.time - startTime;
                float fractionOfJourney = journeyTime / fadeDuration;

                transform.position = Vector3.Lerp(transform.position, DesiredPosition, moveSpeed * Time.deltaTime);
                transform.LookAt(FadeObjectTransform.position);
                Color fadedColor = originalColor;
                fadedColor.a = Mathf.Lerp(1f, 0f, fractionOfJourney);
                playerMaterial.color = fadedColor;

                if (fractionOfJourney >= 1f)
                {
                    //isMovingTowardsTownHall = false;
                    movement_Velocity = 0;
                    if (CameraSwitcher.IsCameraActive(gameManager.gameplayCamera))
                    {
                        CameraSwitcher.SwitchCamera(gameManager.villageCamera);
                        gameManager.UpgradeHelper.GetComponent<UpgradeDetection_Helper>().enabled = true;
                        gameManager.currentSelectedVillagePiece = null;
                    }
                }
            }
            else
            {
                float journeyTime = Time.time - startTime;
                float fractionOfJourney = journeyTime / fadeDuration;

                transform.position = Vector3.Lerp(transform.position, DesiredPosition, moveSpeed * Time.deltaTime);
                transform.LookAt(exitTransform.position);
                Vector3 exitPosition = exitTransform.TransformPoint(Vector3.zero);
                Vector3 direction = (transform.position - DesiredPosition).normalized;



                Color fadedColor = originalColor;
                fadedColor.a = Mathf.Lerp(0f, 1f, fractionOfJourney);
                playerMaterial.color = fadedColor;

                if (fractionOfJourney >= 1f)
                {
                    movement_Velocity = 5;
                }

                if (Vector3.Distance(transform.position, DesiredPosition) < 1f)
                {
                    isMovingAwayFromTownHall = false;
                    exitTransform.gameObject.SetActive(true);
                }
            }
        }

        public float maxIndependentValue = 5.0f; // Define a maximum value for the independent value
        public float independentIncreaseSpeed = 1.0f; // Control how fast it increases
        public float independentDecreaseSpeed = 1.0f; // Control how fast it decreases
        public float independentValue;
        void Update()
        {
            // Get my run to pass through below
            bool run = animator.GetBool("Running");
            GatherInput_Motion();
            if (!isMovingTowardsTownHall)
            {
                Look();
            }
            else
            {
                transform.LookAt(FadeObjectTransform);
            }

            if (isMovingAwayFromTownHall)
                EnteringTownHall(false, exitTransform.position);

            if (Input.GetKeyDown(KeyCode.J) && isGrounded == true)
            {
                Jump();
            }

            if (insideEventArea && Input.GetKeyDown(KeyCode.E) && !isMovingTowardsTownHall)
            {
                isMovingTowardsTownHall = true;
            }

            float currentInputValue = InputVec.magnitude;

            // Gradually increase the independent value based on player input
            if (currentInputValue > 0)
            {
                independentValue = Mathf.Min(independentValue + independentIncreaseSpeed * Time.deltaTime, maxIndependentValue);
            }
            else
            {
                // Gradually decrease the independent value when there's no input
                independentValue = 0;//Mathf.Max(independentValue - independentDecreaseSpeed * Time.deltaTime, 0.0f);
            }

            // Check input and transition between states based on movementInputValue
            if (Input.GetKey(KeyCode.LeftShift) && movementInputValue > 0)
            {
                TransitionToState(MovementState.Running);
                animator.SetBool("Running", true);
                animator.SetFloat("MovementSpeed", 0);

            }
            else if (independentValue > 0)
            {
                currentMovementState = MovementState.WalkingSlow;
                animator.SetBool("Running", false);

            }
            else if(independentValue > 2)
            {
                currentMovementState = MovementState.WalkingQuick;
            }
            else
            {
                currentMovementState = MovementState.Idle;
                animator.SetBool("Running", false);

            }
            if (run)
                return;
            else
                animator.SetFloat("MovementSpeed", independentValue);
        }

        private void FixedUpdate()
        {
            SetMovementSpeed();
            if (isMovingTowardsTownHall)
            {
                EnteringTownHall(true, FadeObjectTransform.position);
            }
            else
            {
                Move();
            }
        }

        void GatherInput_Motion()
        {
            if (isGrounded)
            {
                InputVec = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                movementInputValue = InputVec.magnitude;
            }

        }

        void Look()
        {
            if (InputVec != Vector3.zero)
            {
                var relative = (transform.position + InputVec.ToIso()) - transform.position;
                var rot = Quaternion.LookRotation(relative, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, 360);
            }
        }

        void Move()
        {
            player_RB.MovePosition(transform.position + (transform.forward * InputVec.magnitude) * movement_Velocity * Time.deltaTime);

            //
        }

        float previousMovementInputValue = 0f; // Variable to store the previous value of movementInputValue

        void SetMovementSpeed()
        {
            switch (currentMovementState)
            {
                case MovementState.Idle:
                    //movement_Velocity = 0;          
                    break;

                case MovementState.WalkingSlow:
                    movement_Velocity = walkSpeed; // Adjust as needed
                    break;
                case MovementState.WalkingQuick:
                    movement_Velocity = jogSpeed; // Adjust as needed
                    break;
                case MovementState.Running:
                    movement_Velocity = runSpeed;
                    break;
            }
        }

        void TransitionToState(MovementState newState)
        {
            if (currentMovementState != newState)
            {
                currentMovementState = newState;
            }
        }


        void Jump()
        {
            player_RB.AddForce(new Vector3(0, jumpHeight, 0));
            isGrounded = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == 3 || collision.gameObject.layer == 7)
            {
                isGrounded = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == 11)
            {
                interactiveText.enabled = true;
                exitTransform = other.gameObject.transform.parent.transform.GetChild(2).transform;
                insideEventArea = true;
                FadeObjectTransform = other.gameObject.transform.parent.transform;
            }
            else
                insideEventArea = false;
        }

        private void OnTriggerExit(Collider other)
        {
            insideEventArea = false;
            interactiveText.enabled = false;
        }
    }
}

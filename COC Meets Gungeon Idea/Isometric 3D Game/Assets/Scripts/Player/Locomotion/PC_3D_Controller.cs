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

        // Melee Combat
        public bool isSwordShefed = true;

        List<string> playedAnimations = new List<string>();

        string[] allAnimationClips = new string[]
        {
            "Inward Strike 10 Degrees Normal",
            "Inward Strike 20 Degrees Normal",
            "Inward Strike 30 Degrees Normal",
            "Inward Strike 40 Degrees Normal",
            "Inward Strike 60 Degrees Normal",
            "Inward Strike 70 Degrees Normal",
            "Inward Strike 80 Degrees Normal",
            "Inward Strike 90 Degrees Normal",
            "Outward Strike 10 Degrees Normal",
            "Outward Strike 20 Degrees Normal",
            "Outward Strike 30 Degrees Normal",
            "Outward Strike 40 Degrees Normal",
            "Outward Strike 60 Degrees Normal",
            "Outward Strike 70 Degrees Normal",
            "Outward Strike 80 Degrees Normal",
            "Outward Strike 90 Degrees Normal",
            "First Strike Normal Outward"

        };
        // Dictionary to map animation names to blend tree values (floats)
        Dictionary<string, float> animationToBlendTreeValue = new Dictionary<string, float>();
        bool lastAnimationWasInward = false;
        bool Attacking = false;


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


            // Animation Mapping via floats to blend tree
            // Assign blend tree values (floats) for each animation
            // Modify this according to your blend tree values
            animationToBlendTreeValue.Add("First Slash Inward Normal", 1.0f);
            animationToBlendTreeValue.Add("Inward Strike 10 Degrees Normal", 2.0f);
            animationToBlendTreeValue.Add("Inward Strike 20 Degrees Normal", 3.0f);
            animationToBlendTreeValue.Add("Inward Strike 30 Degrees Normal", 4.0f);
            animationToBlendTreeValue.Add("Inward Strike 40 Degrees Normal", 5.0f);
            animationToBlendTreeValue.Add("Inward Strike 60 Degrees Normal", 6.0f);
            animationToBlendTreeValue.Add("Inward Strike 70 Degrees Normal", 7.0f);
            animationToBlendTreeValue.Add("Inward Strike 80 Degrees Normal", 8.0f);
            animationToBlendTreeValue.Add("Inward Strike 90 Degrees Normal", 9.0f);

            animationToBlendTreeValue.Add("Outward Strike 10 Degrees Normal", 10.0f);
            animationToBlendTreeValue.Add("Outward Strike 20 Degrees Normal", 11.0f);
            animationToBlendTreeValue.Add("Outward Strike 30 Degrees Normal", 12.0f);
            animationToBlendTreeValue.Add("Outward Strike 40 Degrees Normal", 13.0f);
            animationToBlendTreeValue.Add("Outward Strike 60 Degrees Normal", 14.0f);
            animationToBlendTreeValue.Add("Outward Strike 70 Degrees Normal", 15.0f);
            animationToBlendTreeValue.Add("Outward Strike 80 Degrees Normal", 16.0f);
            animationToBlendTreeValue.Add("Outward Strike 90 Degrees Normal", 17.0f);
            animationToBlendTreeValue.Add("First Strike Normal Outward", 18.0f);


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
            if (isSwordShefed == true && Input.GetKey(KeyCode.LeftShift) && movementInputValue > 0)
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


            #region Running in combat Checkers
            bool animSwordBool = animator.GetBool("IsSwordSheath");
            animSwordBool = isSwordShefed;
            animator.SetBool("IsSwordSheath", isSwordShefed);
            #endregion
            if (Input.GetMouseButtonDown(0))
            {
                Attacking = true;
                animator.SetBool("Attacking", Attacking);
                if(isSwordShefed)
                {
                    DrawSword();
                }
                else if(Attacking && !isSwordShefed)
                {
                    MeleeAttack();
                }
            }

            if(Input.GetMouseButtonUp(0))
            {
                Attacking = false;
                animator.SetBool("Attacking", Attacking);
            }


            if (Input.GetMouseButton(1))
                StartCoroutine(SheathSword());

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

        void DrawSword()
        {
            int drawLayer = animator.GetLayerIndex("Combat Handle");
            animator.SetLayerWeight(drawLayer, 1);
            if (isSwordShefed)
            {
                animator.Play("Dummy|Draw Weapon");
            }


            Debug.Log("Sword has been drawn");

            isSwordShefed = false;
        }

        void MeleeAttack()
        {
            string selectedAnimation = GetUniqueRandomAnimation();
            if (selectedAnimation != null)
            {
                Debug.Log("Selected Animation: " + selectedAnimation);
                playedAnimations.Add(selectedAnimation);
                lastAnimationWasInward = selectedAnimation.Contains("Inward");

                // Set the blend tree parameter based on the dictionary mapping
                if (animationToBlendTreeValue.ContainsKey(selectedAnimation))
                {
                    float animationValue = animationToBlendTreeValue[selectedAnimation];
                    animator.Play("Slash Cycle", -1, 0f);
                    animator.SetFloat("CombatDrawFloat", animationValue); // Change "BlendParameter" to your blend tree parameter name
                }
                else
                {
                    Debug.LogError("Animation value not found for: " + selectedAnimation);
                }
            }

        }


        string GetUniqueRandomAnimation()
        {
            string[] remainingClips = GetRemainingClips();

            // Check if Normal Inward animation hasn't been played yet
            if (!playedAnimations.Contains("First Slash Inward Normal") && remainingClips.Length > 0)
            {
                return "First Slash Inward Normal"; // Play "Normal Inward" first if it's available
            }

            if (remainingClips.Length > 0)
            {
                // Find the next animation based on the alternating pattern
                foreach (string clip in remainingClips)
                {
                    if (lastAnimationWasInward && clip.Contains("Outward"))
                    {
                        return clip; // Play an outward animation next
                    }
                    else if (!lastAnimationWasInward && clip.Contains("Inward"))
                    {
                        return clip; // Play an inward animation next
                    }
                }

                // If no suitable animation is found, reset played animations
                ResetPlayedAnimations();
                return null;
            }

            ResetPlayedAnimations();
            return null;

        }

        string[] GetRemainingClips()
        {
            List<string> remaining = new List<string>();

            foreach (string clip in allAnimationClips)
            {
                if (!playedAnimations.Contains(clip) && clip != "First Slash Inward Normal")
                {
                    remaining.Add(clip);
                }
            }

            return remaining.ToArray();
        }

        void ResetPlayedAnimations()
        {
            playedAnimations.Clear();
        }

        IEnumerator SheathSword()
        {
            int drawLayer = animator.GetLayerIndex("Combat Handle");

            animator.SetLayerWeight(drawLayer, 1);
            animator.Play("Dummy|Sheath Weapon");

            Debug.Log("Sword has been Put Away");

            yield return new WaitForSeconds(1.2f);
            animator.SetLayerWeight(drawLayer, 0);

            isSwordShefed = true;

            yield break;
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

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

        // Add these variables for movement towards Town Hall and fading
        public Transform FadeObjectTransform;
        public Transform exitTransform;
        public float moveSpeed = 5f;
        public float fadeDuration = 2f;
        private bool isMovingTowardsTownHall = false;
        public bool isMovingAwayFromTownHall = false;
        private float startTime;
        private Color originalColor;
        public  Material playerMaterial;


        public float movement_Velocity;
        private float dampVelocity;
        Vector3 InputVec;
        GM gameManager;
        // 
        public float GravityStrength;
        public bool isGrounded;
        public float jumpHeight;

        public bool insideEventArea = false;

        public TextMeshPro interactiveText;

        // Start is called before the first frame update
        void Start()
        {
            
            playerMaterial = GetComponent<Renderer>().material;
            originalColor = playerMaterial.color;

            interactiveText.enabled = false;
            gameManager = GameObject.FindObjectOfType<GM>();
            Vector3 gravityS = new Vector3(0, GravityStrength, 0);

            Physics.gravity = gravityS;


            isGrounded = true;


            player_RB = gameObject.AddComponent<Rigidbody>();
            player_col = gameObject.AddComponent<BoxCollider>();

            player_RB.constraints = RigidbodyConstraints.FreezeRotation;
        }

        void StartMovingTowardsFadeTarget()
        {
            interactiveText.enabled = false;
            isMovingTowardsTownHall = true;
            startTime = Time.time;
            movement_Velocity = 0;
        }

        void MoveTowardsFadeTarget()
        {
            float journeyLength = Vector3.Distance(transform.position, FadeObjectTransform.position);
            float journeyTime = Time.time - startTime;
            float fractionOfJourney = journeyTime / fadeDuration;

            transform.position = Vector3.Lerp(transform.position, FadeObjectTransform.position, moveSpeed * Time.deltaTime);

            // Fade the player by modifying the alpha component of the material color
            Color fadedColor = originalColor;
            fadedColor.a = Mathf.Lerp(1f, 0f, fractionOfJourney);
            playerMaterial.color = fadedColor;

            if (fractionOfJourney >= 1f)
            {
                isMovingTowardsTownHall = false;
                movement_Velocity = 0;
                if (CameraSwitcher.IsCameraActive(gameManager.gameplayCamera))   // Am I currently In Game View?
                {
                    // change the current camera to be the upgradecamera (Village Camera View)
                    CameraSwitcher.SwitchCamera(gameManager.villageCamera);
                    // Turn off Player Input On PC 
                    //PC.GetComponent<PC_3D_Controller>().enabled = false;
                    // Turn on my Camera animation handler - Of type Script
                    gameManager.UpgradeHelper.GetComponent<UpgradeDetection_Helper>().enabled = true;
                    gameManager.currentSelectedVillagePiece = null;
                }
            }

        }

        public void MoveAwayFromFadeTarget()
        {
            float journeyTime = Time.time - startTime;
            float fractionOfJourney = journeyTime / fadeDuration;

            // Calculate the world position of the exitTransform
            Vector3 exitPosition = exitTransform.TransformPoint(Vector3.zero);

            // Calculate the direction from the current position to the exitTransform
            Vector3 direction = (transform.position - exitPosition).normalized;

            // Calculate the new position by moving away from the exitTransform
            Vector3 newPosition = transform.position + -direction * moveSpeed * Time.deltaTime;
            transform.LookAt(newPosition);
            transform.position = newPosition;

            // Increase the alpha component of the material color
            Color fadedColor = originalColor;
            fadedColor.a = Mathf.Lerp(0f, 1f, fractionOfJourney);
            playerMaterial.color = fadedColor;

            if (fractionOfJourney >= 1f)
            {
                // The journey is complete, you can reset variables or take other actions.
                //isMovingAwayFromTownHall = false; // You may need to create this boolean variable
                movement_Velocity = 5;
            }

            // Check if the distance between the current position and exitTransform is small enough
            // to consider the object has reached the exitTransform
            if (Vector3.Distance(transform.position, exitPosition) < 1f)
            {
                isMovingAwayFromTownHall = false;
                exitTransform.gameObject.SetActive(true);
            }

        }



        // Update is called once per frame
        void Update()
        {
            GatherInput_Motion();
            if(!isMovingTowardsTownHall)
            {
                Look();
            }
            else
            {
                transform.LookAt(FadeObjectTransform);
            }

            if (isMovingAwayFromTownHall)
                MoveAwayFromFadeTarget();


            if (Input.GetKeyDown(KeyCode.J) && isGrounded == true)
            {
                Jump();
            }

            // Check if inside the event area and start moving and fading
            if (insideEventArea && Input.GetKeyDown(KeyCode.E) && !isMovingTowardsTownHall)
            {
                StartMovingTowardsFadeTarget();
            }

        }

        private void FixedUpdate()
        {
            

            if (isMovingTowardsTownHall)
            {
                MoveTowardsFadeTarget();
            }
            else
            {
                Move();
            }

        }

        void GatherInput_Motion()
        {
            if(isGrounded)  // prevents player direction change mid air
            {
                // gather input from the current preference in unity 
                InputVec = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            }
        }

        void Look()
        {
            if(InputVec != Vector3.zero)
            {

                // make position current and pass through input
                var relative = (transform.position + InputVec.ToIso()) - transform.position;
                // rotation calculation
                var rot = Quaternion.LookRotation(relative, Vector3.up);
                // new rotation
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, 360);
            }

        }

        void Move()
        {
            // move player in the position of forward while taking into consideration the input detection and speed
            player_RB.MovePosition(transform.position + (transform.forward * InputVec.magnitude)
                * movement_Velocity * Time.deltaTime);

            // Later 
            // Allow for a way to slow down with velocity due to no longer moving.
            // Or just have a snappy type of movement#
            

        }

        void Jump()
        {
            player_RB.AddForce(new Vector3(0, jumpHeight, 0));
            isGrounded = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(collision.gameObject.layer == 3 || collision.gameObject.layer == 7)
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
            interactiveText.enabled = false ;
        }
    }
}


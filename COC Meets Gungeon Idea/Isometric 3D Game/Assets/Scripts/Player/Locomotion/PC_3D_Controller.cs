using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Main.Player_Locomotion
{
    public class PC_3D_Controller : MonoBehaviour
    {
        private Rigidbody player_RB;
        private BoxCollider player_col;
        public float movement_Velocity;
        private float dampVelocity;
        Vector3 InputVec;



        public float GravityStrength;
        public bool isGrounded;
        public float jumpHeight;

        // Start is called before the first frame update
        void Start()
        {

            Vector3 gravityS = new Vector3(0, GravityStrength, 0);

            Physics.gravity = gravityS;

            isGrounded = true;


            player_RB = gameObject.AddComponent<Rigidbody>();
            player_col = gameObject.AddComponent<BoxCollider>();
        }

        // Update is called once per frame
        void Update()
        {
            //gather input data on runtime
            GatherInput_Motion();
            // update rotation while in runtime
            Look();

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true)
            {
                Jump();
            }
        }

        private void FixedUpdate()
        {
            // call movement in fixed for physics 
            Move();
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
    }
}


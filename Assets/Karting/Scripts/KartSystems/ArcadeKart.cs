using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text;
using System;
using System.Threading;

namespace KartGame.KartSystems
{
    enum CommandPlayEnum
    {
        Connect = 0,
        Playing = 1
    }
    class Vector
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public override string ToString()
        {
            return $"{X} {Y} {Z}";
        }
    }
    class Play
    {
        public string Name { get; set; }
        public bool IsOne { get; set; }
        public Vector Position { get; set; }
        public CommandPlayEnum Command { get; set; }
        public override string ToString()
        {
            return $"Name: {Name}\t IsONe: {IsOne}\t " +
                $"Position: {Position}\t Command: {Command}";
        }
    }
    public class ArcadeKart : MonoBehaviour
    {
        /// <summary>
        /// Contains parameters that can adjust the kart's behaviors temporarily.
        /// </summary>
        [System.Serializable]
        public class StatPowerup
        {
            public ArcadeKart.Stats modifiers;
            public string PowerUpID;
            public float ElapsedTime;
            public float MaxTime;
        }
    
        /// <summary>
        /// Contains a series tunable parameters to tweak various karts for unique driving mechanics.
        /// </summary>
        [System.Serializable]
        public struct Stats
        {
            [Header("Movement Settings")]
            [Tooltip("The maximum speed forwards")]
            public float TopSpeed;

            [Tooltip("How quickly the Kart reaches top speed.")]
            public float Acceleration;

            [Tooltip("The maximum speed backward.")]
            public float ReverseSpeed;

            [Tooltip("The rate at which the kart increases its backward speed.")]
            public float ReverseAcceleration;

            [Tooltip("How quickly the Kart starts accelerating from 0. A higher number means it accelerates faster sooner.")]
            [Range(0.2f, 1)]
            public float AccelerationCurve;

            [Tooltip("How quickly the Kart slows down when going in the opposite direction.")]
            public float Braking;

            [Tooltip("How quickly to slow down when neither acceleration or reverse is held.")]
            public float CoastingDrag;

            [Range(0, 1)]
            [Tooltip("The amount of side-to-side friction.")]
            public float Grip;

            [Tooltip("How quickly the Kart can turn left and right.")]
            public float Steer;

            [Tooltip("Additional gravity for when the Kart is in the air.")]
            public float AddedGravity;

            [Tooltip("How much the Kart tries to keep going forward when on bumpy terrain.")]
            [Range(0, 1)]
            public float Suspension;

            // allow for stat adding for powerups.
            public static Stats operator +(Stats a, Stats b)
            {
                return new Stats
                {
                    Acceleration        = a.Acceleration + b.Acceleration,
                    AccelerationCurve   = a.AccelerationCurve + b.AccelerationCurve,
                    Braking             = a.Braking + b.Braking,
                    CoastingDrag        = a.CoastingDrag + b.CoastingDrag,
                    AddedGravity        = a.AddedGravity + b.AddedGravity,
                    Grip                = a.Grip + b.Grip,
                    ReverseAcceleration = a.ReverseAcceleration + b.ReverseAcceleration,
                    ReverseSpeed        = a.ReverseSpeed + b.ReverseSpeed,
                    TopSpeed            = a.TopSpeed + b.TopSpeed,
                    Steer               = a.Steer + b.Steer,
                    Suspension          = a.Suspension + b.Suspension
                };
            }
        }

        public Rigidbody Rigidbody { get; private set; }
        public Vector2 Input       { get; private set; }
        public float AirPercent    { get; private set; }
        public float GroundPercent { get; private set; }
        

        public ArcadeKart.Stats baseStats = new ArcadeKart.Stats
        {
            TopSpeed            = 1000f,
            Acceleration        = 50f,
            AccelerationCurve   = 40f,
            Braking             = 100f,
            ReverseAcceleration = 50f,
            ReverseSpeed        = 50f,
            Steer               = 50f,
            CoastingDrag        = 40f,
            Grip                = 1.95f,
            AddedGravity        = 10f,
            Suspension          = 1.2f
        };

        [Header("Vehicle Physics")]
        [Tooltip("The transform that determines the position of the Kart's mass.")]
        public Transform CenterOfMass;

        [Tooltip("The physical representations of the Kart's wheels.")]
        public Transform[] Wheels;

        [Tooltip("Which layers the wheels will detect.")]
        public LayerMask GroundLayers = Physics.DefaultRaycastLayers;

        [Tooltip("How far to raycast when checking for ground.")]
        public float RaycastDist = 0.3f;

        [Tooltip("How high to keep the kart above the ground.")]
        public float MinHeightThreshold = 0.02f;

        public Transform SuspensionBody;

        // saved transforms of where the suspension's neutral positions are
        Vector3 suspensionNeutralPos;
        Quaternion suspensionNeutralRot;

        // the input sources that can control the kart
        IInput[] m_Inputs;

        // can the kart move?
        bool canMove = true;
        List<StatPowerup> activePowerupList = new List<StatPowerup>();
        GameObject lastGroundCollided = null;
        ArcadeKart.Stats finalStats;

        void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            m_Inputs = GetComponents<IInput>();
            suspensionNeutralPos = SuspensionBody.transform.localPosition;
            suspensionNeutralRot = SuspensionBody.transform.localRotation;
        }
        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.  
                var r = ar.AsyncState;
                StateObject state = (StateObject)ar.AsyncState;
                string resp1 = Encoding.UTF8.GetString(state.buffer);
                Socket client = state.workSocket;
                Thread.Sleep(20);
                //Read data from the remote device.
                //int bytesRead = client.EndReceive(ar);

                    // There might be more data, so store the data received so far.  
                    //state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    //string resp = Encoding.UTF8.GetString(state.buffer, 0, bytesRead);
                    // Get the rest of the data.  
                    //client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                      //  new AsyncCallback(ReceiveCallback), state);
                //}
                //else
                //{
                //    // All the data has arrived; put it in response.  
                //    if (state.sb.Length > 1)
                //    {
                //        var response = state.sb.ToString();
                //    }
                //    // Signal that all bytes have been received.  
                //    //receiveDone.Set();
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public class StateObject
        {
            // Client socket.  
            public Socket workSocket = null;
            // Size of receive buffer.  
            public const int BufferSize = 1024;
            // Receive buffer.  
            public byte[] buffer = new byte[BufferSize];
            // Received data string.  
            public StringBuilder sb = new StringBuilder();
        }

        async void connect(Vector3 c)
        {
            IPAddress ip = IPAddress.Parse("95.214.10.36");//IPAddress.Parse("127.0.0.1"); //Dns.GetHostAddresses("google.com.ua")[0];
            IPEndPoint ep = new IPEndPoint(ip, 560);
            Socket s = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.IP);
            try
            {
                await s.ConnectAsync(ep);
                if (s.Connected)
                {
                    string strSend = c.ToString();//"Привіт. Я debil. ya kablan\r\n\r\n";
                                                  //SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                                                  //e.SetBuffer(buffer, 0, buffer.Length);
                    Play play = new Play()
                    {
                        IsOne = true,
                        Command = CommandPlayEnum.Playing,
                        Name ="Daniyl",
                        Position = new Vector()
                        {
                            X = c.x,
                            Y = c.y,
                            Z= c.z
                        }
                    };
                    var json = JsonConvert.SerializeObject(play);
                    s.Send( Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(play)));
                    byte[] buffer = new byte[1024];
                    StateObject state = new StateObject();
                    state.workSocket = s;
                    state.buffer = buffer;
                    s.BeginReceive(buffer, 0, buffer.Length, 0,
                    new AsyncCallback(ReceiveCallback), state);
                    
                    //int l;
                    //do
                    //{
                    //     l =  s.Receive(buffer);
                    //    //txtMesssage.Text += Encoding.UTF8.GetString(buffer, 0, l);
                    //} while (l > 0);
                    //txtMesssage.Text = "Connected good";
                }

            }
            catch (Exception ex)
            {
            }
            finally
            {
                s.Shutdown(SocketShutdown.Both);
                s.Close();
            }
        }
        void FixedUpdate()
        {
            ResetIfStuck();

            GatherInputs();

            // apply our powerups to create our finalStats
            TickPowerups();

            // apply our physics properties
            Rigidbody.centerOfMass = Rigidbody.transform.InverseTransformPoint(CenterOfMass.position);

            // calculate how grounded and airborne we are
            int groundedCount = CountGroundedWheels(out float minHeight);
            GroundPercent = (float)groundedCount / Wheels.Length;
            AirPercent = 1 - GroundPercent;
            var c = transform.position;
            //Network.PostData("byrgyika", c, c);
            connect(c);
            
                // gather inputs
                float accel = Input.y;
            float turn = Input.x;

            // apply vehicle physics
            GroundVehicle(minHeight);
            if (canMove)
            {
                MoveVehicle(accel, turn);
            }
            GroundAirbourne();

            // animation
            AnimateSuspension();
        }

        void GatherInputs()
        {
            // reset input
            Input = Vector2.zero;

            // gather nonzero input from our sources
            for (int i = 0; i < m_Inputs.Length; i++)
            {
                var inputSource = m_Inputs[i];
                Vector2 current = inputSource.GenerateInput();
                if (current.sqrMagnitude > 0)
                {
                    Input = current;
                }
            }
        }

        void TickPowerups()
        {
            // remove all elapsed powerups
            activePowerupList.RemoveAll((p) => { return p.ElapsedTime > p.MaxTime; });

            // zero out powerups before we add them all up
            var powerups = new Stats();

            // add up all our powerups
            for (int i = 0; i < activePowerupList.Count; i++)
            {
                var p = activePowerupList[i];

                // add elapsed time
                p.ElapsedTime += Time.deltaTime;

                // add up the powerups
                powerups += p.modifiers;
            }

            // add powerups to our final stats
            finalStats = baseStats + powerups;

            // clamp values in finalstats
            finalStats.Grip = Mathf.Clamp(finalStats.Grip, 0, 1);
            finalStats.Suspension = Mathf.Clamp(finalStats.Suspension, 0, 1);
        }

        void AnimateSuspension()
        {
            // simple suspension animation
            var suspensionTargetPos = suspensionNeutralPos;
            var suspensionTargetRot = suspensionNeutralRot;
            var bodyRot = transform.rotation.eulerAngles;

            var maxXTilt = finalStats.Suspension * 45;
            var closestNeutralRot = Mathf.Abs(360 - bodyRot.x) < Mathf.Abs(bodyRot.x) ? 360 : 0;
            var xTilt = Mathf.DeltaAngle(closestNeutralRot, bodyRot.x);

            var suspensionT = Mathf.InverseLerp(0, maxXTilt, xTilt);
            suspensionT = suspensionT * suspensionT;

            //Debug.Log("Suspension: " + suspensionT + " bodyRot: "  + bodyRot.x + " neutral: " + closestNeutralRot);
            bodyRot.x = Mathf.Lerp(closestNeutralRot, bodyRot.x, suspensionT);

            // transform bodyRot to suspension local space
            suspensionTargetRot = Quaternion.Inverse(SuspensionBody.transform.rotation) * Quaternion.Euler(bodyRot);

            // apply the new transforms
            SuspensionBody.transform.localPosition = Vector3.Lerp(SuspensionBody.transform.localPosition, suspensionTargetPos, Time.deltaTime * 5f);
            SuspensionBody.transform.localRotation = Quaternion.Slerp(SuspensionBody.transform.localRotation, suspensionTargetRot, Time.deltaTime * 5f);
        }

        int CountGroundedWheels(out float minHeight)
        {
            int groundedCount = 0;
            minHeight = float.MaxValue;

            for (int i = 0; i < Wheels.Length; i++)
            {
                Transform current = Wheels[i];
                groundedCount += Physics.Raycast(current.position, Vector3.down, out RaycastHit hit, RaycastDist) ? 1 : 0;

                if (hit.distance > 0)
                {
                    minHeight = Mathf.Min(hit.distance, minHeight);
                }
            }

            return groundedCount;
        }

        void GroundVehicle(float minHeight)
        {
            if (GroundPercent >= 1f)
            {
                if (minHeight < MinHeightThreshold)
                {
                    float diff = MinHeightThreshold - minHeight;
                    transform.position += diff * transform.up;
                }
            }
        }

        void GroundAirbourne()
        {
            // while in the air, fall faster
            if (AirPercent >= 1)
            {
                Rigidbody.velocity += Physics.gravity * Time.deltaTime * finalStats.AddedGravity;
            }
        }

        void MoveVehicle(float accelInput, float turnInput)
        {
            // manual acceleration curve coefficient scalar
            float accelerationCurveCoeff = 5;
            Vector3 localVel = transform.InverseTransformVector(Rigidbody.velocity);

            bool accelDirectionIsFwd = accelInput >= 0;
            bool localVelDirectionIsFwd = localVel.z >= 0;

            // use the max speed for the direction we are going--forward or reverse.
            float maxSpeed = accelDirectionIsFwd ? finalStats.TopSpeed : finalStats.ReverseSpeed;
            float accelPower = accelDirectionIsFwd ? finalStats.Acceleration : finalStats.ReverseAcceleration;

            float accelRampT = Rigidbody.velocity.magnitude / maxSpeed;
            float multipliedAccelerationCurve = finalStats.AccelerationCurve * accelerationCurveCoeff;
            float accelRamp = Mathf.Lerp(multipliedAccelerationCurve, 1, accelRampT * accelRampT);

            bool isBraking = accelDirectionIsFwd != localVelDirectionIsFwd;

            // if we are braking (moving reverse to where we are going)
            // use the braking accleration instead
            float finalAccelPower = isBraking ? finalStats.Braking : accelPower;

            float finalAcceleration = finalAccelPower * accelRamp;

            // apply inputs to forward/backward
            float turningPower = turnInput * finalStats.Steer;

            Quaternion turnAngle = Quaternion.AngleAxis(turningPower, Rigidbody.transform.up);
            Vector3 fwd = turnAngle * Rigidbody.transform.forward;

            Vector3 movement = fwd * accelInput * finalAcceleration * GroundPercent;

            // simple suspension allows us to thrust forward even when on bumpy terrain
            fwd.y = Mathf.Lerp(fwd.y, 0, finalStats.Suspension);

            // forward movement
            float currentSpeed = Rigidbody.velocity.magnitude;
            bool wasOverMaxSpeed = currentSpeed >= maxSpeed;

            // if over max speed, cannot accelerate faster.
            if (wasOverMaxSpeed && !isBraking) movement *= 0;

            Vector3 adjustedVelocity = Rigidbody.velocity + movement * Time.deltaTime;

            adjustedVelocity.y = Rigidbody.velocity.y;

            //  clamp max speed if we are on ground
            if (GroundPercent > 0)
            {
                if (adjustedVelocity.magnitude > maxSpeed && !wasOverMaxSpeed)
                {
                    adjustedVelocity = Vector3.ClampMagnitude(adjustedVelocity, maxSpeed);
                }
            }

            // coasting is when we aren't touching accelerate
            bool isCoasting = Mathf.Abs(accelInput) < .01f;

            if (isCoasting)
            {
                Vector3 restVelocity = new Vector3(0, Rigidbody.velocity.y, 0);
                adjustedVelocity = Vector3.MoveTowards(adjustedVelocity, restVelocity, Time.deltaTime * finalStats.CoastingDrag);
            }

            Rigidbody.velocity = adjustedVelocity;

            ApplyAngularSuspension();

            if (GroundPercent > 0)
            {
                // manual angular velocity coefficient
                float angularVelocitySteering = .4f;
                float angularVelocitySmoothSpeed = 20f;

                // turning is reversed if we're going in reverse and pressing reverse
                if (!localVelDirectionIsFwd && !accelDirectionIsFwd) angularVelocitySteering *= -1;
                var angularVel = Rigidbody.angularVelocity;

                // move the Y angular velocity towards our target
                angularVel.y = Mathf.MoveTowards(angularVel.y, turningPower * angularVelocitySteering, Time.deltaTime * angularVelocitySmoothSpeed);

                // apply the angular velocity
                Rigidbody.angularVelocity = angularVel;

                // rotate rigidbody's velocity as well to generate immediate velocity redirection
                // manual velocity steering coefficient
                float velocitySteering = 25f;
                // rotate our velocity based on current steer value
                Rigidbody.velocity = Quaternion.Euler(0f, turningPower * velocitySteering * Time.deltaTime, 0f) * Rigidbody.velocity;
            }

            // apply simplified lateral ground friction
            // only apply if we are on the ground at all
            if (GroundPercent > 0f)
            {
                // manual grip coefficient scalar
                float gripCoeff = 30f;
                // what direction is our lateral friction in?
                // it is the direction the wheels are turned, our forward
                Vector3 latFrictionDirection = Vector3.Cross(fwd, transform.up);
                // how fast are we currently moving in our friction direction?
                float latSpeed = Vector3.Dot(Rigidbody.velocity, latFrictionDirection);
                // apply the damping
                Vector3 latFrictionDampedVelocity = Rigidbody.velocity - latFrictionDirection * latSpeed * finalStats.Grip * gripCoeff * Time.deltaTime;

                // apply the damped velocity
                Rigidbody.velocity = latFrictionDampedVelocity;
            }
        }

        void ApplyAngularSuspension()
        {
            // simple suspension dampens x and z angular velocity while on the ground
            Vector3 suspendedX = transform.right;
            Vector3 suspendedZ = transform.forward;
            suspendedX.y *= 0f;
            suspendedZ.y *= 0f;
            var sX = Vector3.Dot(Rigidbody.angularVelocity, suspendedX) * suspendedX;
            var sZ = Vector3.Dot(Rigidbody.angularVelocity, suspendedZ) * suspendedZ;
            var sXZ = sX + sZ;
            var sCoeff = 10f;

            Vector3 suspensionRotation;
            float minimumSuspension = 0.5f;
            if (GroundPercent > 0.5f || finalStats.Suspension < minimumSuspension)
            {
                suspensionRotation = sXZ * finalStats.Suspension * sCoeff * Time.deltaTime;
            }
            else
            {
                suspensionRotation = sXZ * minimumSuspension * sCoeff * Time.deltaTime;
            }

            Vector3 suspendedAngular = Rigidbody.angularVelocity - suspensionRotation;

            // apply the adjusted angularvelocity
            Rigidbody.angularVelocity = suspendedAngular;
        }

        bool IsStuck()
        {
            float speed = Rigidbody.velocity.magnitude;
            if(GroundPercent <= 0 && speed < 0.01f && Mathf.Abs(Input.y) > 0)
                return true;

            return false;
        }

        void OnCollisionEnter(Collision other)
        {
            if(GroundLayers == (GroundLayers | (1 << other.collider.gameObject.layer)))
            {
                lastGroundCollided = other.collider.gameObject;
            }
        }

        void ResetIfStuck()
        {
            if (IsStuck() && lastGroundCollided != null)
            {
                if (lastGroundCollided.TryGetComponent(out Collider collider))
                {
                    Vector3 pos = new Vector3(
                        collider.bounds.center.x,
                        collider.bounds.max.y,
                        collider.bounds.center.z
                    );
                    transform.position = pos;
                }
            }
        }

        public void AddPowerup(StatPowerup statPowerup)
        {
            activePowerupList.Add(statPowerup);
        }

        public void Reset()
        {
            Vector3 euler      = transform.rotation.eulerAngles;
            euler.x            = euler.z = 0f;
            transform.rotation = Quaternion.Euler(euler);
        }

        public float LocalSpeed()
        {
            if (canMove)
            {
                float dot = Vector3.Dot(transform.forward, Rigidbody.velocity);
                if (Mathf.Abs(dot) > 0.1f)
                {
                    float speed = Rigidbody.velocity.magnitude;
                    return dot < 0 ? -(speed / finalStats.ReverseSpeed) : (speed / finalStats.TopSpeed);
                }
                return 0f;
            }
            else
            {
                // use this value to play kart sound when it is waiting the race start countdown.
                return Input.y;
            }
        }

        public void SetCanMove(bool move)
        {
            canMove = move;
        }
    }
}
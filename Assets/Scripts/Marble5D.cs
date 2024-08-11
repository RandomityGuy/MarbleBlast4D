using R50;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public struct Move5D
{
    public Vector4 mv;
    public bool powerup;
    public bool jump;
}

public struct CollisionInfo5D
{
    public Vector5 point;

    public Vector5 normal;

    public Vector5 velocity;

    public float friction;

    public float restitution;

    public float penetration;
}

public class Marble5D : MonoBehaviour
{
    public Vector5 position;
    public Vector5 velocity;
    public BiVector5 omega;
    R500 orientation;

    Vector5 currentUp = new Vector5(0, 1, 0, 0, 0);

    public float radiusOfGyration = 3f; // https://github.com/EpiTorres/into-another-dimension/tree/main

    Vector5 lastRenderedPosition;

    private float _radius = 0.2f;

    private float _maxRollVelocity = 17.32f; // 15f;

    private float _angularAcceleration = 106.06f; // 75f;

    private float _jumpImpulse = 7.5f;

    private float _kineticFriction = 0.7f;

    private float _staticFriction = 1.1f;

    private float _brakingAcceleration = 42.426f; // 30f;

    private float _gravity = 20f;

    private float _airAccel = 5.773f; // 5f;

    private float _maxDotSlide = 0.5f;

    private float _minBounceVel = 0.1f;

    private float _bounceKineticFriction = 1.0f; // 0.2f;

    private float _bounceRestitution = 0.5f;

    private bool _bounceYet;

    private float _bounceSpeed;

    private Vector5 _bouncePos;

    private Vector5 _bounceNormal;

    private float _slipAmount;

    private float _contactTime;

    private float _totalTime;

    public MarbleCameraController5D camera;

    List<CollisionInfo5D> contacts = new List<CollisionInfo5D>();

    // Start is called before the first frame update
    void Start()
    {
        lastRenderedPosition = this.gameObject.GetComponent<Object5D>().worldPosition5D;
        position = lastRenderedPosition;
        orientation = new R500(1);
    }

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.position = new Vector3(position.x, position.y, position.z); // Transform4D.XYZ(lastRenderedPosition) + Transform4D.XYZ(velocity) * Time.deltaTime;
        this.gameObject.GetComponent<Object5D>().positionW = position.w; // lastRenderedPosition.w + velocity.w * Time.deltaTime;
        this.gameObject.GetComponent<Object5D>().positionV = position.v;
    }

    private void FixedUpdate()
    {
        lastRenderedPosition = position;

        Move5D mv;
        mv.mv = new Vector4(0, 0, 0, 0);
        mv.jump = false;
        if (InputManager.GetKey(InputManager.KeyBind.Left))
            mv.mv.x = -1;
        if (InputManager.GetKey(InputManager.KeyBind.Right))
            mv.mv.x = 1;
        if (InputManager.GetKey(InputManager.KeyBind.Forward))
            mv.mv.y = 1;
        if (InputManager.GetKey(InputManager.KeyBind.Backward))
            mv.mv.y = -1;
        if (InputManager.GetKey(InputManager.KeyBind.Ana))
            mv.mv.z = -1;
        if (InputManager.GetKey(InputManager.KeyBind.Kata))
            mv.mv.z = 1;
        if (InputManager.GetKey(InputManager.KeyBind.Sursum))
            mv.mv.w = 1;
        if (InputManager.GetKey(InputManager.KeyBind.Deorsum))
            mv.mv.w = -1;
        if (InputManager.GetKey(InputManager.KeyBind.Putt))
            mv.jump = true;
        
        mv.powerup = false;

        AdvancePhysics(mv, Time.fixedDeltaTime);
        position += velocity * Time.fixedDeltaTime;

        var deltaOmega = omega * Time.deltaTime * 0.5f;
        var deltaOmegaR = R500.e12 * deltaOmega.xy + R500.e13 * deltaOmega.xz + R500.e14 * deltaOmega.xw + R500.e15 * deltaOmega.xv +
            R500.e23 * deltaOmega.yz + R500.e24 * deltaOmega.yw + R500.e25 * deltaOmega.yv + R500.e34 * deltaOmega.zw + R500.e35 * deltaOmega.zv + R500.e45 * deltaOmega.wv;

        orientation = orientation - deltaOmegaR * orientation;
        var mat = new Matrix5x5(
            orientation * new Vector5(1, 0, 0, 0, 0),
            orientation * new Vector5(0, 1, 0, 0, 0),
            orientation * new Vector5(0, 0, 1, 0, 0),
            orientation * new Vector5(0, 0, 0, 1, 0),
            orientation * new Vector5(0, 0, 0, 0, 1)
        );

        this.gameObject.GetComponent<Object5D>().localRotation5D = mat;

        //var deltaOmega = omega;
        //var rotDelta = Transform4D.PlaneRotation(deltaOmega.xy, 0, 1) * Transform4D.PlaneRotation(deltaOmega.xz, 0, 2) * Transform4D.PlaneRotation(deltaOmega.xw, 0, 3) 
        //    * Transform4D.PlaneRotation(deltaOmega.yz, 1, 2) * Transform4D.PlaneRotation(deltaOmega.yw, 1, 3) * Transform4D.PlaneRotation(deltaOmega.zw, 2, 3);
        //this.gameObject.GetComponent<Object4D>().localRotation4D *= rotDelta;
    }

    void FindContacts()
    {
        Collider5D.Hit hit = Collider5D.Hit.Empty;
        foreach (KeyValuePair<int, ColliderGroup5D> kv in Collider5D.colliders)
        {
            //Cache object transforms for this group
            ColliderGroup5D colliderGroup = kv.Value;
            Object5D colliderObj = colliderGroup.colliders[0].obj5D;
            if (colliderObj == null)
            {
                LogReport.Error("Collider4D was not removed properly.");
                continue;
            }
            if (!colliderObj.isActiveAndEnabled) { continue; }
            Transform5D localToWorld5D = colliderObj.WorldTransform5D();
            Transform5D worldToLocal5D = localToWorld5D.inverse;
            if (!colliderGroup.IntersectsAABB(localToWorld5D, worldToLocal5D, position,  _radius)) { continue; }
            foreach (Collider5D collider in colliderGroup.colliders)
            {
                if (collider.gameObject == this.gameObject) continue;
                if (collider.Collide(localToWorld5D, worldToLocal5D, position, _radius, ref hit))
                {
                    var coll = new CollisionInfo5D();
                    coll.normal = hit.displacement.normalized;
                    coll.restitution = 1f;
                    coll.friction = 1;
                    coll.velocity = default(Vector5);
                    coll.point = position + hit.displacement;
                    coll.penetration = 0;
                    contacts.Add(coll);
                }
            }
        }
    }

    void AdvancePhysics(Move5D mv, float dt)
    {
        var remainingTime = dt;
        var it = 0;
        while (remainingTime > 0 && it < 10)
        {
            var timeStep = 0.008f;
            if (timeStep > remainingTime)
                timeStep = remainingTime;

            FindContacts();

            bool isCentered = this._computeMoveForces(mv, omega, out BiVector5 aControl, out BiVector5 desiredOmega);
            this._velocityCancel(contacts, ref velocity, ref omega, isCentered, false);
            Vector5 A = this._getExternalForces(mv, contacts);
            BiVector5 a;
            this._applyContactForces(dt, mv, contacts, isCentered, aControl, desiredOmega, ref velocity, ref omega, ref A, out a);
            velocity += A * dt;
            omega += a * dt;
            this._velocityCancel(contacts, ref velocity, ref omega, isCentered, true);
            this._totalTime += dt;
            if (contacts.Count != 0)
            {
                this._contactTime += dt;
            }

            contacts.Clear();

            remainingTime -= timeStep;
            it++;
        }
    }

    private bool _computeMoveForces(Move5D mv, BiVector5 omega, out BiVector5 aControl, out BiVector5 desiredOmega)
    {
        aControl = default(BiVector5);
        desiredOmega = default(BiVector5);
        Vector5 gWorkGravityDir = -currentUp;
        Vector5 R = -gWorkGravityDir * this._radius;
        Vector5 rollVelocity = R * omega; 
        Vector5 sideDir;
        Vector5 motionDir;
        Vector5 upDir;
        Vector5 wDir;
        Vector5 vDir;
        this._getMarbleAxis(out sideDir, out motionDir, out upDir, out wDir, out vDir);
        float currentYVelocity = Vector5.Dot(rollVelocity, motionDir);
        float currentXVelocity = Vector5.Dot(rollVelocity, sideDir);
        float currentWVelocity = Vector5.Dot(rollVelocity, wDir);
        float currentVVelocity = Vector5.Dot(rollVelocity, vDir);


        float desiredYVelocity = this._maxRollVelocity * mv.mv.y;
        float desiredXVelocity = this._maxRollVelocity * mv.mv.x;
        float desiredWVelocity = this._maxRollVelocity * mv.mv.z;
        float desiredVVelocity = this._maxRollVelocity * mv.mv.w;
        if (desiredYVelocity != 0f || desiredXVelocity != 0f || desiredWVelocity != 0f || desiredVVelocity != 0f)
        {
            if (currentYVelocity > desiredYVelocity && desiredYVelocity > 0f)
            {
                desiredYVelocity = currentYVelocity;
            }
            else if (currentYVelocity < desiredYVelocity && desiredYVelocity < 0f)
            {
                desiredYVelocity = currentYVelocity;
            }
            
            if (currentXVelocity > desiredXVelocity && desiredXVelocity > 0f)
            {
                desiredXVelocity = currentXVelocity;
            }
            else if (currentXVelocity < desiredXVelocity && desiredXVelocity < 0f)
            {
                desiredXVelocity = currentXVelocity;
            }
            
            if (currentWVelocity > desiredWVelocity && desiredWVelocity > 0f)
            {
                desiredWVelocity = currentWVelocity;
            }
            else if (currentWVelocity < desiredWVelocity && desiredWVelocity < 0f)
            {
                desiredWVelocity = currentWVelocity;
            }

            if (currentVVelocity > desiredVVelocity && desiredVVelocity > 0f)
            {
                desiredVVelocity = currentVVelocity;
            }
            else if (currentVVelocity < desiredVVelocity && desiredVVelocity < 0f)
            {
                desiredVVelocity = currentVVelocity;
            }

            desiredOmega = Transform5D.ExteriorProduct(R, desiredYVelocity * motionDir + desiredXVelocity * sideDir + desiredWVelocity * wDir + desiredVVelocity * vDir) / R.sqrMagnitude;
            aControl = desiredOmega - omega;
            float aScalar = aControl.magnitude;
            if (aScalar > this._angularAcceleration)
            {
                aControl *= this._angularAcceleration / aScalar;
            }
            return false;
        }
        return true;
    }

    private void _applyContactForces(float dt, Move5D mv, List<CollisionInfo5D> contacts, bool isCentered, BiVector5 aControl, BiVector5 desiredOmega, ref Vector5 velocity, ref BiVector5 omega, ref Vector5 A, out BiVector5 a)
    {
        a = default(BiVector5);
        this._slipAmount = 0f;
        Vector5 gWorkGravityDir = -currentUp;
        int bestSurface = -1;
        float bestNormalForce = 0f;
        for (int i = 0; i < contacts.Count; i++)
        {
            //if (contacts[i].collider == null)
            //{
                float normalForce = -Vector5.Dot(contacts[i].normal, A);
                if (normalForce > bestNormalForce)
                {
                    bestNormalForce = normalForce;
                    bestSurface = i;
                }
            //}
        }
        CollisionInfo5D bestContact = (bestSurface != -1) ? contacts[bestSurface] : default(CollisionInfo5D);
        bool canJump = bestSurface != -1;
        if (canJump && mv.jump)
        {
            Vector5 velDifference = velocity - bestContact.velocity;
            float sv = Vector5.Dot(bestContact.normal, velDifference);
            if (sv < 0f)
            {
                sv = 0f;
            }
            if (sv < this._jumpImpulse)
            {
                velocity += bestContact.normal * (this._jumpImpulse - sv);
               //  MarbleControlComponent._soundBank.PlayCue(MarbleControlComponent._sounds[12]);
            }
        }
        for (int j = 0; j < contacts.Count; j++)
        {
            float normalForce2 = -Vector5.Dot(contacts[j].normal, A);
            if (normalForce2 > 0f && Vector5.Dot(contacts[j].normal, velocity - contacts[j].velocity) <= 0.0001f)
            {
                A += contacts[j].normal * normalForce2;
            }
        }
        if (bestSurface != -1)
        {
            // TODO: FIX
            //bestContact.velocity - bestContact.normal * Vector3.Dot(bestContact.normal, bestContact.velocity);
            Vector5 vAtC = velocity + ((-bestContact.normal * this._radius) * omega) - bestContact.velocity;
            float vAtCMag = vAtC.magnitude;
            bool slipping = false;
            BiVector5 aFriction = new BiVector5(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
            Vector5 AFriction = new Vector5(0f, 0f, 0f, 0f, 0f);
            if (vAtCMag != 0f)
            {
                slipping = true;
                float friction = this._kineticFriction * bestContact.friction;
                float angAMagnitude = friction * bestNormalForce / (radiusOfGyration * this._radius); // https://math.stackexchange.com/questions/565333/moment-of-inertia-of-a-n-dimensional-sphere
                float AMagnitude = bestNormalForce * friction;
                float totalDeltaV = (angAMagnitude * this._radius + AMagnitude) * dt;
                if (totalDeltaV > vAtCMag)
                {
                    float fraction = vAtCMag / totalDeltaV;
                    angAMagnitude *= fraction;
                    AMagnitude *= fraction;
                    slipping = false;
                }
                Vector5 vAtCDir = vAtC / vAtCMag;

                aFriction = Transform5D.ExteriorProduct(-bestContact.normal, -vAtCDir) * angAMagnitude;
                AFriction = -AMagnitude * vAtCDir;
                this._slipAmount = vAtCMag - totalDeltaV;
            }
            if (!slipping)
            {
                Vector5 R = -gWorkGravityDir * this._radius;
                BiVector5 aadd = Transform5D.ExteriorProduct(R, A) / R.sqrMagnitude;
                if (isCentered)
                {
                    BiVector5 nextOmega = omega + a * dt;
                    aControl = desiredOmega - nextOmega;
                    float aScalar = aControl.magnitude;
                    if (aScalar > this._brakingAcceleration)
                    {
                        aControl *= this._brakingAcceleration / aScalar;
                    }
                }
                Vector5 Aadd = (-bestContact.normal * this._radius) * aControl;

                float aAtCMag = (((-bestContact.normal * this._radius) * aadd) + Aadd).magnitude;
                float friction2 = this._staticFriction * bestContact.friction;
                if (aAtCMag > friction2 * bestNormalForce)
                {
                    friction2 = this._kineticFriction * bestContact.friction;
                    Aadd *= friction2 * bestNormalForce / aAtCMag;
                }
                A += Aadd;
                a += aadd;

            }
            A += AFriction;
            a += aFriction;

        }
        a += aControl;
    }

    private Vector5 _getExternalForces(Move5D mv, List<CollisionInfo5D> contacts)
    {
        Vector5 gWorkGravityDir = -currentUp;
        Vector5 A = gWorkGravityDir * this._gravity;
        if (contacts.Count == 0)
        {
            Vector5 sideDir;
            Vector5 motionDir;
            Vector5 upDir;
            Vector5 wDir;
            Vector5 vDir;
            this._getMarbleAxis(out sideDir, out motionDir, out upDir, out wDir, out vDir);
            A += (sideDir * mv.mv.x + motionDir * mv.mv.y + wDir * mv.mv.z + vDir * mv.mv.w) * this._airAccel;
        }
        return A;
    }

    private void _velocityCancel(List<CollisionInfo5D> contacts, ref Vector5 velocity, ref BiVector5 omega, bool surfaceSlide, bool noBounce)
    {
        float SurfaceDotThreshold = 0.001f;
        bool looped = false;
        int itersIn = 0;
        bool done;
        do
        {
            done = true;
            itersIn++;
            for (int i = 0; i < contacts.Count; i++)
            {
                Vector5 sVel = velocity - contacts[i].velocity;
                float surfaceDot = Vector5.Dot(contacts[i].normal, sVel);
                if ((!looped && surfaceDot < 0f) || surfaceDot < -SurfaceDotThreshold)
                {
                    float velLen = velocity.magnitude;
                    Vector5 surfaceVel = surfaceDot * contacts[i].normal;
                    // this._reportBounce(contacts[i].point, contacts[i].normal, -surfaceDot);
                    if (noBounce)
                    {
                        velocity -= surfaceVel;
                    }
                    //else if (contacts[i].collider != null)
                    //{
                    //    CollisionInfo info = contacts[i];
                    //    MoveComponent otherMover = info.collider.Owner as MoveComponent;
                    //    if (otherMover != null)
                    //    {
                    //        float ourMass = 1f;
                    //        float theirMass = 1f;
                    //        float bounce = 0.5f;
                    //        Vector3 dp = velocity * ourMass - otherMover.Velocity * theirMass;
                    //        Vector3 normP = Vector3.Dot(dp, info.normal) * info.normal;
                    //        normP *= 1f + bounce;
                    //        velocity -= normP / ourMass;
                    //        otherMover.Velocity += normP / theirMass;
                    //        info.velocity = otherMover.Velocity;
                    //        contacts[i] = info;
                    //    }
                    //    else
                    //    {
                    //        float bounce2 = 0.5f;
                    //        Vector3 normV = Vector3.Dot(velocity, info.normal) * info.normal;
                    //        normV *= 1f + bounce2;
                    //        velocity -= normV;
                    //    }
                    //}
                    else
                    {
                        Vector5 velocity2 = contacts[i].velocity;
                        if (velocity2.magnitude < 0.0001f && !surfaceSlide && surfaceDot > -this._maxDotSlide * velLen)
                        {
                            velocity -= surfaceVel;
                            velocity.Normalize();
                            velocity *= velLen;
                            surfaceSlide = true;
                        }
                        else if (surfaceDot > -this._minBounceVel)
                        {
                            velocity -= surfaceVel;
                        }
                        else
                        {
                            float restitution = this._bounceRestitution;
                            restitution *= contacts[i].restitution;
                            Vector5 velocityAdd = -(1f + restitution) * surfaceVel;
                            Vector5 vAtC = sVel + (-contacts[i].normal * this._radius) * omega;
                            float normalVel = -Vector5.Dot(contacts[i].normal, sVel);
                            vAtC -= contacts[i].normal * Vector5.Dot(contacts[i].normal, sVel);
                            float vAtCMag = vAtC.magnitude;
                            if (vAtCMag != 0f)
                            {
                                float friction = this._bounceKineticFriction * contacts[i].friction;
                                float angVMagnitude = friction * normalVel / (radiusOfGyration * this._radius);
                                if (angVMagnitude > vAtCMag / this._radius)
                                {
                                    angVMagnitude = vAtCMag / this._radius;
                                }
                                Vector5 vAtCDir = vAtC / vAtCMag;
                                BiVector5 deltaOmega = Transform5D.ExteriorProduct(contacts[i].normal, vAtCDir);

                                deltaOmega *= angVMagnitude;
                                
                                omega += deltaOmega;
                                velocity -= (contacts[i].normal * this._radius) * deltaOmega;
                            }
                            velocity += velocityAdd;
                        }
                    }
                    done = false;
                }
            }
            looped = true;
            if (itersIn > 6 && noBounce)
            {
                done = true;
            }
        }
        while (!done);
        //if (velocity.sqrMagnitude < 625f)
        //{
        //    bool gotOne = false;
        //    Vector5 dir = new Vector5(0f, 0f, 0f, 0f);
        //    for (int j = 0; j < contacts.Count; j++)
        //    {
        //        Vector5 dir2 = dir + contacts[j].normal;
        //        if (dir2.sqrMagnitude < 0.01f)
        //        {
        //            dir2 += contacts[j].normal;
        //        }
        //        dir = dir2;
        //        gotOne = true;
        //    }
        //    if (gotOne)
        //    {
        //        dir.Normalize();
        //        float soFar = 0f;
        //        for (int k = 0; k < contacts.Count; k++)
        //        {
        //            if (contacts[k].penetration < this._radius)
        //            {
        //                float timeToSeparate = 0.1f;
        //                float dist = contacts[k].penetration;
        //                float outVel = Vector5.Dot(velocity + soFar * dir, contacts[k].normal);
        //                if (timeToSeparate * outVel < dist)
        //                {
        //                    soFar += (dist - outVel * timeToSeparate) / timeToSeparate / Vector5.Dot(contacts[k].normal, dir);
        //                }
        //            }
        //        }
        //        soFar = Mathf.Clamp(soFar, -25f, 25f);
        //        velocity += soFar * dir;
        //    }
        //}
    }

    private void _getMarbleAxis(out Vector5 sideDir, out Vector5 motionDir, out Vector5 upDir, out Vector5 wDir, out Vector5 vDir)
    {
        upDir = currentUp;

        sideDir = new Vector5(1, 0, 0, 0, 0);
        motionDir = new Vector5(0, 0, 1, 0, 0);
        wDir = new Vector5(0, 0, 0, 1, 0);
        vDir = new Vector5(0, 0, 0, 0, 1);

        sideDir = camera.camMatrix * sideDir;
        motionDir = camera.camMatrix * motionDir;

        if (camera.volumeMode)
        {
            if (camera.volumeV)
            {
                upDir = camera.camMatrix * vDir;
                vDir = camera.camMatrix * wDir;
                wDir = camera.camMatrix * upDir;
            }
            else
            {
                upDir = camera.camMatrix * wDir;
                wDir = camera.camMatrix * upDir;
                vDir = camera.camMatrix * vDir;
            }
        }
        else
        {
            upDir = camera.camMatrix * upDir;
            wDir = camera.camMatrix * wDir;
            vDir = camera.camMatrix * vDir;
        }

        //Matrix camMat = Matrix.Identity;
        //if (this._cameraX != null && this._cameraY != null)
        //{
        //    Matrix xRot = Matrix.CreateRotationX(-this._cameraY.Value);
        //    Matrix zRot = Matrix.CreateRotationZ(-this._cameraX.Value);
        //    camMat = Matrix.Multiply(xRot, zRot);
        //}
        //upDir = -gWorkGravityDir;
        //motionDir = MatrixUtil.MatrixGetRow(1, ref camMat);
        //sideDir = Vector3.Cross(motionDir, upDir);
        //sideDir.Normalize();
        //motionDir = Vector3.Cross(upDir, sideDir);
    }
}

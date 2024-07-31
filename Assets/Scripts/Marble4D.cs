using R40;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public struct Move
{
    public Vector3 mv;
    public bool powerup;
    public bool jump;
}

public struct CollisionInfo
{
    public Vector4 point;

    public Vector4 normal;

    public Vector4 velocity;

    public float friction;

    public float restitution;

    public float penetration;
}

public enum MarbleMode
{
    Start,
    Normal,
    Finish
}

public class Marble4D : MonoBehaviour
{
    public Vector4 position;
    public Vector4 velocity;
    public BiVector3 omega;
    Rotor4D orientation;

    public float radiusOfGyration = 2f; // https://github.com/EpiTorres/into-another-dimension/tree/main

    Vector4 lastRenderedPosition;

    Vector4 currentUp = new Vector4(0, 1, 0, 0);

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

    private Vector4 _bouncePos;

    private Vector4 _bounceNormal;

    private float _slipAmount;

    private float _contactTime;

    private float _totalTime;

    bool isOob;

    [System.NonSerialized] public MarbleMode mode;

    public MarbleCameraController4D camera;
    [System.NonSerialized] public MarbleWorld4D world;

    Object4D obj4D;

    List<CollisionInfo> contacts = new List<CollisionInfo>();

    // Start is called before the first frame update
    void Start()
    {
        obj4D = this.gameObject.GetComponent<Object4D>();
        lastRenderedPosition = obj4D.worldPosition4D;
        position = lastRenderedPosition;
        orientation = new Rotor4D();
        mode = MarbleMode.Start;
    }

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.position = Transform4D.XYZ(position); // Transform4D.XYZ(lastRenderedPosition) + Transform4D.XYZ(velocity) * Time.deltaTime;
        obj4D.positionW = position.w; // lastRenderedPosition.w + velocity.w * Time.deltaTime;
    }

    public void SetPosition(Vector4 pos)
    {
        position = pos;
        lastRenderedPosition = pos;
        this.gameObject.GetComponent<Object4D>().localPosition4D = pos;
    }

    private void FixedUpdate()
    {
        lastRenderedPosition = position;

        Move mv;
        mv.mv = new Vector3(0, 0, 0);
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
        if (InputManager.GetKey(InputManager.KeyBind.Putt))
            mv.jump = true;
        
        mv.powerup = false;

        AdvancePhysics(mv, Time.fixedDeltaTime);
        position += velocity * Time.fixedDeltaTime;

        var deltaOmega = omega * Time.fixedDeltaTime / 2;

        orientation = orientation - (deltaOmega * orientation);
        orientation.Normalize();

        obj4D.localRotation4D = orientation.ToMatrix();
    }

    void FindContacts()
    {
        Collider4D.Hit hit = Collider4D.Hit.Empty;
        var colliders = world.collisionWorld4D.SphereIntersection(position, _radius);
        foreach (Collider4D collider in colliders)
        {
            if (collider.gameObject == this.gameObject) continue;

            Transform4D localToWorld4D = collider.obj4D.WorldTransform4D();
            Transform4D worldToLocal4D = localToWorld4D.inverse;

            if (collider.Collide(localToWorld4D, worldToLocal4D, position, _radius, ref hit))
            {
                switch (collider.type)
                {
                    case ColliderType.Collideable:
                    {
                        var coll = new CollisionInfo();
                        coll.normal = hit.displacement.normalized;
                        coll.restitution = 1f;
                        coll.friction = 1;
                        coll.velocity = default(Vector4);
                        coll.point = position + hit.displacement;
                        coll.penetration = 0;
                        contacts.Add(coll);
                    }
                    break;

                    case ColliderType.Finish:
                    {
                        world.TouchFinish();
                    }
                    break;
                }
            }
        }
        //        foreach (KeyValuePair<int, ColliderGroup4D> kv in Collider4D.colliders)
        //{
        //    //Cache object transforms for this group
        //    ColliderGroup4D colliderGroup = kv.Value;
        //    Object4D colliderObj = colliderGroup.colliders[0].obj4D;
        //    if (colliderObj == null)
        //    {
        //        LogReport.Error("Collider4D was not removed properly.");
        //        continue;
        //    }
        //    if (!colliderObj.isActiveAndEnabled) { continue; }
        //    Transform4D localToWorld4D = colliderObj.WorldTransform4D();
        //    Transform4D worldToLocal4D = localToWorld4D.inverse;
        //    if (!colliderGroup.IntersectsAABB(localToWorld4D, worldToLocal4D, position,  _radius)) { continue; }
        //    foreach (Collider4D collider in colliderGroup.colliders)
        //    {
        //        if (collider.gameObject == this.gameObject) continue;
        //        if (collider.Collide(localToWorld4D, worldToLocal4D, position, _radius, ref hit))
        //        {
        //            var coll = new CollisionInfo();
        //            coll.normal = hit.displacement.normalized;
        //            coll.restitution = 1f;
        //            coll.friction = 1;
        //            coll.velocity = default(Vector4);
        //            coll.point = position + hit.displacement;
        //            coll.penetration = 0;
        //            contacts.Add(coll);
        //        }
        //    }
        //}
    }

    void AdvancePhysics(Move mv, float dt)
    {
        var remainingTime = dt;
        var it = 0;
        while (remainingTime > 0 && it < 10)
        {
            var timeStep = 0.008f;
            if (timeStep > remainingTime)
                timeStep = remainingTime;

            FindContacts();

            bool isCentered = this._computeMoveForces(mv, omega, out BiVector3 aControl, out BiVector3 desiredOmega);
            this._velocityCancel(contacts, ref velocity, ref omega, isCentered, false);
            Vector4 A = this._getExternalForces(mv, contacts);
            BiVector3 a;
            this._applyContactForces(dt, mv, contacts, isCentered, aControl, desiredOmega, ref velocity, ref omega, ref A, out a);
            velocity += A * dt;
            omega += a * dt;

            if (this.mode == MarbleMode.Start)
            {
                // Bruh...
                this.velocity.z = 0;
                this.velocity.x = 0;
                this.velocity.w = 0;
            }

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

    private bool _computeMoveForces(Move mv, BiVector3 omega, out BiVector3 aControl, out BiVector3 desiredOmega)
    {
        aControl = default(BiVector3);
        desiredOmega = default(BiVector3);
        Vector4 gWorkGravityDir = -currentUp;
        Vector4 R = -gWorkGravityDir * this._radius;
        Vector4 rollVelocity = R * omega; 
        Vector4 sideDir;
        Vector4 motionDir;
        Vector4 upDir;
        Vector4 wDir;
        this._getMarbleAxis(out sideDir, out motionDir, out upDir, out wDir);
        float currentYVelocity = Vector4.Dot(rollVelocity, motionDir);
        float currentXVelocity = Vector4.Dot(rollVelocity, sideDir);
        float currentWVelocity = Vector4.Dot(rollVelocity, wDir);

        float desiredYVelocity = this._maxRollVelocity * mv.mv.y;
        float desiredXVelocity = this._maxRollVelocity * mv.mv.x;
        float desiredWVelocity = this._maxRollVelocity * mv.mv.z;
        if (desiredYVelocity != 0f || desiredXVelocity != 0f || desiredWVelocity != 0f)
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
            desiredOmega = Transform4D.ExteriorProduct(R, desiredYVelocity * motionDir + desiredXVelocity * sideDir + desiredWVelocity * wDir) / R.sqrMagnitude;
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

    private void _applyContactForces(float dt, Move mv, List<CollisionInfo> contacts, bool isCentered, BiVector3 aControl, BiVector3 desiredOmega, ref Vector4 velocity, ref BiVector3 omega, ref Vector4 A, out BiVector3 a)
    {
        a = default(BiVector3);
        this._slipAmount = 0f;
        Vector4 gWorkGravityDir = -currentUp;
        int bestSurface = -1;
        float bestNormalForce = 0f;
        for (int i = 0; i < contacts.Count; i++)
        {
            //if (contacts[i].collider == null)
            //{
                float normalForce = -Vector4.Dot(contacts[i].normal, A);
                if (normalForce > bestNormalForce)
                {
                    bestNormalForce = normalForce;
                    bestSurface = i;
                }
            //}
        }
        CollisionInfo bestContact = (bestSurface != -1) ? contacts[bestSurface] : default(CollisionInfo);
        bool canJump = bestSurface != -1;
        if (canJump && mv.jump)
        {
            Vector4 velDifference = velocity - bestContact.velocity;
            float sv = Vector4.Dot(bestContact.normal, velDifference);
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
            float normalForce2 = -Vector4.Dot(contacts[j].normal, A);
            if (normalForce2 > 0f && Vector4.Dot(contacts[j].normal, velocity - contacts[j].velocity) <= 0.0001f)
            {
                A += contacts[j].normal * normalForce2;
            }
        }
        if (bestSurface != -1 && mode != MarbleMode.Finish)
        {
            // TODO: FIX
            //bestContact.velocity - bestContact.normal * Vector3.Dot(bestContact.normal, bestContact.velocity);
            Vector4 vAtC = velocity + ((-bestContact.normal * this._radius) * omega) - bestContact.velocity;
            float vAtCMag = vAtC.magnitude;
            bool slipping = false;
            BiVector3 aFriction = new BiVector3(0f, 0f, 0f, 0f, 0f, 0f);
            Vector4 AFriction = new Vector4(0f, 0f, 0f, 0f);
            if (vAtCMag != 0f)
            {
                slipping = true;
                float friction = 0.0f;
                if (this.mode != MarbleMode.Start)
                    friction = this._kineticFriction * bestContact.friction;
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
                Vector4 vAtCDir = vAtC / vAtCMag;

                aFriction = Transform4D.ExteriorProduct(-bestContact.normal, -vAtCDir) * angAMagnitude;
                AFriction = -AMagnitude * vAtCDir;
                this._slipAmount = vAtCMag - totalDeltaV;
            }
            if (!slipping)
            {
                Vector4 R = -gWorkGravityDir * this._radius;
                BiVector3 aadd = Transform4D.ExteriorProduct(R, A) / R.sqrMagnitude;
                if (isCentered)
                {
                    BiVector3 nextOmega = omega + a * dt;
                    aControl = desiredOmega - nextOmega;
                    float aScalar = aControl.magnitude;
                    if (aScalar > this._brakingAcceleration)
                    {
                        aControl *= this._brakingAcceleration / aScalar;
                    }
                }
                Vector4 Aadd = (-bestContact.normal * this._radius) * aControl;

                float aAtCMag = (((-bestContact.normal * this._radius) * aadd) + Aadd).magnitude;
                var friction2 = 0.0f;
                if (mode != MarbleMode.Start)
                    friction2 = this._staticFriction * bestContact.friction;
                if (aAtCMag > friction2 * bestNormalForce)
                {
                    friction2 = 0.0f;
                    if (mode != MarbleMode.Start)
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
        if (this.mode == MarbleMode.Finish)
        {
            a.xy = 0;
            a.xz = 0;
            a.xw = 0;
            a.yz = 0;
            a.yw = 0;
            a.zw = 0;
        }
    }

    private Vector4 _getExternalForces(Move mv, List<CollisionInfo> contacts)
    {
        if (this.mode == MarbleMode.Finish)
            return this.velocity * -16.0f;

        Vector4 gWorkGravityDir = -currentUp;
        Vector4 A = gWorkGravityDir * this._gravity;
        if (contacts.Count == 0 && mode != MarbleMode.Start)
        {
            Vector4 sideDir;
            Vector4 motionDir;
            Vector4 upDir;
            Vector4 wDir;
            this._getMarbleAxis(out sideDir, out motionDir, out upDir, out wDir);
            A += (sideDir * mv.mv.x + motionDir * mv.mv.y + wDir * mv.mv.z) * this._airAccel;
        }
        return A;
    }

    private void _velocityCancel(List<CollisionInfo> contacts, ref Vector4 velocity, ref BiVector3 omega, bool surfaceSlide, bool noBounce)
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
                Vector4 sVel = velocity - contacts[i].velocity;
                float surfaceDot = Vector4.Dot(contacts[i].normal, sVel);
                if ((!looped && surfaceDot < 0f) || surfaceDot < -SurfaceDotThreshold)
                {
                    float velLen = velocity.magnitude;
                    Vector4 surfaceVel = surfaceDot * contacts[i].normal;
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
                        Vector4 velocity2 = contacts[i].velocity;
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
                            Vector4 velocityAdd = -(1f + restitution) * surfaceVel;
                            Vector4 vAtC = sVel + (-contacts[i].normal * this._radius) * omega;
                            float normalVel = -Vector4.Dot(contacts[i].normal, sVel);
                            vAtC -= contacts[i].normal * Vector4.Dot(contacts[i].normal, sVel);
                            float vAtCMag = vAtC.magnitude;
                            if (vAtCMag != 0f)
                            {
                                float friction = this._bounceKineticFriction * contacts[i].friction;
                                float angVMagnitude = friction * normalVel / (radiusOfGyration * this._radius);
                                if (angVMagnitude > vAtCMag / this._radius)
                                {
                                    angVMagnitude = vAtCMag / this._radius;
                                }
                                Vector4 vAtCDir = vAtC / vAtCMag;
                                BiVector3 deltaOmega = Transform4D.ExteriorProduct(contacts[i].normal, vAtCDir);

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
        if (velocity.sqrMagnitude < 625f)
        {
            bool gotOne = false;
            Vector4 dir = new Vector4(0f, 0f, 0f, 0f);
            for (int j = 0; j < contacts.Count; j++)
            {
                Vector4 dir2 = dir + contacts[j].normal;
                if (dir2.sqrMagnitude < 0.01f)
                {
                    dir2 += contacts[j].normal;
                }
                dir = dir2;
                gotOne = true;
            }
            if (gotOne)
            {
                dir.Normalize();
                float soFar = 0f;
                for (int k = 0; k < contacts.Count; k++)
                {
                    if (contacts[k].penetration < this._radius)
                    {
                        float timeToSeparate = 0.1f;
                        float dist = contacts[k].penetration;
                        float outVel = Vector4.Dot(velocity + soFar * dir, contacts[k].normal);
                        if (timeToSeparate * outVel < dist)
                        {
                            soFar += (dist - outVel * timeToSeparate) / timeToSeparate / Vector4.Dot(contacts[k].normal, dir);
                        }
                    }
                }
                soFar = Mathf.Clamp(soFar, -25f, 25f);
                velocity += soFar * dir;
            }
        }
    }

    private void _getMarbleAxis(out Vector4 sideDir, out Vector4 motionDir, out Vector4 upDir, out Vector4 wDir)
    {
        Vector4 gWorkGravityDir = -currentUp;
        upDir = -gWorkGravityDir;

        sideDir = new Vector4(1, 0, 0, 0);
        motionDir = new Vector4(0, 0, 1, 0);
        wDir = new Vector4(0, 0, 0, 1);

        sideDir = camera.camMatrix * sideDir;
        motionDir = camera.camMatrix * motionDir;
        if (camera.volumeMode)
        {
            upDir = camera.camMatrix * wDir;
            wDir = camera.camMatrix * upDir;
        }
        else
        {
            upDir = camera.camMatrix * upDir;
            wDir = camera.camMatrix * wDir;
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

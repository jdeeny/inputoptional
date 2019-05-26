using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PlayerCommand
{
    Idle,
    GetOpen,
    CoverReceiver,
    GetBall,
    RunToGoal,
    Protect,
    Pass,
    Hit,
    Intercept,
}

public class PlayerAI : MonoBehaviour
{

    public float thrust;
    public float turn_speed;
    public float reaction_base;
    public float reaction_random;
    float nearRadius = 5f;
    float visionRadius = 30f;

    float reaction_remaining = 0;

    int layer = 9;

    //Placeholders
    public string playerName;

    PlayerCommand current_command = PlayerCommand.Idle;
    int team = 0;

    int n = 0;

    CapsuleCollider col;
    Rigidbody rb;
    Animator animator;
    
    public Transform hand;

    // constants:
    const float JumpPower = 8f;		// determines the jump force applied when jumping (and therefore the jump height)
    const float GroundSpeed = 12f;
    const float AirSpeed = 1f;		// determines the max speed of the character while airborne
    const float AirControl = 0.1f;	// determines the response speed of controlling the character while airborne
    const float StationaryTurnSpeed = 180f;	// additional turn speed added when the player is stationary (added to animation root rotation)
    const float MovingTurnSpeed = 360f;		// additional turn speed added when the player is moving (added to animation root rotation)
    const float RunCycleLegOffset = 0.2f;	// animation cycle offset (0-1) used for determining correct leg to jump off

    [SerializeField]
    string animationationGetUpFromBelly = "GetUp.GetUpFromBelly";
    [SerializeField]
    string animationationGetUpFromBack = "GetUp.GetUpFromBack";
	float _ragdollingEndTime;   //A helper variable to store the time when we transitioned from ragdolled to blendToAnim state

    RagdollState ragdollState = RagdollState.Animated;
    const float RagdollToMecanimBlendTime = 0.5f;   //How long do we blend when transitioning from ragdolled to animated
    readonly List<RigidComponent> _rigids = new List<RigidComponent>();
    readonly List<TransformComponent> _transforms = new List<TransformComponent>();
    Transform _hipsTransform;
    Rigidbody _hipsTransformRigid;
    Vector3 _storedHipsPosition;
    Vector3 _storedHipsPositionPrivAnim;
    Vector3 _storedHipsPositionPrivBlend;
	bool _groundChecker;
	float _jumpStartedTime;
    bool _throwing = false;
    Vector3 _throwLocation;
    
    public Vector3 CharacterVelocity { get { return _onGround ? rb.velocity : _airVelocity; } }
    // Animator parameters
    readonly int animatorForward = Animator.StringToHash("Forward");
    readonly int animatorTurn = Animator.StringToHash("Turn");
    readonly int animatorCrouch = Animator.StringToHash("Crouch");
    readonly int animatorOnGround = Animator.StringToHash("OnGround");
    readonly int animatorThrowing = Animator.StringToHash("Throwing");
    readonly int animatorJump = Animator.StringToHash("Jump");
    readonly int animatorJumpLeg = Animator.StringToHash("JumpLeg");
    readonly int animatorCapsuleY = Animator.StringToHash("CapsuleY");
    // Animator Animations
	readonly int animatorGrounded = Animator.StringToHash("Base Layer.Grounded.Grounded");

    RoboSounds roboSounds;

    // parameters needed to control character
    bool _onGround; // Is the character on the ground
    Vector3 _moveInput;
    bool _crouch;
    bool _jump;
    float _turnAmount;
    float _forwardAmount;
    bool _enabled = true;
    protected Vector3 _airVelocity;
    protected bool _jumpPressed = false;
    protected bool _firstAnimatorFrame = true;  // needed for prevent changing position in first animation frame

    float onGroundSince = 0f;
    Dictionary<string, HashSet<Collider>> visionSets;

    void Start()
    {
        playerName = NameGenerator.GenerateRobotName();
        roboSounds = GetComponent<RoboSounds>();
        col = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        hand = gameObject.transform; //default incase we can't find the hand
        var limbs = gameObject.GetComponentsInChildren<Transform>();
        foreach(var l in limbs) {
            if (l.name == "mixamorig:Spine1") {
                hand = l;
            }
        }
        gameObject.layer = LayerMask.NameToLayer("Player");


		Rigidbody[] rigidBodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigid in rigidBodies)
        {
            if (rigid.transform == transform)
                continue;

            RigidComponent rigidCompontnt = new RigidComponent(rigid);
            _rigids.Add(rigidCompontnt);
        }

        _hipsTransform = animator.GetBoneTransform(HumanBodyBones.Hips);
        _hipsTransformRigid = _hipsTransform.GetComponent<Rigidbody>();

        ActivateRagdollParts(false);

        //Find all the transforms in the character, assuming that this script is attached to the root
        //For each of the transforms, create a BodyPart instance and store the transform
        foreach (var t in GetComponentsInChildren<Transform>())
        {
            var trComp = new TransformComponent(t);
            _transforms.Add(trComp);
        }

    }


    void FixedUpdate()
    {
        _moveInput = Vector3.zero;
        if(_hipsTransformRigid == null) return;
        if (ragdollState == RagdollState.WaitStablePosition &&
            _hipsTransformRigid.velocity.magnitude < 0.1f)
        {
            GetUp();
        }

        if (ragdollState == RagdollState.Animated &&
            CharacterVelocity.y < -10f)
        {
            // kill and resuscitate will force character to enter in Ragdoll 
            RagdollIn();
            RagdollOut();
        }

        if (IsRagdolled) {
            onGroundSince += Time.fixedDeltaTime;
            if(Random.Range(-2f, onGroundSince) >= transform.position.y) {
                RagdollOut();
            }
        } else {
            onGroundSince = 0f;
        }

        visionSets = UpdatePlayerVision();

/*         reaction_remaining -= Time.deltaTime;
        if(reaction_remaining > 0f)
            return;*/

        //_moveInput = Vector3.zero;

        switch (current_command)
        {
            default:
                StopMoving();
                break;
            case PlayerCommand.GetBall:
                RunToBall();
                break;
            case PlayerCommand.RunToGoal:
                RunToGoal();
                break;
            case PlayerCommand.GetOpen:
                RunToOpenArea();
                break;
            case PlayerCommand.Protect:
                StopMoving();
                //RunTo(GetBallCarrierLocation());
                break;
            case PlayerCommand.Pass:
                //StopMoving();
                PassTo(new Vector3(0f, 0f, 0f));
                break;
        }

        if (!_enabled)
            return;

        _onGround = !_jumpPressed && PlayerTouchGound();
        int currentAnimation = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;

        ApplyCapsuleHeight();
        ApplyExtraTurnRotation(currentAnimation);		// this is in addition to root rotation in the animations
        ConvertMoveInput();				// converts the relative move vector into local turn & fwd values

        // control and velocity handling is different when grounded and airborne:
        if (_onGround)
            HandleGroundedVelocities(currentAnimation);
        else
            HandleAirborneVelocities();

		UpdateAnimator(); // send input and other state parameters to the animator
    }


    void LateUpdate()
    {
        if (ragdollState != RagdollState.BlendToAnim)
            return;

        float ragdollBlendAmount = 1f - Mathf.InverseLerp(
            _ragdollingEndTime,
            _ragdollingEndTime + RagdollToMecanimBlendTime,
            Time.time);

        // In LateUpdate(), Mecanim has already updated the body pose according to the animations.
        // To enable smooth transitioning from a ragdoll to animation, we lerp the position of the hips
        // and slerp all the rotations towards the ones stored when ending the ragdolling

        if (_storedHipsPositionPrivBlend != _hipsTransform.position)
        {
            _storedHipsPositionPrivAnim = _hipsTransform.position;
        }
        _storedHipsPositionPrivBlend = Vector3.Lerp(_storedHipsPositionPrivAnim, _storedHipsPosition, ragdollBlendAmount);
        _hipsTransform.position = _storedHipsPositionPrivBlend;

        foreach (TransformComponent trComp in _transforms)
        {
            //rotation is interpolated for all body parts
            if (trComp.PrivRotation != trComp.Transform.localRotation)
            {
                trComp.PrivRotation = Quaternion.Slerp(trComp.Transform.localRotation, trComp.StoredRotation, ragdollBlendAmount);
                trComp.Transform.localRotation = trComp.PrivRotation;
            }

            //position is interpolated for all body parts
            if (trComp.PrivPosition != trComp.Transform.localPosition)
            {
                trComp.PrivPosition = Vector3.Slerp(trComp.Transform.localPosition, trComp.StoredPosition, ragdollBlendAmount);
                trComp.Transform.localPosition = trComp.PrivPosition;
            }
        }

        //if the ragdoll blend amount has decreased to zero, move to animated state
        if (Mathf.Abs(ragdollBlendAmount) < Mathf.Epsilon)
        {
            ragdollState = RagdollState.Animated;
        }
    }


    void UpdateAnimator()
    {
        // Here we tell the animator what to do based on the current states and inputs.

        // update the animator parameters
        animator.SetFloat(animatorForward, _forwardAmount, 0.1f, Time.deltaTime);
        animator.SetFloat(animatorTurn, _turnAmount, 0.1f, Time.deltaTime);
        animator.SetBool(animatorOnGround, _onGround);
        animator.SetBool(animatorThrowing, _throwing);
        if (!_onGround)	// if flying
        {
            animator.SetFloat(animatorJump, CharacterVelocity.y);
        }
        else
        {
            // calculate which leg is behind, so as to leave that leg trailing in the jump animation
            // (This code is reliant on the specific run cycle offset in our animations,
            // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
            float runCycle = Mathf.Repeat(
                    animator.GetCurrentAnimatorStateInfo(0).normalizedTime + RunCycleLegOffset, 1);

            float jumpLeg = (runCycle < 0.5f ? 1 : -1) * _forwardAmount;
            animator.SetFloat(animatorJumpLeg, jumpLeg);
        }
    }

    void RunTo(Vector3 location)
    {
        //if (_onGround)
        //{
            //transform.LookAt(location);
            //Vector3 dir = transform.forward.normalized;
            //dir.y = 0;

        _moveInput = location - transform.position; //transform.TransformDirection(transform.position - location).normalized;
        //Debug.Log("loc: " + location + " pos: " + transform.position);
        _moveInput.y = 0f;
//        Debug.Log("Move input: " + _moveInput);

//            _moveInput = transform.TransformDirection(location - transform.position);
            //rb.AddForce(dir * thrust, ForceMode.Impulse);
        //}
    }
    void RunAwayFrom(Vector3 location)
    {
        _moveInput = location - transform.position;
        _moveInput.y = 0f;
        //_moveInput = Quaternion.Euler(0, 180, 0) * _moveInput;
        //_moveInput = new Vector3(1f,0f,1f);//transform.TransformVector(transform.position - location);

/*        if (_onGround)
        {

            transform.LookAt(location);
            transform.Rotate(new Vector3(0f, 180f, 0f));
            Vector3 dir = transform.forward.normalized;
            dir.y = 0;
            _moveInput = dir;
            //rb.AddForce(dir * thrust, ForceMode.Impulse);
        }*/
    }


    void HandleSpotCollision(Collision col)
    {
        if (col.gameObject.name.StartsWith("Spot"))
        {
            if(GameManager.Instance.GetBallPlayer() == gameObject) {
                //Debug.Log("Hit Spot and have ball");

                GameManager.Instance.ball.GetComponent<BallBehavior>().Detach();
                GameManager.Instance.Score(GameManager.Instance.GetBallOwner());

                foreach (Team t in GameManager.Instance.teams)
                {
                    foreach (GameObject player in t.players)
                    {
                        if (player != null)
                        {
                            player.GetComponent<PlayerAI>().ExplodeOnGoal();
                        }
                    }

                }
                GameManager.Instance.teams[team - 1].RemovePlayer(gameObject);
                Destroy(gameObject,0.1f);
            }
        }
    }


    void HandlePlayerCollision(Collision col)
    {
        if (col.gameObject.tag == "Player")
        {
            //Debug.Log("Collide: " + col.gameObject.name);
            var theirVel = col.gameObject.GetComponent<Rigidbody>().velocity;
            var velDiff = (rb.velocity - theirVel).magnitude;
            if(rb.velocity.magnitude < theirVel.magnitude) velDiff *= -1f;
            //Debug.Log("velDiff: " + velDiff);
            if(Random.Range(-10f, 10f) > velDiff) {
                roboSounds.PlayCrash();
                rb.velocity = theirVel * Random.Range(1f, 5f);
                RagdollIn();
            }
        }
    }

    public void ExplodeOnGoal()
    {
        RagdollIn();
        var explode_dist = Mathf.Max(1f, 200f - rb.position.magnitude);
        var explode_vel = explode_dist * Random.Range(.04f, .1f) * rb.position.normalized;
        explode_vel.y += explode_dist * Random.Range(0.01f, .1f);
        ApplyVelocity(explode_vel);
    }

    void StopMoving()
    {
        if (_onGround)
        {
            if (rb.velocity.magnitude >= 0.001f)
            {
                //body.velocity = Vector3.zero;
                //body.AddForce(body.velocity.normalized * -0.1f * thrust, ForceMode.Impulse);
            }
        }
    }


    float TimeToPosition(Vector3 loc)
    {
        return (transform.position - loc).magnitude / GroundSpeed;
    }

    Vector3 FindBallIntercept() {
        float t = 0f;
        Vector3 p = transform.position;
        foreach(var loc in GameManager.Instance.ballPositions) {
            p = loc;
            if(TimeToPosition(loc) <= t) {
                if (loc.y <= 9f) {
                    //Debug.Log(gameObject.name + " found intercept: " + kv.Value + " " + kv.Key);
                    return loc;
                } else {
                    //Debug.Log(gameObject.name + " REJECT intercept: " + kv.Value + " " + kv.Key);
                }
            }
            t += Time.fixedDeltaTime * 2;
        }
        return p;
    }    

    void RunToBall()
    {
        var intercept = FindBallIntercept();
        
        var diff = transform.position - intercept;
        diff.y = 0f;
        if(intercept.y > col.height * .8 & diff.magnitude < intercept.y / 8f) {
            _jump = true;
        }
        RunTo(intercept);//Vector3.zero);//GameManager.Instance.ballLandingPosition);
    }

    void RunToGoal()
    {
        RunTo(GameManager.Instance.spot.transform.position);
    }


    public void ReadyKickoff() {
        RagdollOut();
    }

    void RunToOpenArea()
    {
        Vector3 target = Vector3.zero;

        int n = 0;
        foreach(var c in visionSets["vision"])
        {
            target += c.transform.position;
            n++;
        }
        if(n > 0) {
            target /= n;
        } else {
            target = transform.position;
        }

        
        RunTo(target);
    }

    public void SetCommand(PlayerCommand command)
    {
        current_command = command;
        reaction_remaining = reaction_base + Random.Range(0f, 1f) * reaction_random;
    }

    public void SetTeam(int team_num)
    {
        team = team_num;
    }

    public int GetTeam()
    {
        return team;
    }

    public GameObject FindNearestEnemy() {
        GameObject closest = null;
        float distance = 10000000f;
        foreach(Team t in GameManager.Instance.teams) {
            if(t.teamNumber != team) {
                foreach(GameObject player in t.players) {
                    float d = Vector3.Distance(player.transform.position, transform.position);
                    if(d >= 1 && d < distance) {
                        distance = d;
                        closest = player;
                    }
                }
            }
        }
        return closest;
    }

    public GameObject FindNearestPlayer()
    {
        GameObject closest = null;
        float distance = 10000000f;
        foreach (Team t in GameManager.Instance.teams)
        {
            foreach (GameObject player in t.players)
            {
                if (player != this)
                {
                    float d = Vector3.Distance(player.transform.position, transform.position);
                    if (d >= 1 && d < distance)
                    {
                        distance = d;
                        closest = player;
                    }
                }
            }
            
        }
        return closest;
    }




    Vector3 GetBallCarrierLocation()
    {
        return GameManager.Instance.GetBallPlayer().transform.position;
    }


    void PassTo(Vector3 location)
    {
        _throwing = true;
        _throwLocation = location;
    }

    // Animation event so throw is timed with arm
    void DoThrow()
    {
        Debug.Log("Throw callback");
        _throwing = false;
        GameManager.Instance.ball.GetComponent<BallBehavior>().ThrowTo(_throwLocation);
    }


    private void ActivateRagdollParts(bool activate)
    {
        CharacterEnable(!activate);

        //_hipsTransform.GetComponentInChildren<Collider>()
        foreach (var rigid in _rigids)
        {
            Collider partColider = rigid.RigidBody.GetComponent<Collider>();

            // fix for RagdollHelper (bone collider - BoneHelper.cs)
            if (partColider == null)
            {
                const string colliderNodeSufix = "_ColliderRotator";
                string childName = rigid.RigidBody.name + colliderNodeSufix;
                Transform transform = rigid.RigidBody.transform.Find(childName);
                partColider = transform.GetComponent<Collider>();
            }

            partColider.isTrigger = !activate;

            if (activate)
            {
                rigid.RigidBody.isKinematic = false;
                StartCoroutine(FixTransformAndEnableJoint(rigid));
            }
            else
                rigid.RigidBody.isKinematic = true;
        }

        //if (activate)
        //	foreach (var joint in GetComponentsInChildren<CharacterJoint>())
        //	{
        //		var jointTransform = joint.transform;
        //		var pivot = joint.connectedBody.transform;
        //		jointTransform.position = pivot.position;
        //		jointTransform.Translate(joint.connectedAnchor, pivot);
        //	}
    }

    public bool Raycast(Ray ray, out RaycastHit hit, float distance)
    {
        var hits = Physics.RaycastAll(ray, distance);
        
        for (int i = 0; i < hits.Length; ++i)
        {
            var h = hits[i];
            if (h.transform != transform && h.transform.root == transform.root)
            {
                hit = h;
                return true;
            }
        }

        // if no hits found, return false
        hit = new RaycastHit();
        return false;
    }


    public bool IsRagdolled
    {
        get
        {
            return
                ragdollState == RagdollState.Ragdolled ||
                ragdollState == RagdollState.WaitStablePosition;
        }
        set
        {
            if (value)
                RagdollIn();
            else
                RagdollOut();
        }
    }


    public void AddExtraMove(Vector3 move)
    {
        if (IsRagdolled)
        {
            Vector3 airMove = new Vector3(move.x * AirSpeed, 0f, move.z * AirSpeed);
            foreach (var rigid in _rigids)
                rigid.RigidBody.AddForce(airMove / 100f, ForceMode.VelocityChange);
        }
    }

/// <summary>
		/// Prevents jittering (as a result of applying joint limits) of bone and smoothly translate rigid from animated mode to ragdoll
		/// </summary>
		/// <param name="rigid"></param>
		/// <returns></returns>
		static IEnumerator FixTransformAndEnableJoint(RigidComponent joint)
		{
			if (joint.Joint == null || !joint.Joint.autoConfigureConnectedAnchor)
				yield break;

			SoftJointLimit highTwistLimit = new SoftJointLimit();
			SoftJointLimit lowTwistLimit = new SoftJointLimit();
			SoftJointLimit swing1Limit = new SoftJointLimit();
			SoftJointLimit swing2Limit = new SoftJointLimit();

			SoftJointLimit curHighTwistLimit = highTwistLimit = joint.Joint.highTwistLimit;
			SoftJointLimit curLowTwistLimit = lowTwistLimit = joint.Joint.lowTwistLimit;
			SoftJointLimit curSwing1Limit = swing1Limit = joint.Joint.swing1Limit;
			SoftJointLimit curSwing2Limit = swing2Limit = joint.Joint.swing2Limit;
			
			float aTime = 0.3f;
			Vector3 startConPosition = joint.Joint.connectedBody.transform.InverseTransformVector(joint.Joint.transform.position - joint.Joint.connectedBody.transform.position);

			joint.Joint.autoConfigureConnectedAnchor = false;
			for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
			{
				Vector3 newConPosition = Vector3.Lerp(startConPosition, joint.ConnectedAnchorDefault, t);
				joint.Joint.connectedAnchor = newConPosition;
					
				curHighTwistLimit.limit = Mathf.Lerp(177, highTwistLimit.limit, t);
				curLowTwistLimit.limit = Mathf.Lerp(-177, lowTwistLimit.limit, t);
				curSwing1Limit.limit = Mathf.Lerp(177, swing1Limit.limit, t);
				curSwing2Limit.limit = Mathf.Lerp(177, swing2Limit.limit, t);

				joint.Joint.highTwistLimit = curHighTwistLimit;
				joint.Joint.lowTwistLimit = curLowTwistLimit;
				joint.Joint.swing1Limit = curSwing1Limit;
				joint.Joint.swing2Limit = curSwing2Limit;

						
				yield return null;
			}
			joint.Joint.connectedAnchor = joint.ConnectedAnchorDefault;
			yield return new WaitForFixedUpdate();
			joint.Joint.autoConfigureConnectedAnchor = true;


			joint.Joint.highTwistLimit = highTwistLimit;
			joint.Joint.lowTwistLimit = lowTwistLimit;
			joint.Joint.swing1Limit = swing1Limit;
			joint.Joint.swing2Limit = swing2Limit;
		}

		/// <summary>
		/// Ragdoll character
		/// </summary>
		void RagdollIn()
		{
			//Transition from animated to ragdolled

			ActivateRagdollParts(true);     // allow the ragdoll RigidBodies to react to the environment
			animator.enabled = false;      // disable animation
			ragdollState = RagdollState.Ragdolled;
			ApplyVelocity(CharacterVelocity);
            GameManager.Instance.ball.GetComponent<BallBehavior>().Detach();
		}

		/// <summary>
		/// Smoothly translate to animator's bone positions when character stops falling
		/// </summary>
		void RagdollOut()
		{
			if (ragdollState == RagdollState.Ragdolled)
				ragdollState = RagdollState.WaitStablePosition;
		}

		private void GetUp()
		{
			//Transition from ragdolled to animated through the blendToAnim state
			_ragdollingEndTime = Time.time;     //store the state change time
			//animator.SetFloat(animationatorForward, 0f);
			//animator.SetFloat(animationatorTurn, 0f);
			animator.enabled = true;               //enable animation
			ragdollState = RagdollState.BlendToAnim;
			_storedHipsPositionPrivAnim = Vector3.zero;
			_storedHipsPositionPrivBlend = Vector3.zero;

			_storedHipsPosition = _hipsTransform.position;

			// get distanse to floor
			Vector3 shiftPos = _hipsTransform.position - transform.position;
			shiftPos.y = GetDistanceToFloor(shiftPos.y);

			// shift and rotate character node without children
			MoveNodeWithoutChildren(shiftPos);

			//Store the ragdolled position for blending
			foreach (TransformComponent trComp in _transforms)
			{
				trComp.StoredRotation = trComp.Transform.localRotation;
				trComp.PrivRotation = trComp.Transform.localRotation;

				trComp.StoredPosition = trComp.Transform.localPosition;
				trComp.PrivPosition = trComp.Transform.localPosition;
			}

			//Initiate the get up animation
			string getUpAnim = CheckIfLieOnBack() ? animationationGetUpFromBack : animationationGetUpFromBelly;
			animator.Play(getUpAnim, 0, 0);	// you have to set time to 0, or if your animation will interrupt, next time animation starts from previous position
			ActivateRagdollParts(false);    // disable gravity on ragdollParts.
		}

		private float GetDistanceToFloor(float currentY)
		{
			RaycastHit[] hits = Physics.RaycastAll(new Ray(_hipsTransform.position, Vector3.down));
			float distFromFloor = float.MinValue;

			foreach (RaycastHit hit in hits)
				if (!hit.transform.IsChildOf(transform))
					distFromFloor = Mathf.Max(distFromFloor, hit.point.y);

			if (Mathf.Abs(distFromFloor - float.MinValue) > Mathf.Epsilon)
				currentY = distFromFloor - transform.position.y;

			return currentY;
		}

		private void MoveNodeWithoutChildren(Vector3 shiftPos)
		{
			Vector3 ragdollDirection = GetRagdollDirection();

			// shift character node position without children
			_hipsTransform.position -= shiftPos;
			transform.position += shiftPos;

			// rotate character node without children
			Vector3 forward = transform.forward;
			transform.rotation = Quaternion.FromToRotation(forward, ragdollDirection) * transform.rotation;
			_hipsTransform.rotation = Quaternion.FromToRotation(ragdollDirection, forward) * _hipsTransform.rotation;
		}

		private bool CheckIfLieOnBack()
		{
			var left = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position;
			var right = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).position;
			var hipsPos = _hipsTransform.position;

			left -= hipsPos;
			left.y = 0f;
			right -= hipsPos;
			right.y = 0f;

			var q = Quaternion.FromToRotation(left, Vector3.right);
			var t = q * right;

			return t.z < 0f;
		}

		private Vector3 GetRagdollDirection()
		{
			Vector3 ragdolledFeetPosition = (
				animator.GetBoneTransform(HumanBodyBones.Hips).position);// +
																	  //animator.GetBoneTransform(HumanBodyBones.RightToes).position) * 0.5f;
			Vector3 ragdolledHeadPosition = animator.GetBoneTransform(HumanBodyBones.Head).position;
			Vector3 ragdollDirection = ragdolledFeetPosition - ragdolledHeadPosition;
			ragdollDirection.y = 0;
			ragdollDirection = ragdollDirection.normalized;

			if (CheckIfLieOnBack())
				return ragdollDirection;
			else
				return -ragdollDirection;
		}

		/// <summary>
		/// Apply velocity 'predieVelocity' to to each rigid of character
		/// </summary>
		private void ApplyVelocity(Vector3 predieVelocity)
		{
			foreach (var rigid in _rigids)
				rigid.RigidBody.velocity = predieVelocity;
		}


		//Declare a class that will hold useful information for each body part
		sealed class TransformComponent
		{
			public readonly Transform Transform;
			public Quaternion PrivRotation;
			public Quaternion StoredRotation;

			public Vector3 PrivPosition;
			public Vector3 StoredPosition;

			public TransformComponent(Transform t)
			{
				Transform = t;
			}
		}

		struct RigidComponent
		{
			public readonly Rigidbody RigidBody;
			public readonly CharacterJoint Joint;
			public readonly Vector3 ConnectedAnchorDefault;

			public RigidComponent(Rigidbody rigid)
			{
				RigidBody = rigid;
				Joint = rigid.GetComponent<CharacterJoint>();
				if (Joint != null)
					ConnectedAnchorDefault = Joint.connectedAnchor;
				else
					ConnectedAnchorDefault = Vector3.zero;
			}
		}


    void CharacterEnable(bool enable) {
        //Debug.Log("CharacterEnable!");
        _enabled = enable;

	    col.enabled = enable;
		rb.isKinematic = !enable;
		if (enable)
			_firstAnimatorFrame = true;

    }
    void OnAnimatorMove()
    {
        //if (Time.deltaTime < Mathf.Epsilon)
            //return;

        Vector3 deltaPos;
        Vector3 deltaGravity = Physics.gravity * Time.deltaTime;
        _airVelocity += deltaGravity;

        if (_onGround)
        {
            deltaPos = animator.deltaPosition;
            deltaPos.y -= 5f * Time.deltaTime;
        }
        else
        {
            deltaPos = _airVelocity * Time.deltaTime;
        }

        if (_firstAnimatorFrame)
        {
            // if Animator just started, Animator move character
            // so you need to zeroing movement
            deltaPos = new Vector3(0f, deltaPos.y, 0f);
            _firstAnimatorFrame = false;
        }

        UpdatePlayerPosition(deltaPos);

        // apply animator rotation
        transform.rotation *= animator.deltaRotation;
        _jumpPressed = false;
    }


    void ProccessOnCollisionOccured(Collision collision)
    {
        // if collision comes from botton, that means
        // that character on the ground
        if(col == null) {
            col = gameObject.GetComponent<CapsuleCollider>();
        }

        float charBottom =
            transform.position.y +
            col.center.y - col.height / 2 +
            col.radius * 0.8f;
        
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.point.y < charBottom && !contact.otherCollider.transform.IsChildOf(transform))
            {
                _groundChecker = true;
                //Debug.DrawRay(contact.point, contact.normal, Color.blue);
//                roboSounds.PlayCrash();
                break;
            }
        }

        HandleSpotCollision(collision);
        HandlePlayerCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        ProccessOnCollisionOccured(collision);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        ProccessOnCollisionOccured(collision);
    }


    bool PlayerTouchGound()
    {
        bool grounded = _groundChecker;
        _groundChecker = false;
        // if the character is on the ground and
        // half of second was passed, return true
        return grounded & (_jumpStartedTime + 0.5f < Time.time );
    }
    void UpdatePlayerPosition(Vector3 deltaPos)
    {
        if(Time.deltaTime <= Mathf.Epsilon) return;
        
        Vector3 finalVelocity = deltaPos / Time.deltaTime;
        if (!_jumpPressed)
        {
            finalVelocity.y = rb.velocity.y;
        }
        else
        {
            _jumpStartedTime = Time.time;
        }
        if (System.Single.IsNaN(finalVelocity.x)) finalVelocity.x = 0f;
        if (System.Single.IsNaN(finalVelocity.y)) finalVelocity.y = 0f;
        if (System.Single.IsNaN(finalVelocity.z)) finalVelocity.z = 0f;

        _airVelocity = finalVelocity;		// i need this to correctly detect player velocity in air mode
        rb.velocity = finalVelocity;
        //Debug.Log("vel: " + rb.velocity);
    }
    
    void ApplyExtraTurnRotation(int currentAnimation)
    {
        if (currentAnimation != animatorGrounded || Time.deltaTime > Mathf.Epsilon)
            return;

        // help the character turn faster (this is in addition to root rotation in the animation)
        float turnSpeed = Mathf.Lerp(StationaryTurnSpeed, MovingTurnSpeed,
                                        _forwardAmount);

        transform.Rotate(0, _turnAmount * turnSpeed * Time.deltaTime, 0);
    }
    
    private void ConvertMoveInput()
    {
        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction. 

        //_moveInput = new Vector3(1f, 0, 1f);

        Vector3 localMoveInput = _moveInput * 10f;
        localMoveInput = localMoveInput.magnitude > GroundSpeed ? localMoveInput.normalized * GroundSpeed : localMoveInput;

        Vector3 localMove = transform.InverseTransformDirection(localMoveInput);

        if ((Mathf.Abs(localMove.x) > float.Epsilon) &
            (Mathf.Abs(localMove.z) > float.Epsilon))
            _turnAmount = Mathf.Atan2(localMove.x, localMove.z);
        else
            _turnAmount = 0f;

        _forwardAmount = localMove.z;
        //Debug.Log(gameObject.name + " fwd: " + _forwardAmount + " turn: " + _turnAmount + " move: " + _moveInput + " local: " + localMove);
    }

    void HandleGroundedVelocities(int currentAnimation)
    {
        bool animationGrounded = currentAnimation == animatorGrounded;

        // check whether conditions are right to allow a jump
        if (!(_jump & !_crouch & animationGrounded))
            return;

        // jump!
        Debug.Log("JUMP!");
        Vector3 newVelocity = Vector3.zero;//CharacterVelocity / 10f;
        newVelocity.y += JumpPower;
        _airVelocity = newVelocity;

        _jump = false;
        _onGround = false;
        _jumpPressed = true;
    }
    void HandleAirborneVelocities()
    {
        // we allow some movement in air, but it's very different to when on ground
        // (typically allowing a small change in trajectory)
        Vector3 airMove = new Vector3(_moveInput.x * AirSpeed, _airVelocity.y, _moveInput.z * AirSpeed);
        _airVelocity = Vector3.Lerp(_airVelocity, airMove, Time.deltaTime * AirControl);
    }

    
    
    
    void ApplyCapsuleHeight()
    {
        float capsuleY = animator.GetFloat(animatorCapsuleY);
        col.height = capsuleY;
        var c = col.center;
        c.y = capsuleY / 2f;
        col.center = c;
    }
    
    
    
    
    
    
    
    
    //Possible states of the ragdoll
    enum RagdollState
    {
        /// <summary>
        /// Mecanim is fully in control
        /// </summary>
        Animated,
        /// <summary>
        /// Mecanim turned off, but when stable position will be found, the transition to Animated will heppend
        /// </summary>
        WaitStablePosition,
        /// <summary>
        /// Mecanim turned off, physics controls the ragdoll
        /// </summary>
        Ragdolled,
        /// <summary>
        /// Mecanim in control, but LateUpdate() is used to partially blend in the last ragdolled pose
        /// </summary>
        BlendToAnim,
    }





    Dictionary<string, HashSet<Collider>> UpdatePlayerVision()
    {
        Vector3 halfExtents = new Vector3(visionRadius, visionRadius, visionRadius);
        Vector3 frontOffset = new Vector3(visionRadius / 2f, 0f, 0f);
        Vector3 leftOffset = new Vector3(0f, 0f, visionRadius / 2f);
        Quaternion orientation = Quaternion.LookRotation(transform.forward, transform.up);

        LayerMask layerMaskPlayer = LayerMask.GetMask("Player");
        LayerMask layerMaskBall = LayerMask.GetMask("Ball");
        Dictionary<string, HashSet<Collider>> sets = new Dictionary<string, HashSet<Collider>>();

        sets["nearBall"] = new HashSet<Collider>(Physics.OverlapSphere(transform.position, nearRadius, layerMaskBall));
        sets["near"] = new HashSet<Collider>(Physics.OverlapSphere(transform.position, nearRadius, layerMaskPlayer));
        sets["vision"] = new HashSet<Collider>(Physics.OverlapSphere(transform.position, visionRadius, layerMaskPlayer));
        sets["leftcenter"] = new HashSet<Collider>(Physics.OverlapBox(transform.position + leftOffset, halfExtents, orientation, layerMaskPlayer));
        sets["rightcenter"] = new HashSet<Collider>(Physics.OverlapBox(transform.position - leftOffset, halfExtents, orientation, layerMaskPlayer));
        sets["frontmiddle"] = new HashSet<Collider>(Physics.OverlapBox(transform.position + frontOffset, halfExtents, orientation, layerMaskPlayer));
        sets["backmiddle"] = new HashSet<Collider>(Physics.OverlapBox(transform.position - frontOffset, halfExtents, orientation, layerMaskPlayer));

        // Remove myself
        foreach (KeyValuePair<string, HashSet<Collider>> kvp in sets)
        {
            kvp.Value.Remove(col);
        }

        sets["far"] = new HashSet<Collider>(sets["vision"]);
        sets["far"].ExceptWith(sets["near"]);

        sets["center"] =  new HashSet<Collider>(sets["leftcenter"]);
        sets["center"].IntersectWith(sets["rightcenter"]);

        sets["middle"] = new HashSet<Collider>(sets["frontmiddle"]);
        sets["middle"].IntersectWith(sets["backmiddle"]);

        sets["left"] = new HashSet<Collider>(sets["leftcenter"]);
        sets["left"].ExceptWith(sets["rightcenter"]);

        sets["right"] = new HashSet<Collider>(sets["rightcenter"]);
        sets["right"].ExceptWith(sets["leftcenter"]);

        sets["front"] = new HashSet<Collider>(sets["frontmiddle"]);
        sets["front"].ExceptWith(sets["backmiddle"]);

        sets["back"] = new HashSet<Collider>(sets["backmiddle"]);
        sets["back"].ExceptWith(sets["frontmiddle"]);

        sets["frontLeft"] = new HashSet<Collider>(sets["front"]);
        sets["frontLeft"].ExceptWith(sets["left"]);

        sets["frontCenter"] = new HashSet<Collider>(sets["front"]);
        sets["frontCenter"].ExceptWith(sets["center"]);

        sets["frontRight"] = new HashSet<Collider>(sets["front"]);
        sets["frontRight"].ExceptWith(sets["right"]);

        sets["middleLeft"] = new HashSet<Collider>(sets["middle"]);
        sets["middleLeft"].ExceptWith(sets["left"]);

        sets["middleCenter"] = new HashSet<Collider>(sets["middle"]);
        sets["middleCenter"].ExceptWith(sets["center"]);

        sets["middleRight"] = new HashSet<Collider>(sets["middle"]);
        sets["middleRight"].ExceptWith(sets["right"]);

        sets["backLeft"] = new HashSet<Collider>(sets["back"]);
        sets["backLeft"].ExceptWith(sets["left"]);

        sets["backCenter"] = new HashSet<Collider>(sets["back"]);
        sets["backCenter"].ExceptWith(sets["center"]);

        sets["backRight"] = new HashSet<Collider>(sets["back"]);
        sets["backRight"].ExceptWith(sets["right"]);


        sets["nearFrontLeft"] = new HashSet<Collider>(sets["frontLeft"]);
        sets["nearFrontLeft"].ExceptWith(sets["near"]);

        sets["nearFrontCenter"] = new HashSet<Collider>(sets["frontCenter"]);
        sets["nearFrontCenter"].ExceptWith(sets["near"]);

        sets["nearFrontRight"] = new HashSet<Collider>(sets["frontRight"]);
        sets["nearFrontRight"].ExceptWith(sets["near"]);

        sets["nearMiddleLeft"] = new HashSet<Collider>(sets["middleLeft"]);
        sets["nearMiddleLeft"].ExceptWith(sets["near"]);

        sets["nearMiddleRight"] = new HashSet<Collider>(sets["middleRight"]);
        sets["nearMiddleRight"].ExceptWith(sets["near"]);

        sets["nearbackLeft"] = new HashSet<Collider>(sets["backLeft"]);
        sets["nearbackLeft"].ExceptWith(sets["near"]);

        sets["nearbackCenter"] = new HashSet<Collider>(sets["backCenter"]);
        sets["nearbackCenter"].ExceptWith(sets["near"]);

        sets["nearbackRight"] = new HashSet<Collider>(sets["backRight"]);
        sets["nearbackRight"].ExceptWith(sets["near"]);


        sets["farFrontLeft"] = new HashSet<Collider>(sets["frontLeft"]);
        sets["farFrontLeft"].ExceptWith(sets["far"]);

        sets["farFrontCenter"] = new HashSet<Collider>(sets["frontCenter"]);
        sets["farFrontCenter"].ExceptWith(sets["far"]);

        sets["farFrontRight"] = new HashSet<Collider>(sets["frontRight"]);
        sets["farFrontRight"].ExceptWith(sets["far"]);

        sets["farMiddleLeft"] = new HashSet<Collider>(sets["middleLeft"]);
        sets["farMiddleLeft"].ExceptWith(sets["far"]);

        sets["farMiddleRight"] = new HashSet<Collider>(sets["middleRight"]);
        sets["farMiddleRight"].ExceptWith(sets["far"]);

        sets["farbackLeft"] = new HashSet<Collider>(sets["backLeft"]);
        sets["farbackLeft"].ExceptWith(sets["far"]);

        sets["farbackCenter"] = new HashSet<Collider>(sets["backCenter"]);
        sets["farbackCenter"].ExceptWith(sets["far"]);

        sets["farbackRight"] = new HashSet<Collider>(sets["backRight"]);
        sets["farbackRight"].ExceptWith(sets["far"]);


        /*if (Random.Range(0f, 1f) < (1f/1000f))
        {
            foreach (KeyValuePair<string, HashSet<Collider>> kvp in sets)
            {
                string s = " ";
                foreach (var c in kvp.Value)
                {
                    s += c.gameObject.name + " ";
                }
                Debug.Log(transform.gameObject.name + " Set " + kvp.Key + ": " + kvp.Value.Count + s);
            }
        }*/
        return sets;
    }




}

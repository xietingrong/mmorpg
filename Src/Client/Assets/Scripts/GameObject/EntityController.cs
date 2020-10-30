using SkillBridge.Message;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Entities;
using Managers;

public class EntityController : MonoBehaviour, IEntityNotify, IEntityController
{

    public Animator anim;
    public Rigidbody rb;
    private AnimatorStateInfo currentBaseState;

    public Entity entity;

    public UnityEngine.Vector3 position;
    public UnityEngine.Vector3 direction;
    Quaternion rotation;

    public UnityEngine.Vector3 lastPosition;
    Quaternion lastRotation;

    public float speed;
    public float animSpeed = 1.5f;
    public float jumpPower = 3.0f;

    public bool isPlayer = false;

    public RideController rideController;

    private int currentRide = 0;

    public Transform rideBone;
    public EntityEffectManager EffectMgr;
    // Use this for initialization
    void Start() {
        if (entity != null)
        {
            EntityManager.Instance.RegisterEntityChangeNotify(entity.entityId, this);
            this.EffectMgr = this.gameObject.GetComponent<EntityEffectManager>();
            this.UpdateTransform();
        }

        if (!this.isPlayer)
            rb.useGravity = false;
    }

    public void UpdateTransform()//------
    {
        this.position = GameObjectTool.LogicToWorld(entity.position);
        this.rb.MovePosition(this.position);
        this.lastPosition = this.position;

        UpdateDirection();
    }
 
    public void UpdateDirection()//------
    {
        this.direction = GameObjectTool.LogicToWorld(entity.direction);
        this.transform.forward = this.direction;
        this.lastRotation = this.rotation;
    }
    void OnDestroy()
    {
        if (entity != null)
            Debug.LogFormat("{0} OnDestroy :ID:{1} POS:{2} DIR:{3} SPD:{4} ", this.name, entity.entityId, entity.position, entity.direction, entity.speed);

        if (UIWorldElementManager.Instance != null)
        {
            UIWorldElementManager.Instance.RemoveCharacterNameBar(this.transform);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (this.entity == null)
            return;

        this.entity.OnUpdate(Time.fixedDeltaTime);

        if (!this.isPlayer)
        {
            this.UpdateTransform();
        }
    }

    public void OnEntityChanged(Entity entity)
    {
        Debug.LogFormat("OnEntityChanged :ID:{0} POS:{1} DIR:{2} SPD:{3} ", entity.entityId, entity.position, entity.direction, entity.speed);
    }


    public void OnEntityRemoved()
    {
        if (UIWorldElementManager.Instance != null)
            UIWorldElementManager.Instance.RemoveCharacterNameBar(this.transform);
        Destroy(this.gameObject);
    }

    public void OnEntityEvent(EntityEvent entityEvent, int param)
    {
        switch (entityEvent)
        {
            case EntityEvent.Idle:
                anim.SetBool("Move", false);
                anim.SetTrigger("Idle");
                break;
            case EntityEvent.MoveFwd:
                anim.SetBool("Move", true);
                break;
            case EntityEvent.MoveBack:
                anim.SetBool("Move", true);
                break;
            case EntityEvent.Jump:
                anim.SetTrigger("Jump");
                break;
            case EntityEvent.Ride:
                {
                    this.Ride(param);
                }
                break;
        }
        if (this.rideController != null) this.rideController.OnEntityEvent(entityEvent, param);
    }


    public void Ride(int rideId)
    {
        if (currentRide == rideId) return;
        currentRide = rideId;
        if (rideId > 0)
        {
            this.rideController = GameObjectManager.Instance.LoadRide(rideId, this.transform);
        }
        else
        {
            Destroy(this.rideController.gameObject);
            this.rideController = null;
        }

        if (this.rideController == null)
        {
            this.anim.transform.localPosition = Vector3.zero;
            this.anim.SetLayerWeight(1, 0);
        }
        else
        {
            this.rideController.SetRider(this);
            this.anim.SetLayerWeight(1, 1);
        }
    }

    public void SetRidePotision(Vector3 position)
    {
        this.anim.transform.position = position + (this.anim.transform.position - this.rideBone.position);
    }
    void OnMouseDown()
    {
        Creature target = this.entity as Creature;
        if (target.IsCurrentPlayer) return;
        BattleManager.Instance.CurrentTarget = this.entity as Creature;
    }
    public void PlayAnim(string name)
    {
        this.anim.SetTrigger(name);
    }

    public void SetStandby(bool standby)
    {
        this.anim.SetBool("Standby", standby);
    }

 

    public void PlayEffect(EffectType type, string name, Creature target, float duration)
    {
        Transform transform = null;
        Vector3 pos = new Vector3();
        if (target!= null)
        {
            transform = target.Controller.GetTransform();
            pos = target.GetHitOffset();
        }
          
        if (type == EffectType.Position || type == EffectType.Hit)
            FXManager.Instance.PlayEffect(type, name, transform, pos, duration);
        else
            this.EffectMgr.PlayEffect(type, name, transform, pos, duration);
    }

    public Transform GetTransform()
    {
        return this.transform;
    }

    public void PlayEffect(EffectType type, string name, NVector3 postion, float duration)
    {
        if (type == EffectType.Position || type == EffectType.Hit)
            FXManager.Instance.PlayEffect(type, name, null, GameObjectTool.LogicToWorld(postion), duration);
        else
            this.EffectMgr.PlayEffect(type, name, null, GameObjectTool.LogicToWorld(postion), duration);
    }
}

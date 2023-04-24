using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
    public bool isPlayer;
    public CharacterInput characterInput;
    public Head head;
    public UpperBody upperBody;
    public Locomotion locomotion;
    //public WeaponController weaponController;
    public StanceController stanceController;
    public CharacterEquipmentManager equipmentManager;
    public AnimatorController animatorController;
    public Arms arms;
    public Health health;
    public Body body;
    public Legs legs;
    public DamageReactionController damageReactionController;

    public Rigidbody rb;

    public Transform tRig;
    public GameObject goAliveModel;
    public GameObject goDeadModel;
    public CapsuleCollider capColider;

    [SerializeField] private GameObject goSkinnedMeshObject_REMOVETHISIJUSTHAVETHISFORTESTIG;
    [SerializeField] private RootMotion.FinalIK.LimbIK[] armAndLegIks_REMOVETHISIJUSTHAVETHISFORTESTIG;
    [SerializeField] private EasyIK[] easyIkRigs;

    public event Delegates.EmptyDelegate updateEvent;
    public event Delegates.EmptyDelegate fixedUpdateEvent;
    public event Delegates.EmptyDelegate lateUpdateEvent;

    protected void Awake() {
        rb = GetComponent<Rigidbody>();
        capColider = GetComponent<CapsuleCollider>();

        characterInput = GetComponent<CharacterInput>();
        characterInput.Init(this);

        animatorController.Init(this);
        equipmentManager.Init(this);
        stanceController.Init(this);
        head.Initialize(this);
        locomotion.Initialize(this);
        //weaponController.Init(this);

        upperBody.Init(this);
        damageReactionController.Init(this);
        arms.Init(this);
        health.Init(this);
        body.Init(this);
        legs.Init(this);

        health.diedEvent += Health_diedEvent;

        animatorController.animatorUpdatedEvent += AnimatorController_animatorUpdatedEvent;
    }

    private void AnimatorController_animatorUpdatedEvent() {
        //foreach (EasyIK ik in easyIkRigs)
        //    ik.Solve();
    }

    protected void Update() {

        if (Input.GetKeyDown(KeyCode.F9) && !isPlayer)
            foreach (RootMotion.FinalIK.LimbIK ik in armAndLegIks_REMOVETHISIJUSTHAVETHISFORTESTIG)
                Destroy(ik);

        if (Input.GetKeyDown(KeyCode.F8) && !isPlayer) {
            //foreach (RootMotion.FinalIK.LimbIK ik in armAndLegIks_REMOVETHISIJUSTHAVETHISFORTESTIG)
            //    Destroy(ik);
            Destroy(arms.hand_R);
            Destroy(arms.hand_L);
            Destroy(locomotion.wallrunController);
            Destroy(transform.parent.Find("Cameras").gameObject);
            //Destroy(goSkinnedMeshObject_REMOVETHISIJUSTHAVETHISFORTESTIG);
            Destroy(this);
        }

        if (!health.isAlive)
            return;

        updateEvent?.Invoke();
    }

    protected void FixedUpdate() {
        if (!health.isAlive)
            return;

        fixedUpdateEvent?.Invoke();
    }

    protected void LateUpdate() {
        if (!health.isAlive)
            return;

        lateUpdateEvent?.Invoke();
    }

    private void Health_diedEvent() {
        capColider.enabled = false;
        rb.isKinematic = true;
        goAliveModel.SetActive(false);
        body.Ragdollify();
        goDeadModel.SetActive(true);
    }
}

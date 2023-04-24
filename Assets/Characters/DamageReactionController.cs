using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DamageReactionController {
    public DamageReceiver drPelvis;
    public DamageReceiver drTorso_1;
    public DamageReceiver drTorso_2;
    public DamageReceiver drHead;

    public BoneDamageReaction bdrPelvis;
    public BoneDamageReaction bdrTorso_1;
    public BoneDamageReaction bdrTorso_2;
    public BoneDamageReaction bdrHead;

    private BoxCollider colTorso_1;
    private BoxCollider colTorso_2;

    private Character character;
    public void Init(Character character) {
        this.character = character;

        drPelvis = character.body.tPelvis.GetComponent<DamageReceiver>();
        drTorso_1 = character.body.tTorso_1.GetComponent<DamageReceiver>();
        drTorso_2 = character.body.tTorso_2.GetComponent<DamageReceiver>();
        drHead = character.body.tHead.GetComponent<DamageReceiver>();

        colTorso_1 = drTorso_1.GetComponent<BoxCollider>();
        colTorso_2 = drTorso_2.GetComponent<BoxCollider>();

        bdrPelvis = new BoneDamageReaction(character.body.tPelvis, character);
        bdrTorso_1 = new BoneDamageReaction(character.body.tTorso_1, character);
        bdrTorso_2 = new BoneDamageReaction(character.body.tTorso_2, character);
        bdrHead = new BoneDamageReaction(character.body.tHead, character);

        drPelvis.bulletHitEvent += DrPelvis_bulletHitEvent;
        drTorso_1.bulletHitEvent += DrTorso_1_bulletHitEvent;
        drTorso_2.bulletHitEvent += DrTorso_2_bulletHitEvent;
        drHead.bulletHitEvent += DrHead_bulletHitEvent;

        character.updateEvent += Character_updateEvent;
    }

    private void Character_updateEvent() {
        bdrHead.ApplyAndUpdate();
        bdrTorso_2.ApplyAndUpdate();
        bdrTorso_1.ApplyAndUpdate();
        bdrPelvis.ApplyAndUpdate();
        GizmoManager.i.DrawSphere(Time.deltaTime, Color.blue, character.rb.position + character.rb.centerOfMass, 0.1f);
    }

    private void DrPelvis_bulletHitEvent(float damage, Vector3 hitPoint, Vector3 bulletPathVector) {
        Vector3 COM = character.rb.position + character.rb.centerOfMass;
        Vector3 COMtoPoint = VectorUtils.FromToVector(COM, hitPoint);
        Vector3 COMtoPointVertical = Vector3.Project(COMtoPoint, character.transform.up);
        Vector3 COMtoPointHorizontal = COMtoPoint - COMtoPointVertical;
        Vector3 reactPelvis = COMtoPointVertical * 1.2f + COMtoPointHorizontal * 0.5f;
        Vector3 reactTorso1 = COMtoPointVertical * -0.9f + COMtoPointHorizontal * 0;
        Vector3 reactTorso2 = COMtoPointVertical * -0.75f + COMtoPointHorizontal * 0;
        Vector3 reactHead = COMtoPointVertical * -0f + COMtoPointHorizontal * -0.25f;

        bdrHead.AddReaction(Vector3.Cross(reactHead.normalized, bulletPathVector) * 80 * reactHead.magnitude);
        bdrTorso_2.AddReaction(Vector3.Cross(reactTorso2.normalized, bulletPathVector) * 80 * reactTorso2.magnitude);
        bdrTorso_1.AddReaction(Vector3.Cross(reactTorso1.normalized, bulletPathVector) * 80 * reactTorso1.magnitude);
        bdrPelvis.AddReaction(Vector3.Cross(reactPelvis.normalized, bulletPathVector) * 80 * reactPelvis.magnitude);
        GizmoManager.i.DrawLine(2, Color.green, hitPoint, hitPoint + COMtoPoint.normalized * 4);

        //character.rb.velocity += bulletPathVector * 3;
        character.rb.AddForceAtPosition(bulletPathVector * 3000, hitPoint);
    }

    private void DrTorso_1_bulletHitEvent(float damage, Vector3 hitPoint, Vector3 bulletPathVector) {
        Vector3 COM = character.rb.position + character.rb.centerOfMass;
        Vector3 COMtoPoint = VectorUtils.FromToVector(COM, hitPoint);
        Vector3 COMtoPointVertical = Vector3.Project(COMtoPoint, character.transform.up);
        Vector3 COMtoPointHorizontal = COMtoPoint - COMtoPointVertical;
        Vector3 reactPelvis = COMtoPointVertical * 1.2f + COMtoPointHorizontal * 0.5f;
        Vector3 reactTorso1 = COMtoPointVertical * -0.9f + COMtoPointHorizontal * 0;
        Vector3 reactTorso2 = COMtoPointVertical * -0.75f + COMtoPointHorizontal * 0;
        Vector3 reactHead = COMtoPointVertical * -0f + COMtoPointHorizontal * -0.25f;

        bdrHead.AddReaction(Vector3.Cross(reactHead.normalized, bulletPathVector) * 80 * reactHead.magnitude);
        bdrTorso_2.AddReaction(Vector3.Cross(reactTorso2.normalized, bulletPathVector) * 80 * reactTorso2.magnitude);
        bdrTorso_1.AddReaction(Vector3.Cross(reactTorso1.normalized, bulletPathVector) * 80 * reactTorso1.magnitude);
        bdrPelvis.AddReaction(Vector3.Cross(reactPelvis.normalized, bulletPathVector) * 80 * reactPelvis.magnitude);

        //character.rb.velocity += bulletPathVector * 3;
        character.rb.AddForceAtPosition(bulletPathVector * 3000, hitPoint);
    }

    private void DrTorso_2_bulletHitEvent(float damage, Vector3 hitPoint, Vector3 bulletPathVector) {
        Vector3 COM = character.rb.position + character.rb.centerOfMass;
        Vector3 COMtoPoint = VectorUtils.FromToVector(COM, hitPoint);
        Vector3 COMtoPointVertical = Vector3.Project(COMtoPoint, character.transform.up);
        Vector3 COMtoPointHorizontal = COMtoPoint - COMtoPointVertical;
        Vector3 reactTorso2 = COMtoPointVertical * 0.5f + COMtoPointHorizontal * 2;
        Vector3 reactTorso1 = COMtoPointVertical * 0.2f + COMtoPointHorizontal * 1;
        Vector3 reactHead = COMtoPointVertical * -0.5f + COMtoPointHorizontal * -1;
        GizmoManager.i.DrawLine(2, Color.green, hitPoint, hitPoint + COMtoPoint.normalized * 4);

        bdrHead.AddReaction(Vector3.Cross(reactHead.normalized, bulletPathVector) * 80 * reactHead.magnitude);
        bdrTorso_2.AddReaction(Vector3.Cross(reactTorso2.normalized, bulletPathVector) * 80 * reactTorso2.magnitude);
        bdrTorso_1.AddReaction(Vector3.Cross(reactTorso1.normalized, bulletPathVector) * 80 * reactTorso1.magnitude);

        //character.rb.velocity += bulletPathVector * 3;
        character.rb.AddForceAtPosition(bulletPathVector * 2000, hitPoint);
    }


    private void DrHead_bulletHitEvent(float damage, Vector3 hitPoint, Vector3 bulletPathVector) {
        Vector3 COM = character.rb.position + character.rb.centerOfMass;
        Vector3 COMtoPoint = VectorUtils.FromToVector(COM, hitPoint);
        Vector3 COMtoPointVertical = Vector3.Project(COMtoPoint, character.transform.up);
        Vector3 COMtoPointHorizontal = COMtoPoint - COMtoPointVertical;
        Vector3 reactTorso2 = COMtoPointVertical * 0.2f + COMtoPointHorizontal * 0.3f;
        ////Vector3 reactTorso1 = COMtoPointVertical * 0.2f + COMtoPointHorizontal * 1;
        Vector3 reactHead = COMtoPointVertical * 0.35f + COMtoPointHorizontal * 2f;
        GizmoManager.i.DrawLine(2, Color.green, hitPoint, hitPoint + COMtoPoint.normalized * 4);

        bdrHead.AddReaction(Vector3.Cross(reactHead.normalized, bulletPathVector) * 80 * reactHead.magnitude);
        bdrTorso_2.AddReaction(Vector3.Cross(reactTorso2.normalized, bulletPathVector) * 80 * reactTorso2.magnitude);
        //bdrTorso_1.AddReaction(Vector3.Cross(reactTorso1.normalized, bulletPathVector) * 80 * reactTorso1.magnitude);

        ////character.rb.velocity += bulletPathVector * 3;
        character.rb.AddForceAtPosition(bulletPathVector * 1000, hitPoint);
    }
}

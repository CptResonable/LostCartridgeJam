using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DamageReactionController {
    private DamageReceiver drPelvis;
    private DamageReceiver drTorso_1;
    private DamageReceiver drTorso_2;
    private DamageReceiver drHead;

    private BoneDamageReaction bdrPelvis;
    private BoneDamageReaction bdrTorso_1;
    private BoneDamageReaction bdrTorso_2;
    private BoneDamageReaction bdrHead;

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

        //if (character.isPlayer)
        //    return;

        drPelvis.bulletHitEvent += DrPelvis_bulletHitEvent;
        drTorso_1.bulletHitEvent += DrTorso_1_bulletHitEvent;
        drTorso_2.bulletHitEvent += DrTorso_2_bulletHitEvent;
        drHead.bulletHitEvent += DrHead_bulletHitEvent;

        //character.lateUpdateEvent += Character_lateUpdateEvent;
        //character.updateEvent += Character_updateEvent;
    }

    private void Character_updateEvent() {
        bdrHead.ApplyAndUpdate();
        bdrTorso_2.ApplyAndUpdate();
        bdrTorso_1.ApplyAndUpdate();
        bdrPelvis.ApplyAndUpdate();
        GizmoManager.i.DrawSphere(Time.deltaTime, Color.blue, character.rb.position + character.rb.centerOfMass, 0.1f);
    }

    private void Character_lateUpdateEvent() {
        bdrHead.ApplyAndUpdate();
        bdrTorso_2.ApplyAndUpdate();
        bdrTorso_1.ApplyAndUpdate();
        bdrPelvis.ApplyAndUpdate();
        GizmoManager.i.DrawSphere(Time.deltaTime, Color.blue, character.rb.position + character.rb.centerOfMass, 0.1f);
    }

    public void ManualUpdateTest() {
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

    //private void DrTorso_2_bulletHitEvent(float damage, Vector3 hitPoint, Vector3 bulletPathVector) {
    //    Vector3 COM = character.rb.position + character.rb.centerOfMass;
    //    Vector3 COMtoPoint = VectorUtils.FromToVector(COM, hitPoint);
    //    Vector3 COMtoPointVertical = Vector3.Project(COMtoPoint, character.transform.up);
    //    Vector3 COMtoPointHorizontal = COMtoPoint - COMtoPointVertical;
    //    COMtoPointVertical *= 0.5f;
    //    COMtoPointHorizontal *= 2;
    //    COMtoPoint = COMtoPointVertical + COMtoPointHorizontal;
    //    GizmoManager.i.DrawLine(2, Color.green, hitPoint, hitPoint + COMtoPoint.normalized * 4);

    //    Vector3 v = Vector3.Cross(COMtoPoint.normalized, bulletPathVector);
    //    GizmoManager.i.DrawLine(2, Color.red, hitPoint, hitPoint + v.normalized * 4);
    //    bdrTorso_2.AddReaction(v.normalized * 50);
    //    //Debug.Log("v: " + v.magnitudeb);
    //}

    //private void DrTorso_2_bulletHitEvent(float damage, Vector3 hitPoint, Vector3 bulletPathVector) {
    //    Vector3 COM = character.rb.position + character.rb.centerOfMass;
    //    Vector3 COMtoPoint = VectorUtils.FromToVector(COM, hitPoint);
    //    GizmoManager.i.DrawLine(2, Color.green, hitPoint, hitPoint + COMtoPoint.normalized * 4);

    //    Vector3 v = Vector3.Cross(COMtoPoint.normalized, bulletPathVector);
    //    GizmoManager.i.DrawLine(2, Color.red, hitPoint, hitPoint + v.normalized * 4);
    //    bdrTorso_2.AddReaction(v.normalized * 50);
    //    //Debug.Log("v: " + v.magnitudeb);

    //    Vector3 hPart = new Vector3(COMtoPoint.x, 0, COMtoPoint.z);
    //    Vector3 vPart = new Vector3(0, COMtoPoint.y, 0);

    //    Vector3 yawVector = Vector3.Project(bulletPathVector, Vector3.Cross(character.transform.up, hPart));

    //    float yaw = Vector3.Dot(Vector3.Cross(character.transform.up, hPart), yawVector) * yawVector.magnitude * 90;
    //    //Vector3 bulletPathHorizontal = new Vector3(bulletPathVector.x, 0, bulletPathVector.z);

    //    bdrTorso_2.AddReaction(0, yaw);
    //}
    //private void DrTorso_2_bulletHitEvent(float damage, Vector3 hitPoint, Vector3 bulletPathVector) {
    //    Vector3 COM = character.rb.centerOfMass;
    //    Vector3 COMtoPoint = VectorUtils.FromToVector(COM, hitPoint);

    //    Vector3 proj = Vector3.ProjectOnPlane(COMtoPoint, bulletPathVector);

    //    Vector3 hPart = new Vector3(COMtoPoint.x, 0, COMtoPoint.z);
    //    Vector3 vPart = new Vector3(0, COMtoPoint.y, 0);

    //    Vector3 yawVector = Vector3.Project(bulletPathVector, Vector3.Cross(character.transform.up, hPart));
    //    Debug.Log("huh: " + Vector3.Dot(Vector3.Cross(character.transform.up, hPart), yawVector));
    //    float yaw = Vector3.Dot(Vector3.Cross(character.transform.up, hPart), yawVector) * yawVector.magnitude * 90;
    //    //Vector3 bulletPathHorizontal = new Vector3(bulletPathVector.x, 0, bulletPathVector.z);

    //    bdrTorso_2.AddReaction(0, yaw);
    //}
    //private void DrTorso_2_bulletHitEvent(float damage, Vector3 hitPoint, Vector3 bulletPathVector) {
    //    Vector3 COM = character.rb.centerOfMass;
    //    Vector3 COMtoPoint = VectorUtils.FromToVector(COM, hitPoint);
    //    Vector3 hPart = new Vector3(COMtoPoint.x, 0, COMtoPoint.z);
    //    Vector3 vPart = new Vector3(0, COMtoPoint.y, 0);

    //    Vector3 yawVector = Vector3.Project(bulletPathVector, Vector3.Cross(character.transform.up, hPart));
    //    Debug.Log("huh: " + Vector3.Dot(Vector3.Cross(character.transform.up, hPart), yawVector));
    //    float yaw = Vector3.Dot(Vector3.Cross(character.transform.up, hPart), yawVector) * yawVector.magnitude * 90;
    //    //Vector3 bulletPathHorizontal = new Vector3(bulletPathVector.x, 0, bulletPathVector.z);

    //    bdrTorso_2.AddReaction(0, yaw);
    //}
}

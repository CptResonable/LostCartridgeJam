using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Hand {
    [SerializeField] private Transform tHand;
    [SerializeField] private Transform tArmRotator;

    [SerializeField] private float armRotatorRotationLerpSpeed;

    private Player player;

    public void Initialize(Player player) {
        this.player = player;
        player.updateEvent += Player_updateEvent;
    }

    private void Player_updateEvent() {
        UpdateArmRotator();
        UpdateHand();
    }

    private void UpdateArmRotator() {
        tArmRotator.position = player.head.tHead.position;
        tArmRotator.rotation = Quaternion.Slerp(tArmRotator.rotation, player.head.tHead.rotation, armRotatorRotationLerpSpeed * Time.deltaTime);
    }

    private void UpdateHand() {
        tHand.rotation = player.head.tHead.rotation;
    }
}

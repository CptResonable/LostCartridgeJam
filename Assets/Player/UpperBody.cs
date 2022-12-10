using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpperBody {
    private Player player;

    public void Init(Player player) {
        this.player = player;

        player.updateEvent += Player_updateEvent;
    }

    private void Player_updateEvent() {
        Quaternion t2Rot = Quaternion.Slerp(player.transform.rotation, player.body.tHead.rotation, 0.7f);
        player.body.tTorso_1.rotation = Quaternion.Slerp(player.transform.rotation, player.body.tHead.rotation, 0.35f);
        player.body.tTorso_2.rotation = t2Rot;
        player.body.tTorso_2.Rotate(Vector3.up * 30, Space.Self);
    }
}

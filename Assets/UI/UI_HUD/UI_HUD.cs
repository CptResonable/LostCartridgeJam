using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_HUD : MonoBehaviour {
    [SerializeField] private Image imgHitMark;

    private void Awake() {
        ProjectileManager.i.projectileHitCharacterEvent += I_projectileHitCharacterEvent;
    }

    private void I_projectileHitCharacterEvent(ProjectileHitCharacterParams projectileHitCharacterParams) {
        if (CharacterManager.i.characters[projectileHitCharacterParams.projectileParams.characterId].isPlayer)
            DisplayHitMark(projectileHitCharacterParams);
    }

    private void DisplayHitMark(ProjectileHitCharacterParams projectileHitCharacterParams) {
        Debug.Log("SDSDKSDKLSDKLKLSD");
        //PlayerCharAddon playerAddon = CharacterManager.i.characters[projectileHitCharacterParams.projectileParams.characterId].GetComponent<PlayerCharAddon>();
        //playerAddon.
        //imgHitMark.transform.position =  CharacterManager.i.characters[projectileHitCharacterParams.projectileParams.characterId]. .WorldToScreenPoint(projectileHitCharacterParams.hit.point);
        imgHitMark.transform.position = Camera.main.WorldToScreenPoint(projectileHitCharacterParams.hit.point);
    }
}

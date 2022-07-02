using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(menuName = "Item/Weapon")]
public class Weapon : Item
{
    public enum WeaponKind
    {
        Attack,
        Defense,
    }
    //�@�A�C�e���̎��
    [SerializeField]
    public WeaponKind weaponKind;
    //�@�A�C�e���̃A�C�R��
    [SerializeField]
    public int Power;
}

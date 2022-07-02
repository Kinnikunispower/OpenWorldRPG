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
    //　アイテムの種類
    [SerializeField]
    public WeaponKind weaponKind;
    //　アイテムのアイコン
    [SerializeField]
    public int Power;
}

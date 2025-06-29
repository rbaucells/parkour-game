using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    IList<GameObject> weapons = new List<GameObject>();

    [SerializeField] GameObject weapon1;

    [SerializeField] Transform aimingWeaponContainer;

    [Button("Equip Weapon")]
    public void EquipWeapon()
    {
        if (weapons.Count < 4)
        {
            // summon the weapon
            GameObject curWeapon = Instantiate(weapon1, aimingWeaponContainer);

            // add it to our inventory
            weapons.Add(curWeapon);
            int index = weapons.Count - 1;

            GunScript gunScript = curWeapon.GetComponent<GunScript>();
            // // place it on the screen where it should be
            curWeapon.transform.localPosition = gunScript.screenPositions[index];
        }
    }

    [Button("Delete Weapon")]
    public void RemoveWeapon()
    {
        if (weapons.Count > 0)
        {
            // remove the weapon
            Destroy(weapons[0]);
            weapons.Remove(weapons[0]);

            int counter = 0;

            foreach (GameObject gameObject in weapons)
            {
                gameObject.transform.localPosition = gameObject.GetComponent<GunScript>().screenPositions[counter];
                counter++;
            }
        }
    }
}

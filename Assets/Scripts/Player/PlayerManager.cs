using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour, IDamageable
{

    private AbstractGun _primaryWeapon, _secondaryWeapon, _powerWeapon;

    [SerializeField]
    private GameObject _weaponHolder;
    [SerializeField]
    private GameObject _hudCanvas;

    private StateController _stateController;
    private Healthbar _healthbar;
    // Start is called before the first frame update
    void Start()
    {

        _stateController = GetComponent<StateController>();
        _stateController.SetActiveGun(_weaponHolder.GetComponentInChildren<AbstractGun>());
        _healthbar = _hudCanvas.GetComponentInChildren<Healthbar>();
        _healthbar.onDeath += onDeath;
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void PickupGun(AbstractGun weapon)
    {
        // TODO: handle differrent weapon slots
        

        // TODO: we need to not destroy the child of the _weaponHolder and instead put it on the ground
        foreach (Transform child in _weaponHolder.transform)
        {
            Destroy(child.gameObject);
        }

        _stateController.SetActiveGun(weapon);

        Debug.Log(weapon.transform.position);
        // We have to reset the transform after parenting to the _weaponholder
        Vector3 originalPosition = weapon.transform.position;
        Quaternion originalRotation = weapon.transform.rotation;
        Vector3 originalScale = weapon.transform.localScale;

        weapon.transform.SetParent(_weaponHolder.transform);

        weapon.transform.localPosition = originalPosition;
        weapon.transform.localRotation = originalRotation;
        weapon.transform.localScale = originalScale;
    }

    private void onDeath()
    {
        Debug.Log("YOU DEAD");
    }
    
    public void Damage(float damage)
    {
        _healthbar.ApplyDamage(damage);
    }
}

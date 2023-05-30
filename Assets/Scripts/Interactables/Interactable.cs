using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    private float _angleCutoff = 30f;

    private bool _isInteractable;
    private Collider _playerCollider;
    private Camera _cam;

    private Action<Collider> _interactAction;

    // Start is called before the first frame update
    void Start()
    {
        InputManager.interactAction += Interact;
    }


    private void Interact()
    {
        if (_isInteractable)
        {
            _interactAction?.Invoke(_playerCollider);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // store reference to the collider and camera for cheaper calls moving forwards
            _playerCollider = other;
            _cam = other.gameObject.GetComponentInChildren<Camera>();
        }
    }

    private void OnTriggerStay(Collider other)
    {

        if (other == _playerCollider)
        {
            Vector3 camDirection = _cam.transform.forward;

            Vector3 itemDirection = transform.position - _cam.transform.position;
            itemDirection.Normalize();

            float lookAngle = Vector3.Angle(camDirection, itemDirection);

            if (lookAngle < _angleCutoff)
            {
                _isInteractable = true;
            } else
            {
                _isInteractable = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // throw away our references
        if (other == _playerCollider)
        {
            _playerCollider = null;
            _cam = null;
            _isInteractable = false;
        }
    }

    public void Subscribe(Action<Collider> action)
    {
        _interactAction += action;
    }
}

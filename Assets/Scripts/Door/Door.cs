using System;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _doorBody;

    [Header("Times")]
    [SerializeField] private float _openingTime = 1f;
    [SerializeField] private float _closingTime = 0.5f;

    // EVENTI CHIARI
    public event Action DoorOpening;
    public event Action DoorClosing;
    public event Action DoorOpened;
    public event Action DoorClosed;

    private bool _isOpen = false;
    private bool _isRotating = false;
    private bool _closeRequested = false;

    private bool _opening; // direzione attuale

    private Quaternion _startRotation;
    private Quaternion _targetRotation;
    private Quaternion _fromRotation;

    private float _rotationTimer = 0f;
    private float _currentDuration;

    private void Start()
    {
        _startRotation = _doorBody.transform.localRotation;
    }

    private void Update()
    {
        if (!_isRotating)
            return;

        _rotationTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_rotationTimer / _currentDuration);

        _doorBody.transform.localRotation =
            Quaternion.Lerp(_fromRotation, _targetRotation, t);

        if (t >= 1f)
        {
            _isRotating = false;
            _rotationTimer = 0f;

            _isOpen = _opening;

            if (_isOpen)
                DoorOpened?.Invoke();
            else
                DoorClosed?.Invoke();

            // richiesta di chiusura durante apertura
            if (_closeRequested)
            {
                _closeRequested = false;
                CloseDoor();
            }
        }
    }

    public void OpenDoor(float rotation)
    {
        if (_isOpen)
            return;

        if (_isRotating)
            return;

        _opening = true;
        _fromRotation = _doorBody.transform.localRotation;
        _currentDuration = _openingTime;

        _targetRotation = Quaternion.Euler(
            _startRotation.eulerAngles.x,
            rotation,
            _startRotation.eulerAngles.z
        );

        _isRotating = true;
        DoorOpening?.Invoke();
    }

    public void CloseDoor()
    {
        if (!_isOpen && !_isRotating)
            return;

        if (_isRotating)
        {
            _closeRequested = true;
            return;
        }

        _opening = false;
        _fromRotation = _doorBody.transform.localRotation;
        _currentDuration = _closingTime;
        _targetRotation = _startRotation;

        _isRotating = true;
        DoorClosing?.Invoke();
    }

    public void ToggleDoor(float rotation)
    {
        if (_isOpen)
            CloseDoor();
        else
            OpenDoor(rotation);
    }

    public bool IsOpen => _isOpen;
    public bool IsRotating => _isRotating;
}

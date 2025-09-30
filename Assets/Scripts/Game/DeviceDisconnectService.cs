using UnityEngine;
using UnityEngine.InputSystem;

public class DeviceDisconnectService {
    public void OnDeviceChange(InputDevice device, InputDeviceChange change) {
        if (change == InputDeviceChange.Disconnected) {
            Debug.Log("Device disconnected: " + device);
            Time.timeScale = 0;
        }
        else if (change == InputDeviceChange.Reconnected) {
            Debug.Log("Device reconnected: " + device);
            Time.timeScale = 1f;
        }
    }
}
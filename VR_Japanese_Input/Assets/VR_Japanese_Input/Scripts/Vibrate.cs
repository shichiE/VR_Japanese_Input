using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vibrate : MonoBehaviour {

    /// <summary>
    /// ハンドコントローラー振動
    /// </summary>
    /// <param name="isRightHand">振動する手が右かどうか</param>
    /// <param name="length">振動の長さ</param>
    /// <param name="strength">振動の強さ</param>
    public void VibrateHand(bool isRightHand, int length, byte strength)
    {
        // 振動
        byte[] vibration = new byte[length];
        for (int i = 0; i < vibration.Length; i++)
        {
            vibration[i] = strength;
        }
        OVRHapticsClip hapticsClip = new OVRHapticsClip(vibration, vibration.Length);
        if (isRightHand)
        {
            OVRHaptics.RightChannel.Mix(hapticsClip);
        }
        else
        {
            OVRHaptics.LeftChannel.Mix(hapticsClip);
        }
    }
}

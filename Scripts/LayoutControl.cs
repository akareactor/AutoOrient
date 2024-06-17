using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public enum DebugScreenOri { None, Portrait, Landscape }

// Screen.orientation только для мобильных устройств! https://docs.unity3d.com/ScriptReference/ScreenOrientation.html
// Поэтому для десктопа только альбом!
public class LayoutControl : MonoBehaviour {

    public DebugScreenOri debugOrientation;
    [Tooltip("Камера, которая меняет свою позицию при смене ориентации")]
    public Camera cam; 
    [Tooltip("Перпендикулярный камере квадрат, который должен касаться краями экрана в Portrait или Landscape")]
    public GameObject quad;
    [Tooltip("Отступ по горизонтали для портрета")]
    public float marginX = 0.25f;
    [Tooltip("Отступ от вертикали для альбома")]
    public float marginY = 0.5f;
    bool landscapeOrientation = false; // для однократного переключения
    public UnityEvent portraitEvent;
    public UnityEvent landscapeEvent;
    public GameObject portraitCanvas;
    public GameObject landscapeCanvas;


    // Концепция проста - при смене ориентации можно сдвигать камеру так, чтобы объекты в поле зрения оставались вписанными в квадрат.
    // просто двигаем камеру так, чтобы квадрат полностью попал в поле зрения по горизонтали или вертикали
    // https://answers.unity.com/questions/1190535/auto-scale-camera-to-fit-game-object-to-screen-cen.html
    // https://forum.unity.com/threads/fit-object-exactly-into-perspective-cameras-field-of-view-focus-the-object.496472/
    void RepositionCam () {
        if (quad) {
            Vector3 quadSize = quad.GetComponent<Renderer>().bounds.size;
            float fov = 0;
            float halfSize = 0;
            float margin = 0;
            if (landscapeOrientation) {
                fov = cam.fieldOfView; 
                halfSize = quadSize.y / 2f;
                margin = marginY;
            } else {
                fov = Camera.VerticalToHorizontalFieldOfView(cam.fieldOfView, cam.aspect);
                halfSize = quadSize.x / 2f;
                margin = marginX;
            }
            float dist = (halfSize + margin) / (Mathf.Tan(fov * Mathf.Deg2Rad) / 2f);
            cam.transform.position = quad.transform.position - dist * (quad.transform.position - cam.transform.position).normalized;
        }
	}

    // сигнал приходит от CameraAspect
    // Достаточно знать только пропорцию экрана.
    public void ChangeOrientationEvent (float aspect) {
        // Лучше альбом включать на aspect >= 1, а не на aspect > 1, так надёжнее работает на квадрате.
        if (aspect >= 1) { // landscape
            landscapeOrientation = true;
            portraitCanvas.SetActive(false); landscapeCanvas.SetActive(true);
            landscapeEvent.Invoke();
        } else { // portrait
            landscapeOrientation = false;
            landscapeCanvas.SetActive(false); portraitCanvas.SetActive(true);
            portraitEvent.Invoke();
        }
        if (cam) RepositionCam();
    }

}

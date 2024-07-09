using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// <summary>
// https://forum.unity.com/threads/force-camera-aspect-ratio-16-9-in-viewport.385541/
// </summary>

[System.Serializable]
public class FloatEvent : UnityEvent<float> {}

[RequireComponent(typeof(Camera))]
public class CameraAspect : MonoBehaviour {

	public float aspectSoftness = 0.1f; // мягкость пропорций, обычно 10% нормально
	public float landscape = 16.0f / 9.0f;
	public float portrait = 9.0f / 16.0f;
	private int ScreenSizeX = 0;
	private int ScreenSizeY = 0;
	public Camera camera;
	public FloatEvent rescaleEvent;
	public AspectRatioFitter landscapeFitter;
	public AspectRatioFitter portraitFitter;
	public AspectRatioFitter commonFitter; // 2024-07-09 15:32:48 добавил третий фиттер, общий. Чтоб не ломать совместимость.
	public bool clearBackground = true;
	public Color clearColor = Color.red;

	private void Awake () {
		if (!camera) camera = GetComponent<Camera>(); else print("Need a link to camera!");
	}

	private float RescaleCamera () {
		float windowaspect = (float)Screen.width / (float)Screen.height;
		float aspect = windowaspect;
		// 10% мягкость пропорций
		// Значение aspect остаётся неопределённым за рамками условий < или >, это позволяет сохранять предыдущую пропорцию до тех пор, пока не вычислится новая.
		if (windowaspect >= 1) {
			if (windowaspect < ((1f - aspectSoftness) * landscape)) aspect = (1f - aspectSoftness) * landscape;
			if (windowaspect > ((1f + aspectSoftness) * landscape)) aspect = (1f + aspectSoftness) * landscape;
			if (landscapeFitter != null) landscapeFitter.aspectRatio = aspect;
		}
		if (windowaspect < 1) {
			if (windowaspect < ((1f - aspectSoftness) * portrait)) aspect = (1f - aspectSoftness) * portrait;
			if (windowaspect > ((1f + aspectSoftness) * portrait)) aspect = (1f + aspectSoftness) * portrait;
			if (portraitFitter != null) portraitFitter.aspectRatio = aspect;
		}
		// общий фиттер приспосабливается и к портрету, и к альбому
		if (commonFitter != null) commonFitter.aspectRatio = aspect;
		// финальный расчёт рамки камеры
		float scaleheight = windowaspect / aspect;
		if (camera) {
			if (scaleheight < 1.0f) {
				Rect rect = camera.rect;
				rect.width = 1.0f;
				rect.height = scaleheight;
				rect.x = 0;
				rect.y = (1.0f - scaleheight) / 2.0f;
				camera.rect = rect;
			} else { // add pillarbox
				float scalewidth = 1.0f / scaleheight;
				Rect rect = camera.rect;
				rect.width = scalewidth;
				rect.height = 1.0f;
				rect.x = (1.0f - scalewidth) / 2.0f;
				rect.y = 0;
				camera.rect = rect;
			}
		} else print("No camera here!");
	return windowaspect;
	}

	void OnPreCull () {
		/* Clear background - no need with background camera */
		if (clearBackground) {
			//if (Application.isEditor) return;
			Rect wp = Camera.main.rect;
			Rect nr = new Rect(0, 0, 1, 1);
			Camera.main.rect = nr;
			GL.Clear(true, true, clearColor);
			Camera.main.rect = wp;
		}
	}

	void Start () {
		//RescaleCamera(); это бессмысленно! Первый же Update сделает это.
	}

	void Update () { 
		// оптимизация и охрана изменения размеров окна
		if (Screen.width != ScreenSizeX || Screen.height != ScreenSizeY) {
			rescaleEvent?.Invoke(RescaleCamera());
			ScreenSizeX = Screen.width; ScreenSizeY = Screen.height;
		}
	}

}

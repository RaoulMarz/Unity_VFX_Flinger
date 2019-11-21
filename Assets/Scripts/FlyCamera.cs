using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
public class FlyCamera : MonoBehaviour {
/*
   Writen by Windexglow 11-13-10.  Use it, edit it, steal it I don't care.
   Converted to C# 27-02-13 - no credit wanted.
   [Raoul Marais - Adapted and added quite more functionality -- 18 November 2019]
   Expanded a bit with more key alternatives and a UI which should be handled somewhere else really
   wasd : basic movement
   shift : Makes camera accelerate
   space : Moves camera on X and Z axis only.  So camera doesn't gain any height*/
float mainSpeed = 1.0f;     //regular speed
float bogSlow = 0.25f;     //to reduce speed to a much slower turtle trotting speed
float shiftAdd = 2.2f;     //multiplied by how long shift is held.  Basically running
float maxShift = 5.0f;     //Maximum speed when holdin gshift
float camSens = 0.15f;     //How sensitive it with mouse
private Vector3 lastMouse = new Vector3(255, 255, 255);     //kind of in the middle of the screen, rather than at the top (play)
private Vector3 cameraLookAt = new Vector3(10, 5, 2.0f);
private float totalRun = 1.0f;
private bool settingsVisible = false;
private bool helpVisible = false;
private Dictionary<String, DateTime> keyBounceMap = new Dictionary<String, DateTime>();
const Int32 BOUNCE_TIMEOUT_MILLISECS = 900;
public bool appFlagShowSandstorm = true;
public bool appFlagShowCameraSandstorm = true;
public bool appFlagShowTorches = true;
public bool appFlagLandscapeFire = true;
public Text positionText;
public Text velocityText;
public Text infoText;
public RectTransform panelSettings;
public RectTransform panelHelp;
public Light sceneLight;
GameObject gTest1;
GameObject gTest2;

void clearInformationDisplayText() {
	infoText.text = "";
	positionText.text = "";
	velocityText.text = "";
}

void toggleSettingPanelDisplay() {
	settingsVisible = !settingsVisible;
	if (settingsVisible)
		helpVisible = false;
}

void toggleHelpDisplay() {
	helpVisible = !helpVisible;
	if (helpVisible)
		settingsVisible = false;
}

/* Need to flesh this out -- activate or deactivate objects with a certain tag */
void activateActionOnTag(bool activate, string tag) {

}

DateTime getBounceMapKey(String key) {
	if (keyBounceMap.ContainsKey(key)) {
		return keyBounceMap[key];
	}
	return new DateTime();
}

void setBounceKeyValue(String key, DateTime value) {
	//keyBounceMap.TryAdd(key, value);
	if (keyBounceMap.ContainsKey(key)) {
		keyBounceMap.Remove(key);
		keyBounceMap.Add(key, value);
	} else
		keyBounceMap.Add(key, value);
}

void showSettingsConsoleIfVisible(bool infoDisplay = true) {
	GameObject pnlFrameSettings = GameObject.Find("PanelSettings");
	if (pnlFrameSettings != null) {
		Transform[] allChildren = pnlFrameSettings.GetComponentsInChildren<Transform>();
		if (infoDisplay)
			infoText.text = $"Game object found :: pnlFrameSettings={pnlFrameSettings} , #children = {allChildren.Length}";
		pnlFrameSettings.gameObject.SetActive(settingsVisible); //!pnlFrameSettings.activeSelf
		return;
	} else {
		panelSettings.gameObject.SetActive(settingsVisible);  //!panelSettings.gameObject.activeSelf
	}
}

void showHelpPanelIfVisible(bool infoDisplay = true) {
	GameObject pnlFrameHelp = GameObject.Find("PanelHelp");
	if (pnlFrameHelp != null) {
		pnlFrameHelp.gameObject.SetActive(helpVisible); //!pnlFrameSettings.activeSelf
		return;
	} else {
		panelHelp.gameObject.SetActive(helpVisible);  //!panelSettings.gameObject.activeSelf
	}
}

bool isCameraMovementEnabled() {
	return (!settingsVisible) && (!helpVisible);
}

void displayGenerateTestUIdemo() {
	gTest1 = new GameObject();
	Canvas canvas = gTest1.AddComponent<Canvas>();
	canvas.renderMode = RenderMode.WorldSpace;
	CanvasScaler cs = gTest1.AddComponent<CanvasScaler>();
	cs.scaleFactor = 10.0f;
	cs.dynamicPixelsPerUnit = 10f;
	GraphicRaycaster gr = gTest1.AddComponent<GraphicRaycaster>();
	gTest1.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 3.0f);
	gTest1.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 3.0f);
	gTest2 = new GameObject();
	gTest2.name = "Text";
	gTest2.transform.parent = gTest1.transform;
	Text t = gTest2.AddComponent<Text>();
	gTest2.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 3.0f);
	gTest2.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 3.0f);
	t.alignment = TextAnchor.MiddleCenter;
	t.horizontalOverflow = HorizontalWrapMode.Overflow;
	t.verticalOverflow = VerticalWrapMode.Overflow;
	Font ArialFont = (Font)Resources.GetBuiltinResource (typeof(Font), "Arial.ttf");
	t.font = ArialFont;
	t.fontSize = 7;
	t.text = "Test";
	t.enabled = true;
	t.color = Color.black;
	gTest1.name = "Text Label";
	bool bWorldPosition = false;
	float fTextLabelHeight = 38.0f;
	gTest1.GetComponent<RectTransform>().SetParent(this.transform, bWorldPosition);
	gTest1.transform.localPosition = new Vector3(0f, fTextLabelHeight, 0f);
	gTest1.transform.localScale = new Vector3(
		1.0f / this.transform.localScale.x * 0.1f,
		1.0f / this.transform.localScale.y * 0.1f,
		1.0f / this.transform.localScale.z * 0.1f );
}

void actionToggleSandstorm(bool toggleValue) {
    appFlagShowSandstorm = toggleValue;
	Debug.Log($"actionToggleSandstorm() called(), ${toggleValue}");
	GameObject effectSandstorm_Gust = GameObject.Find("Particle_SandGust1_box");
	GameObject effectSandstorm_Cone1 = GameObject.Find("Particle_SandGust2_Cone");
	GameObject effectSandstorm_Cone2 = GameObject.Find("Particle_SandGust3_ConeR");
    /* Can also use effectSandstorm_Cone2.activeSelf when not having a global toggle (group) */
	if (effectSandstorm_Gust != null) {
		effectSandstorm_Gust.SetActive(toggleValue);
	}
	if (effectSandstorm_Cone1 != null) {
		effectSandstorm_Cone1.SetActive(toggleValue);
	}
	if (effectSandstorm_Cone2 != null) {
		effectSandstorm_Cone2.SetActive(toggleValue);
	}
}

void actionToggleCameraAchorStorm(bool toggleValue) {
    appFlagShowCameraSandstorm = toggleValue;
	Debug.Log($"actionToggleCameraAchorStorm() called(), ${toggleValue}");
	//GameObject effectSandstorm = GameObject.Find("Toggle_Static_Sandstorm");
}

void actionToggleTorches(bool toggleValue) {
    appFlagShowTorches = toggleValue;
	Debug.Log($"actionToggleTorches() called(), ${toggleValue}");
	GameObject objectTorch1 = GameObject.Find("Torch-Post");
	if (objectTorch1 != null) {
        Debug.Log($"actionToggleTorches() finding Torch-Post, ${objectTorch1}");
        objectTorch1.SetActive(toggleValue);
		//objectTorch1.gameObject.SetActive(objectTorch1.gameObject.activeSelf);
	}
    GameObject objectTorch2 = GameObject.Find("Sleeper-Pole-Post");
    if (objectTorch2 != null) {
        Debug.Log($"actionToggleTorches() finding Sleeper-Pole-Post, ${objectTorch2}");
        objectTorch2.SetActive(toggleValue);
	}
	GameObject objectTorch3 = GameObject.Find("Torch-Mounted");
	if (objectTorch3 != null) {
        Debug.Log($"actionToggleTorches() finding Torch-Mounted, ${objectTorch3}");
		objectTorch3.SetActive(toggleValue);
	}
	GameObject objectTorch4 = GameObject.Find("Torch-On-Post");
	if (objectTorch4 != null) {
		Debug.Log($"actionToggleTorches() finding Torch-On-Post, ${objectTorch4}");
		objectTorch4.SetActive(toggleValue);
	}
}

void actionToggleLandscapeFire(bool toggleValue) {
	appFlagLandscapeFire = toggleValue;
	Debug.Log($"actionToggleLandscapeFire() called(), ${toggleValue}");
	GameObject objectScapeFire1 = GameObject.Find("Particle-Fire-Zone1");
	if (objectScapeFire1 != null) {
        Debug.Log($"actionToggleLandscapeFire() finding Particle-Fire-Zone1, ${objectScapeFire1}");
        objectScapeFire1.SetActive(toggleValue);
	}
    GameObject objectScapeFire2 = GameObject.Find("Particle-Fire-Zone2");
    if (objectScapeFire2 != null) {
        Debug.Log($"actionToggleLandscapeFire() finding Particle-Fire-Zone2, ${objectScapeFire2}");
        objectScapeFire2.SetActive(toggleValue);
	}
}

void Start() {
	Debug.Log("Start called for FlyCamera");
    Toggle toggleObj = (Toggle)FindObjectOfType(typeof(Toggle));
    if (toggleObj)
        Debug.Log($"Toggle object found: ${toggleObj}");
    else
        Debug.Log("No Toggle object could be found");
	
	try {
        GameObject objectStaticSandstorm = GameObject.Find("Toggle_Static_Sandstorm");
        Debug.Log($"objectStaticSandstorm object = ${objectStaticSandstorm.name}");
        if (objectStaticSandstorm is Selectable) {
            Debug.Log($"objectStaticSandstorm is Selectable , object = ${objectStaticSandstorm}");
        }
        if (objectStaticSandstorm is Toggle) {
            Debug.Log($"objectStaticSandstorm is Toggle , object = ${objectStaticSandstorm.name}");
        }        
        Toggle toggleStaticSandstorm = objectStaticSandstorm.GetComponent<Toggle>();
		if (toggleStaticSandstorm != null) {
			toggleStaticSandstorm.onValueChanged.AddListener(delegate {
					actionToggleSandstorm(toggleStaticSandstorm.isOn);
				});
		}
        
	} catch (Exception e) {
		Debug.Log($"toggleStaticSandstorm, exception= ${e.Message}");
	}
	try {
        GameObject objectCameraStormEffects = GameObject.Find("Toggle_Camera_Anchored_Storm");
		Toggle toggleCameraStormEffects = GameObject.Find("Toggle_Camera_Anchored_Storm").GetComponent<Toggle>();
		if (toggleCameraStormEffects != null) {
			toggleCameraStormEffects.onValueChanged.AddListener(delegate {
					actionToggleCameraAchorStorm(toggleCameraStormEffects.isOn);
				});
		}
	} catch (Exception e) {
		Debug.Log($"toggleCameraStormEffects, exception= ${e.Message}");
	}
	try {
		Toggle toggleShowTorches = GameObject.Find("Toggle_Show_Torches").GetComponent<Toggle>();
		if (toggleShowTorches != null) {
			toggleShowTorches.onValueChanged.AddListener(delegate {
					actionToggleTorches(toggleShowTorches.isOn);
				});
		}
	} catch (Exception e) {
		Debug.Log($"toggleShowTorches, exception= ${e.Message}");
	}
	try {
		Toggle toggleShowLandscapeFire = GameObject.Find("Toggle_Show_Landscape_Fire").GetComponent<Toggle>();
		if (toggleShowLandscapeFire != null) {
			toggleShowLandscapeFire.onValueChanged.AddListener(delegate {
					actionToggleLandscapeFire(toggleShowLandscapeFire.isOn);
				});
		}
	} catch (Exception e) {
		Debug.Log($"toggleShowLandscapeFire, exception= ${e.Message}");
	}
    //Move the toggle of the UI components to the end, because of a very important rule
    // "returns active GameObjects" when using GameObject.Find(), so objects must *be* *ACTIVE*
    if (panelSettings != null) {
		showSettingsConsoleIfVisible(false);
	}
	if (panelHelp != null) {
		showHelpPanelIfVisible(false);
	}
}

bool outsideBounds(Vector3 newPosition, bool ignoreZ = false) {
    if (newPosition != null) {
        if ( (newPosition.x > 200.0) || (newPosition.y > 200.0) )
            return true;
    }
	return false;
}

void Update() {
	if (sceneLight != null) {
		sceneLight.intensity = 0.35f;
	}
	bool allowCameraMovement = isCameraMovementEnabled();
	if (allowCameraMovement) {
		lastMouse = Input.mousePosition - lastMouse;
		lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0 );
		lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
		transform.eulerAngles = lastMouse;
		lastMouse = Input.mousePosition;
	}
	//Mouse  camera angle done.
	//Keyboard commands
	Vector3 p = GetBaseInput();
	if (allowCameraMovement) {
		float speedCoefficient = mainSpeed;
		if (Input.GetKey (KeyCode.RightControl)) {
			toggleSettingPanelDisplay();
		}
		if ( (Input.GetKey (KeyCode.LeftShift)) || (Input.GetKey (KeyCode.LeftControl)) ) {
			totalRun += Time.deltaTime;
			if (Input.GetKey (KeyCode.LeftShift))
				speedCoefficient = shiftAdd; //p  = p * totalRun * shiftAdd;
			else
				speedCoefficient = bogSlow;
			p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
			p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
			p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
		}
		else {
			totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
			//p = p * mainSpeed;
		}
		if (velocityText != null) {
			velocityText.text = $"Velocity = {speedCoefficient}";
			positionText.text = $"Position change <before> = {p}";
		}
		p = p * speedCoefficient;
		p = p * Time.deltaTime;

		Vector3 newPosition = transform.position;

		if (Input.GetKey(KeyCode.Space)) { //If player wants to move on X and Z axis only
			if (outsideBounds(newPosition, true) == false) {
				transform.Translate(p);
				newPosition.x = transform.position.x;
				newPosition.z = transform.position.z;
				transform.position = newPosition;
			}
		}
		else {
			if (outsideBounds(newPosition) == false)
				transform.Translate(p);
		}
	}
}
private void switchFunctionPanel() {
	/*
	   if (helpVisible) {
	    showHelpPanelIfVisible();
	    return;
	   }
	   showSettingsConsoleIfVisible();
	 */
	showHelpPanelIfVisible(helpVisible);
	showSettingsConsoleIfVisible(settingsVisible);
}
private Vector3 GetBaseInput() {     //returns the basic values, if it's 0 than it's not active.
	Vector3 p_Velocity = new Vector3();
	DateTime currentTime = DateTime.Now;
	if ( (Input.GetKey (KeyCode.W)) || (Input.GetKey (KeyCode.UpArrow)) ) {
		p_Velocity += new Vector3(0, 0, 1);
	}
	if ( (Input.GetKey (KeyCode.S)) || (Input.GetKey (KeyCode.DownArrow)) ) {
		p_Velocity += new Vector3(0, 0, -1);
	}
	if ( (Input.GetKey (KeyCode.A))  || (Input.GetKey (KeyCode.LeftArrow)) ) {
		p_Velocity += new Vector3(-1, 0, 0);
	}
	if ( (Input.GetKey (KeyCode.D)) || (Input.GetKey (KeyCode.RightArrow)) ) {
		p_Velocity += new Vector3(1, 0, 0);
	}
	if (Input.GetKey(KeyCode.F1)) {
		DateTime lastKeyPressTime = getBounceMapKey("F1");
		TimeSpan tsDiff = currentTime.Subtract(lastKeyPressTime);
		if (tsDiff.TotalMilliseconds <= BOUNCE_TIMEOUT_MILLISECS)
			return p_Velocity;
		else {
			setBounceKeyValue("F1", currentTime);
		}
		clearInformationDisplayText();
		toggleHelpDisplay();
		switchFunctionPanel();
		//showHelpPanelIfVisible();
		//displayGenerateTestUIdemo();
	}
	if (Input.GetKey(KeyCode.F2)) {
		DateTime lastKeyPressTime = getBounceMapKey("F2");
		TimeSpan tsDiff = currentTime.Subtract(lastKeyPressTime);
		if (tsDiff.TotalMilliseconds <= BOUNCE_TIMEOUT_MILLISECS)
			return p_Velocity;
		else {
			setBounceKeyValue("F2", currentTime);
		}
		clearInformationDisplayText();
		toggleSettingPanelDisplay();
		switchFunctionPanel();
	}
	if ( (Input.GetKey(KeyCode.Escape)) || (Input.GetKey (KeyCode.Q)) ) {
		DateTime lastKeyPressTime = getBounceMapKey("Escape");
		TimeSpan tsDiff = currentTime.Subtract(lastKeyPressTime);
		if (tsDiff.TotalMilliseconds <= BOUNCE_TIMEOUT_MILLISECS)
			return p_Velocity;
		else {
			setBounceKeyValue("Escape", currentTime);
		}
		if (infoText != null)
			infoText.text = "Quiting game";
		Application.Quit();
	}
	return p_Velocity;
}
}

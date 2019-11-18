using UnityEngine;
using UnityEngine.UI;
using System.Collections;
 
public class FlyCamera : MonoBehaviour {
 
    /*
    Writen by Windexglow 11-13-10.  Use it, edit it, steal it I don't care.  
    Converted to C# 27-02-13 - no credit wanted.
    Simple flycam I made, since I couldn't find any others made public.  
    Made simple to use (drag and drop, done) for regular keyboard layout  
    wasd : basic movement
    shift : Makes camera accelerate
    space : Moves camera on X and Z axis only.  So camera doesn't gain any height*/
     
     
    float mainSpeed = 1.0f; //regular speed
    float bogSlow = 0.25f;
    float shiftAdd = 2.2f; //multiplied by how long shift is held.  Basically running
    float maxShift = 5.0f; //Maximum speed when holdin gshift
    float camSens = 0.15f; //How sensitive it with mouse
    private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)
    private float totalRun = 1.0f;

    public Text positionText;
    public Text velocityText;
     
    void Update () {
        lastMouse = Input.mousePosition - lastMouse ;
        lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0 );
        lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x , transform.eulerAngles.y + lastMouse.y, 0);
        transform.eulerAngles = lastMouse;
        lastMouse = Input.mousePosition;
        //Mouse  camera angle done.  
       
        //Keyboard commands
        float speedCoefficient = mainSpeed;
        Vector3 p = GetBaseInput();
        if ( (Input.GetKey (KeyCode.LeftShift)) || (Input.GetKey (KeyCode.LeftControl)) ) {
            totalRun += Time.deltaTime;
            if (Input.GetKey (KeyCode.LeftShift))
                speedCoefficient = shiftAdd;  //p  = p * totalRun * shiftAdd;
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
            positionText.text = $"Position <before> = {p}";
        }
        p = p * speedCoefficient;
        
       
        p = p * Time.deltaTime;
        //positionText.text = $"Position = {p}";
        Vector3 newPosition = transform.position;
        if (Input.GetKey(KeyCode.Space)){ //If player wants to move on X and Z axis only
            transform.Translate(p);
            newPosition.x = transform.position.x;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }
        else {
            transform.Translate(p);
        }
       
    }
     
    private Vector3 GetBaseInput() { //returns the basic values, if it's 0 than it's not active.
        Vector3 p_Velocity = new Vector3();
        if ( (Input.GetKey (KeyCode.W)) || (Input.GetKey (KeyCode.UpArrow)) ) {
            p_Velocity += new Vector3(0, 0 , 1);
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
        return p_Velocity;
    }
}

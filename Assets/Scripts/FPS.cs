using UnityEngine;
using System.Collections;
using UnityEngine.UI;
//[AddComponentMenu( "Utilities/FPS")]
public class FPS : MonoBehaviour
{	
	public bool updateColor = true; // Do you want the color to change if the FPS gets low
	public bool allowDrag = true; // Do you want to allow the dragging of the FPS window
	public  float frequency = 0.5F; // The update frequency of the fps
	public int nbDecimal = 1; // How many decimal do you want to display
	
	private float accum   = 0f; // FPS accumulated over the interval
	private int   frames  = 0; // Frames drawn over the interval
	private Color color = Color.white; // The color of the GUI, depending of the FPS ( R < 10, Y < 30, G >= 30 )
	private string sFPS = ""; // The fps formatted into a string.

    void Start()
    {
        StartCoroutine( fpsEnum() );
    }

    public Text FPS_text;
    void Update()
    {
        accum += Time.timeScale/ Time.deltaTime;
        ++frames;

        FPS_text.text = sFPS + " FPS";
        FPS_text.color = color;
    }
    public float fps;
    IEnumerator fpsEnum()
    {
        
        while( true)
        {
            fps = accum/frames;
            sFPS = fps.ToString( "f" + Mathf.Clamp( nbDecimal, 0, 10 ) );
            color = (fps >= 30) ? Color.green : ((fps > 10) ? Color.red : Color.yellow);
            accum = 0.0F;
            frames = 0;
            
            yield return new WaitForSeconds( frequency );
        }
    }
}
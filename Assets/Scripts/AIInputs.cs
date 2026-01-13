using UnityEngine;
using UnityEngine.UI;

public class AIInputs : MonoBehaviour
{
    private CarAI ca;

    public GameObject aiPanel;
    
    public RectTransform s;
    public Image steer, acc, br;
    public Text speedText;
    
    public Color usedColor, normalColor;

    private void Update()
    {
        aiPanel.SetActive(false);
        
        if (GameObject.FindGameObjectWithTag("Car"))
        {
            ca = GameObject.FindGameObjectWithTag("Car").GetComponent<CarAI>();
        }
        else
        {
            return;
        }
        
        aiPanel.SetActive(true);

        if (ca.steerOutput != 0)
        {
            s.localRotation = Quaternion.Euler(0, 0, -ca.steerOutput);
            steer.color = usedColor;
        }
        else
        {
            s.localRotation = Quaternion.Euler(0, 0, 0);
            steer.color = normalColor;
        }

        if (!ca.isBraking)
        {
            acc.color = usedColor;
        }
        else
        {
            acc.color = normalColor;
        }
        
        if (ca.isBraking)
        {
            br.color = usedColor;
        }
        else
        {
            br.color = normalColor;
        }

        speedText.text = ca.speedKPH.ToString("0");
    }
}

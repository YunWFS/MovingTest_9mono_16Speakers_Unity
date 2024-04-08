using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonoController : MonoBehaviour
{
    OSC osc;
    Button btn;
    public VirtualSourceController virtualSourceController;
    // Start is called before the first frame update
    void Start()
    {
        osc = GameObject.Find("Osc").GetComponent<OSC>();
        virtualSourceController = GameObject.Find("Monos").GetComponent<VirtualSourceController>();
        btn = GetComponent<Button>();
		btn.onClick.AddListener(TaskOnClick);
    }

    // Update is called once per frame
    void Update()
    {
        if(virtualSourceController.MoveingSource || virtualSourceController.StaticSource) btn.interactable = false;
        else btn.interactable = true;
        // OscMessage message;

        // message = new OscMessage();
        // message.address = "/OnMouseDownXYZ";
        // message.values.Add(transform.position.x);
        // message.values.Add(transform.position.y);
        // message.values.Add(transform.position.z);
        // osc.Send(message);
    }

    void TaskOnClick(){
		Debug.Log ("You have clicked the button!");
		Debug.Log (GetComponent<RectTransform>().anchoredPosition);
		Debug.Log (virtualSourceController.position1);
		Debug.Log (this.name);

        virtualSourceController.ChangeSelectedPositon(GetComponent<RectTransform>().anchoredPosition, this.name);
        // OscMessage message;
        

        // message = new OscMessage();
        // message.address = "/OnMouseDownXYZ";
        // message.values.Add(transform.position.x);
        // message.values.Add(transform.position.y);
        // message.values.Add(transform.position.z);
        // osc.Send(message);
	}
}

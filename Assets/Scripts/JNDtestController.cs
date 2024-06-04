using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JNDtestController : MonoBehaviour
{
    OSC osc;
    public Vector2 position1;
    public Vector2 position2;
    public string mono1;
    public string mono2;
    bool hasFirst = false;
    bool hasSecond = false;

    public bool MoveingSource = false;
    public bool StaticSource = false;

    string[] sounds = new string[]{"BGM", "Bird"};
    string soundName = "BGM";

    // public Vector3 MovingPosition;

    TextMeshProUGUI mText;
    Button startBtn;
    TMP_Dropdown m_Dropdown;

    GameObject MovingMono;

    float moveTime = 5.0f;
    float moving = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        osc = GameObject.Find("Osc").GetComponent<OSC>();
        moveTime = 5.0f;
        hasFirst = false;
        hasSecond = false;
        mono1 = "";
        mono2 = "";
        soundName = sounds[1];
        // MovingPosition = new Vector3(0.0f, 0.0f, 0.0f);
        MovingMono = GameObject.Find("MovingMono");
        MovingMono.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 0.0f);
        mText = GameObject.Find("MonoInfo").GetComponent<TextMeshProUGUI>();
        startBtn = GameObject.Find("StartButton").GetComponent<Button>();
        startBtn.onClick.AddListener(StartOnClick);
        
        m_Dropdown = GameObject.Find("SelectSound").GetComponent<TMP_Dropdown>();
        DropdownInit();
        m_Dropdown.value = 1;
        m_Dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(m_Dropdown);
        });
    }

    // Update is called once per frame
    void Update()
    {
        if(StaticSource || MoveingSource || !hasFirst) startBtn.interactable = false;
        else startBtn.interactable = true;
        if(StaticSource){
            moving += Time.deltaTime;
        } else if(MoveingSource){
            moving += Time.deltaTime;
            MovingMono.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(position1, position2, moving / moveTime);
            
            OscMessage message = new OscMessage();
            message.address = "/UpdateXYZ";
            message.values.Add(1); // monoIndex
            message.values.Add(MovingMono.GetComponent<RectTransform>().anchoredPosition.x); // x
            message.values.Add(MovingMono.GetComponent<RectTransform>().anchoredPosition.y); // y
            message.values.Add(0); // z
            osc.Send(message);   
        }
        if(moving >= moveTime){
            StaticSource = false;
            MoveingSource = false;
            moving = 0.0f;
            hasFirst = false;
            hasSecond = false;
            mono1 = "";
            mono2 = "";

            OscMessage message = new OscMessage();
            message.address = "/StopSound";
            message.values.Add(1); // monoIndex
            osc.Send(message);
        }
    }

    public void ChangeSelectedPositon(Vector2 pos, string monoName){
        if(StaticSource || MoveingSource) return;
        Debug.Log(mText.text);
        if(!hasFirst){
            position1 = pos;
            mono1 = monoName;
            hasFirst = true;
        } else if(!hasSecond){
            position2 = pos;
            mono2 = monoName;
            hasSecond = true;
        } else {
            position1 = pos;
            mono1 = monoName;
            mono2 = "";
            position2 = pos;
            hasFirst = true;
            hasSecond = false;
        }
        mText.text = mono1 + (hasSecond ? " to " + mono2 : "");
    }

    void StartOnClick(){
        if(StaticSource || MoveingSource) return;
		if(hasFirst && hasSecond){
            MoveingSource = true;
            MovingMono.GetComponent<RectTransform>().anchoredPosition = position1;
            Debug.Log("start move");

            OscMessage message = new OscMessage();
            message.address = "/PlaySound";
            message.values.Add(1); // monoIndex
            message.values.Add(soundName);
            message.values.Add(1); // loop
            osc.Send(message);
        } else if(hasFirst){
            StaticSource = true;
            MovingMono.GetComponent<RectTransform>().anchoredPosition = position1;
            Debug.Log("start static");

            OscMessage message = new OscMessage();
            message.address = "/PlaySound";
            message.values.Add(1); // monoIndex
            message.values.Add(soundName);
            message.values.Add(1); // loop
            osc.Send(message);
        }
	}

    void DropdownInit(){
        m_Dropdown.ClearOptions();
        TMP_Dropdown.OptionData m_NewData;
        List<TMP_Dropdown.OptionData> m_Messages = new List<TMP_Dropdown.OptionData>();

        foreach(string sound in sounds){
            m_NewData = new TMP_Dropdown.OptionData();
            m_NewData.text = sound;
            m_Messages.Add(m_NewData);
        }
        //Take each entry in the message List
        foreach (TMP_Dropdown.OptionData message in m_Messages)
        {
            //Add each entry to the Dropdown
            m_Dropdown.options.Add(message);
            //Make the index equal to the total number of entries
        }
    }
    void DropdownValueChanged(TMP_Dropdown change)
    {
        soundName = sounds[change.value];
    }
}

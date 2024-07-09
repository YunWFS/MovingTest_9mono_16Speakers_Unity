using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class SoundStaticLRController : MonoBehaviour
{
    OSC osc;
    private Vector2 position;
    public string mono;

    public bool MoveingSource = false;
    int[] posIdx = new int[22]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11};
    private int currPosIdx = 0; 
    Vector2[] monosPos = new Vector2[12];    
    string[] sounds = new string[]{"Bird", "FemaleSpeech", "MaleSpeech"};
    string soundName = "BGM";

    // public Vector3 MovingPosition;

    TextMeshProUGUI mText;
    
    Button startBtn;
    TextMeshProUGUI startBtnText;
    TMP_Dropdown m_Dropdown;

    GameObject MovingMono;

    float moveTime;
    float moving = 0.0f;
    string path = "Assets/Resources/";
    int round = 0;
    // Start is called before the first frame update
    void Start()
    {
        osc = GameObject.Find("Osc").GetComponent<OSC>();
        moveTime = 8.0f;
        moving = 0.0f;
        mono = "";
        // mono2 = "";
        soundName = sounds[1];

        MovingMono = GameObject.Find("MovingMono");
        MovingMono.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 0.0f);
        
        mText = GameObject.Find("MonoInfo").GetComponent<TextMeshProUGUI>();
        startBtn = GameObject.Find("StartButton").GetComponent<Button>();
        startBtn.onClick.AddListener(StartOnClick);
        startBtnText = GameObject.Find("StartButton").transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        
        m_Dropdown = GameObject.Find("SelectSound").GetComponent<TMP_Dropdown>();
        DropdownInit();
        m_Dropdown.value = 1;
        m_Dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(m_Dropdown);
        });
        
        for(int i = 1; i < 12; ++i) {
            monosPos[i] = GameObject.Find("Mono" + i).GetComponent<RectTransform>().anchoredPosition;
            // Debug.Log(monosPos[i]);
        }
        currPosIdx = posIdx.Length;
        // TestPathInit();
    }

    // Update is called once per frame
    void Update()
    {
        if(MoveingSource){
            moving += Time.deltaTime;
            
            OscMessage message = new OscMessage();
            message.address = "/UpdateXYZ";
            message.values.Add(1); // monoIndex
            message.values.Add(MovingMono.GetComponent<RectTransform>().anchoredPosition.x); // x
            message.values.Add(MovingMono.GetComponent<RectTransform>().anchoredPosition.y); // y
            message.values.Add(0); // z
            osc.Send(message);   
        }
        if(moving >= moveTime){
            MoveingSource = false;
            moving = 0.0f;

            mono = "";

            OscMessage message = new OscMessage();
            message.address = "/StopSound";
            message.values.Add(1); // monoIndex
            osc.Send(message);
        }
    }

    void StartOnClick(){
        if(MoveingSource) return;
        
        if(currPosIdx == 0){
            startBtnText.text = "Next";
        } else if(currPosIdx == posIdx.Length -1){
            startBtnText.text = "Restart";
        } else if(currPosIdx > posIdx.Length -1){
            mText.text = "Start Test";
            startBtnText.text = "Start";
            TestPathInit();
            return;
        }

        mText.text = "Test index " + (currPosIdx + 1);
        position = monosPos[posIdx[currPosIdx]];
        MoveingSource = true;
        MovingMono.GetComponent<RectTransform>().anchoredPosition = position;
        Debug.Log("start play");

        OscMessage message = new OscMessage();
        message.address = "/UpdateXYZ";
        message.values.Add(1); // monoIndex
        message.values.Add(MovingMono.GetComponent<RectTransform>().anchoredPosition.x); // x
        message.values.Add(MovingMono.GetComponent<RectTransform>().anchoredPosition.y); // y
        message.values.Add(0); // z
        osc.Send(message);
        
        message = new OscMessage();
        message.address = "/PlaySound";
        message.values.Add(1); // monoIndex
        message.values.Add(soundName);
        message.values.Add(1); // loop
        osc.Send(message);
        ++currPosIdx;
	}
    void TestPathInit(){
        currPosIdx = 0;
        for (int t = 0; t < posIdx.Length; ++t){
            int tmp = posIdx[t];
            int r = Random.Range(t, posIdx.Length);
            posIdx[t] = posIdx[r];
            posIdx[r] = tmp;
        }
        StreamWriter writer = new StreamWriter(path + "SoundStaticTestLR.txt", true);
        ++round;
        writer.WriteLine("\nLeft-Right Test Case " + round + ", sound: " + soundName + "\n");

        for (int t = 0; t < posIdx.Length; ++t){
            Debug.Log("Test Case " + (t+1) + ": pos" + posIdx[t]);
            writer.WriteLine((t+1) + ": pos" + posIdx[t]);
        }
        writer.Close();
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

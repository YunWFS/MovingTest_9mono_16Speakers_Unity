using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class SoundSectorTestController : MonoBehaviour
{
    OSC osc;
    private Vector2 position1;
    private Vector2 position2;
    public string mono1;
    public string mono2;

    public bool MoveingSource = false;

    // int pathsIdx = new int[12, 2]{{ 1, 3}, { 3, 1}, { 4, 6},{ 6, 4}, { 7, 9}, { 9, 7}, 
    int numOfPath = 12;

    int radius;
    int[,] pathsIdx;
    //  = new int[12, 2]{{ 1, 3}, { 3, 1}, { 4, 6},{ 6, 4}, { 7, 9}, { 9, 7}, 
    //                                  { 1, 7}, { 7, 1}, { 2, 8}, { 8, 2}, { 3, 9}, { 9, 3}};
    // Vector2[,] pathsPos;
    private int currPathIdx = 0; 
    Vector2[] monosPos = new Vector2[10];    
    string[] sounds = new string[]{"Bird",
                                    "Violin", "Piccolo", "Oboe", "CorAnglais", "Clarinet", 
                                    "Castanets", "Glockenspiel", "FemaleSpeech", "MaleSpeech", "Piano"};
                                    
                                    // {"Bird", "BGM", 
                                    // "Violin", "Piccolo", "Oboe", "CorAnglais", "Clarinet", 
                                    // "Saxophone", "Trumpet", "Harp", "Castanets", "Triangles",
                                    // "Glockenspiel", "GrandPiano", "Harpsichord", "Celesta", "FemaleSpeech",
                                    // "MaleSpeech", "Piano"};
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
        mono1 = "";
        mono2 = "";
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
        
        for(int i = 1; i < 10; ++i) {
            monosPos[i] = GameObject.Find("Mono" + i).GetComponent<RectTransform>().anchoredPosition;
            // Debug.Log(monosPos[i]);
        }
        numOfPath = 12;
        radius = 60;
        pathsIdx = new int[12, 2]{{ 1, 3}, { 3, 1}, { 4, 6},{ 6, 4}, { 7, 9}, { 9, 7}, 
                                     { 1, 7}, { 7, 1}, { 2, 8}, { 8, 2}, { 3, 9}, { 9, 3}};
        currPathIdx = numOfPath;
        // TestPathInit();
    }

    // Update is called once per frame
    void Update()
    {
        if(MoveingSource){
            moving += Time.deltaTime;
            if(Mathf.Abs(pathsIdx[currPathIdx-1, 0] - pathsIdx[currPathIdx-1, 1]) == 2){
                float ang;
                switch(pathsIdx[currPathIdx-1, 0]){
                    case 1:
                        ang = Mathf.PI * 1 / 3 * (2 - moving / moveTime);
                        MovingMono.GetComponent<RectTransform>().anchoredPosition = new Vector2(radius * Mathf.Cos(ang), radius * Mathf.Sin(ang));
                        break;
                    case 3:
                        ang = Mathf.PI * 1 / 3 * (1 + moving / moveTime);
                        MovingMono.GetComponent<RectTransform>().anchoredPosition = new Vector2(radius * Mathf.Cos(ang), radius * Mathf.Sin(ang));
                        break;
                    case 4:
                        ang = Mathf.PI * 1 / 3 * (2 - moving / moveTime);
                        MovingMono.GetComponent<RectTransform>().anchoredPosition = new Vector2(radius * 2 * Mathf.Cos(ang), radius * 2 * Mathf.Sin(ang));
                        break;
                    case 6:
                        ang = Mathf.PI * 1 / 3 * (1 + moving / moveTime);
                        MovingMono.GetComponent<RectTransform>().anchoredPosition = new Vector2(radius * 2 * Mathf.Cos(ang), radius * 2 * Mathf.Sin(ang));
                        break;
                    case 7:
                        ang = Mathf.PI * 1 / 3 * (2 - moving / moveTime);
                        MovingMono.GetComponent<RectTransform>().anchoredPosition = new Vector2(radius * 3 * Mathf.Cos(ang), radius * 3 * Mathf.Sin(ang));
                        break;
                    case 9:
                        ang = Mathf.PI * 1 / 3 * (1 + moving / moveTime);
                        MovingMono.GetComponent<RectTransform>().anchoredPosition = new Vector2(150 * Mathf.Cos(ang), 150 * Mathf.Sin(ang));
                        break;
                }
                
            } else MovingMono.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(position1, position2, moving / moveTime);
            
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

            mono1 = "";
            mono2 = "";

            OscMessage message = new OscMessage();
            message.address = "/StopSound";
            message.values.Add(1); // monoIndex
            osc.Send(message);
        }
    }

    void StartOnClick(){
        if(MoveingSource) return;
        
        if(currPathIdx == 0){
            startBtnText.text = "Next";
        } else if(currPathIdx == numOfPath-1){
            startBtnText.text = "Restart";
        } else if(currPathIdx > numOfPath-1){
            mText.text = "Start Test";
            startBtnText.text = "Start";
            TestPathInit();
            return;
        }

        mText.text = "Test index " + (currPathIdx + 1);
        position1 = monosPos[pathsIdx[currPathIdx, 0]];
        position2 = monosPos[pathsIdx[currPathIdx, 1]];
        // Debug.Log("1: " + position1 + ",2: " + position2);
        MoveingSource = true;
        MovingMono.GetComponent<RectTransform>().anchoredPosition = position1;
        Debug.Log("start move");

        OscMessage message = new OscMessage();
        message.address = "/PlaySound";
        message.values.Add(1); // monoIndex
        message.values.Add(soundName);
        message.values.Add(1); // loop
        osc.Send(message);
        ++currPathIdx;
	}
    void TestPathInit(){
        currPathIdx = 0;
        for (int t = 0; t < numOfPath; ++t){
            int tmp0 = pathsIdx[t, 0];
            int tmp1 = pathsIdx[t, 1];
            int r = Random.Range(t, numOfPath);
            pathsIdx[t, 0] = pathsIdx[r, 0];
            pathsIdx[t, 1] = pathsIdx[r, 1];
            pathsIdx[r, 0] = tmp0;
            pathsIdx[r, 1] = tmp1;
        }
        StreamWriter writer = new StreamWriter(path + "SoundSectorTest.txt", true);
        ++round;
        writer.WriteLine("\nTest Case " + round + ", sound: " + soundName + "\n");

        for (int t = 0; t < numOfPath; ++t){
            Debug.Log((t+1) + ": (" + pathsIdx[t, 0] + ", " + pathsIdx[t, 1] + ")");
            writer.WriteLine((t+1) + ": (" + pathsIdx[t, 0] + ", " + pathsIdx[t, 1] + ")");
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

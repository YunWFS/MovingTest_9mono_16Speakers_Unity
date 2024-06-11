using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class JNDtestV2Controller : MonoBehaviour
{
    OSC osc;
    private Vector2 originPosition;
    enum playingState {None, Origin, Stop, Moved};
    playingState currState = playingState.None;
    private bool isPlaying = false;
    private int testPerDist = 5;
    bool[] TestDir;   // false = left, true = right
    int[] dist = { 80, 60, 40, 30, 20, 10};
    private int currTestDir = 0; 
    private int round = 0;
    string[] sounds = new string[]{"Bird", "BGM", 
                                    "Violin", "Piccolo", "Oboe", "CorAnglais", "Clarinet", 
                                    "Saxophone", "Trumpet", "Harp", "Castanets", "Triangles",
                                    "Glockenspiel", "GrandPiano", "Harpsichord", "Celesta", "FemaleSpeech",
                                    "MaleSpeech", "Piano"};
    string soundName = "BGM";

    TextMeshProUGUI mText;
    
    Button startBtn;
    TextMeshProUGUI startBtnText;
    TMP_Dropdown m_Dropdown;

    GameObject MovingMono;

    private float playMaxTime;
    private float stopMaxTime;
    float[] playTotalMaxTime = new float[3];
    float playingTime = 0.0f;
    string path = "Assets/Resources/";

    // Start is called before the first frame update
    void Start()
    {
        osc = GameObject.Find("Osc").GetComponent<OSC>();

        soundName = sounds[1];
        playMaxTime = 1.0f;
        stopMaxTime = 1.0f;
        
        round = 0;
        testPerDist = 5;
        TestDir = new bool[testPerDist];
        playingTime = 0.0f;
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
        originPosition = GameObject.Find("Mono5").GetComponent<RectTransform>().anchoredPosition;
        playTotalMaxTime[0] = playMaxTime;
        playTotalMaxTime[1] = playMaxTime + stopMaxTime;
        playTotalMaxTime[2] = playMaxTime + playTotalMaxTime[1];
        currState = playingState.None;
        currTestDir = 0;
        TestcaseInit();
    }

    // Update is called once per frame
    void Update()
    {
        if(isPlaying){
            playingTime += Time.deltaTime;
        }

        if(playingTime >= playTotalMaxTime[0] && currState == playingState.Origin){
            currState = playingState.Stop;
            OscMessage message = new OscMessage();
            message.address = "/StopSound";
            message.values.Add(1); // monoIndex
            osc.Send(message);
            MovingMono.GetComponent<RectTransform>().anchoredPosition = originPosition + new Vector2(dist[round] * (TestDir[currTestDir]? 1 : -1), 0);

            message = new OscMessage();
            message.address = "/UpdateXYZ";
            message.values.Add(1); // monoIndex
            message.values.Add(MovingMono.GetComponent<RectTransform>().anchoredPosition.x); // x
            message.values.Add(MovingMono.GetComponent<RectTransform>().anchoredPosition.y); // y
            message.values.Add(0); // z
            osc.Send(message); 

        } else if(playingTime >= playTotalMaxTime[1] && currState == playingState.Stop){
            currState = playingState.Moved;
            OscMessage message = new OscMessage();
            message.address = "/PlaySound";
            message.values.Add(1); // monoIndex
            message.values.Add(soundName);
            message.values.Add(1); // loop
            osc.Send(message);

        } else if(playingTime >= playTotalMaxTime[2] && currState == playingState.Moved){
            currState = playingState.None;
            OscMessage message = new OscMessage();
            message.address = "/StopSound";
            message.values.Add(1); // monoIndex
            osc.Send(message);
            ++currTestDir;
            
            MovingMono.GetComponent<RectTransform>().anchoredPosition = originPosition;
            message = new OscMessage();
            message.address = "/UpdateXYZ";
            message.values.Add(1); // monoIndex
            message.values.Add(MovingMono.GetComponent<RectTransform>().anchoredPosition.x); // x
            message.values.Add(MovingMono.GetComponent<RectTransform>().anchoredPosition.y); // y
            message.values.Add(0); // z
            osc.Send(message); 

            playingTime = 0.0f;
            isPlaying = false;
        }
    }

    void StartOnClick(){
        if(isPlaying) return;
        
        if(currTestDir == 0){
            startBtnText.text = "Next";
        } else if(currTestDir == testPerDist-1){
            startBtnText.text = "Next Test Dist.";
        } else if(currTestDir == testPerDist){
            ++round;
            mText.text = "Start Test";
            startBtnText.text = "Start";
            TestcaseInit();
            return;
        }

        mText.text = "Test dist " + dist[round] +  " test case " + (currTestDir + 1);
        MovingMono.GetComponent<RectTransform>().anchoredPosition = originPosition;

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

        isPlaying = true;
        currState = playingState.Origin;
	}
    void TestcaseInit(){
        currTestDir = 0;
        if(round >= testPerDist) round = 0;
        for (int t = 0; t < testPerDist; ++t){
            TestDir[t] = Random.Range(0, 2) == 1 ? true : false;
        }
        StreamWriter writer = new StreamWriter(path + "JNDtestV2.txt", true);
        
        writer.WriteLine("\nDist: " + dist[round] + "\n");
            
        for (int t = 0; t < testPerDist; ++t){
            Debug.Log("Dist " + dist[round] + ", testcase " + (t+1) + ": " + (TestDir[t] ? "right" : "left"));
            writer.WriteLine(t+1 + ": " + (TestDir[t] ? "right" : "left") + ", ");
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

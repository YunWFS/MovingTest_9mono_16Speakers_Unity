using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class JNDtestController : MonoBehaviour
{
    OSC osc;
    private Vector2 originPosition;
    enum playingState {None, Origin, Stop, Moved};
    playingState currState = playingState.None;
    private bool isPlaying = false;
    bool[] TestDir = new bool[15];   // false = left, true = right
    int[] dist = { 0, 0, 0, 10, 10, 10, 20, 20, 20, 30, 30, 30, 50, 50, 50};
    private int currTestIdx = 0; 
    string[] sounds = new string[]{"Bird", "BGM", "EX",
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

    int round = 0;
    // Start is called before the first frame update
    void Start()
    {
        osc = GameObject.Find("Osc").GetComponent<OSC>();

        soundName = sounds[1];
        playMaxTime = 8.0f;
        stopMaxTime = 2.0f;
        
        round = 0;
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
            
            MovingMono.GetComponent<RectTransform>().anchoredPosition = originPosition + new Vector2(dist[currTestIdx] * (TestDir[currTestIdx]? 1 : -1), 0);
            
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
            ++currTestIdx;
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
        
        if(currTestIdx == 0){
            startBtnText.text = "Next";
        } else if(currTestIdx == 14){
            startBtnText.text = "Restart";
        } else if(currTestIdx > 14){
            mText.text = "Start Test";
            startBtnText.text = "Start";
            TestcaseInit();
            return;
        }

        mText.text = "Test index " + (currTestIdx + 1);
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
        currTestIdx = 0;
        for (int t = 0; t < 15; ++t){
            int tmp = dist[t];
            int r = Random.Range(t, 15);
            dist[t] = dist[r];
            dist[r] = tmp;
            TestDir[t] = Random.Range(0, 2) == 1 ? true : false;
        }
        StreamWriter writer = new StreamWriter(path + "JNDtestV1.txt", true);
        
        ++round; 
        writer.WriteLine("\nTest Case " + round + "\n");
            
        for (int t = 0; t < 15; ++t){
            Debug.Log(t+1 + ": (" + dist[t] + ", " + (TestDir[t] ? "right" : "left") + ")");
            writer.WriteLine(t+1 + ": (" + dist[t] + ", " + (TestDir[t] ? "right" : "left") + ")");
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class JND9testController : MonoBehaviour
{
    OSC osc;
    private Vector2 originPosition;
    private Vector2 frontPos, backPos;
    private Vector2 leftPos, rightPos;
    enum playingState {None, Origin, Stop, Moved};
    private bool isPlaying = false;
    private int testPerDist;
    bool[] Moved = {false, false, false, false, false, true, true, true, true, true,};   // false = left, true = right
    int[] posList = {1, 2, 3, 4, 5, 6, 7, 8, 9};
    int[] dist = {30, 25, 20, 15, 10, 9, 8, 7, 6, 5};
    string[] dir2 = {"left-right", "front-back"};
    private int currTestMoved = 0; 
    string[] sounds = new string[]{"Bird", 
                                    "FemaleSpeech",
                                    "MaleSpeech"};
    string soundName = "BGM";
    int currDist;
    string currDir;
    int currPos;

    TextMeshProUGUI mText;
    
    Button startBtn;
    TextMeshProUGUI startBtnText;
    TMP_Dropdown m_Dropdown;
    TMP_Dropdown SelectPos_Dropdown;
    TMP_Dropdown SelectDist_Dropdown;
    TMP_Dropdown SelectDir_Dropdown;

    GameObject MovingMono;
    Vector2[] monosPos = new Vector2[10];   

    private float playMaxTime;
    private float movePathTime;
    private float currChangeTime;
    
    bool dir = false;
    float playingTime = 0.0f;
    string path = "Assets/Resources/";

    // Start is called before the first frame update
    void Start()
    {
        osc = GameObject.Find("Osc").GetComponent<OSC>();

        soundName = sounds[1];
        playMaxTime = 8.0f;
        movePathTime = 4.0f;
        currChangeTime = movePathTime;
        // stopMaxTime = 1.0f;
        
        testPerDist = 10;
        playingTime = 0.0f;
        MovingMono = GameObject.Find("MovingMono");
        MovingMono.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 0.0f);
        
        mText = GameObject.Find("MonoInfo").GetComponent<TextMeshProUGUI>();
        startBtn = GameObject.Find("StartButton").GetComponent<Button>();
        startBtn.onClick.AddListener(StartOnClick);
        startBtnText = GameObject.Find("StartButton").transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        
        m_Dropdown = GameObject.Find("SelectSound").GetComponent<TMP_Dropdown>();
        SelectPos_Dropdown = GameObject.Find("SelectPos").GetComponent<TMP_Dropdown>();
        SelectDist_Dropdown = GameObject.Find("SelectDist").GetComponent<TMP_Dropdown>();
        SelectDir_Dropdown = GameObject.Find("SelectDir").GetComponent<TMP_Dropdown>();
        DropdownInit();
        m_Dropdown.value = 1;
        m_Dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(m_Dropdown);
        });

        DropdownInitDist();
        SelectDist_Dropdown.value = 1;
        SelectDist_Dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChangedDist(SelectDist_Dropdown);
        });
        currDist = dist[1];

        DropdownInitDir();
        SelectDir_Dropdown.value = 1;
        SelectDir_Dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChangedDir(SelectDir_Dropdown);
        });
        currDir = dir2[1];

        DropdownInitPos();
        SelectPos_Dropdown.value = 1;
        SelectPos_Dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChangedPos(SelectPos_Dropdown);
        });
        currPos = posList[1];

        for(int i = 1; i < 10; ++i) {
            monosPos[i] = GameObject.Find("Mono" + i).GetComponent<RectTransform>().anchoredPosition;
            // Debug.Log(monosPos[i]);
        }
        originPosition = monosPos[currPos];
        frontPos = new Vector2(originPosition.x, originPosition.y - currDist);
        backPos = new Vector2(originPosition.x, originPosition.y + currDist);
        leftPos = new Vector2(originPosition.x - currDist, originPosition.y);
        rightPos = new Vector2(originPosition.x + currDist, originPosition.y);
        // currState = playingState.None;
        currTestMoved = testPerDist;
        TestcaseInit();
    }

    // Update is called once per frame
    void Update()
    {
        if(isPlaying){
            playingTime += Time.deltaTime;
        }
        if(playingTime >= playMaxTime){
            OscMessage message = new OscMessage();
            message.address = "/StopSound";
            message.values.Add(1); // monoIndex
            osc.Send(message);
            MovingMono.GetComponent<RectTransform>().anchoredPosition = originPosition;
            playingTime = 0.0f;
            currChangeTime = movePathTime;
            isPlaying = false;
            ++currTestMoved;
            m_Dropdown.enabled = true;
            SelectDist_Dropdown.enabled = true;
            SelectPos_Dropdown.enabled = true;
        } else if(isPlaying && Moved[currTestMoved]){
            if(playingTime >= currChangeTime) {
                dir = !dir;
                currChangeTime += movePathTime;
            }
            
            if(dir){
                if(currDir == "left-right") MovingMono.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(leftPos, rightPos, (currChangeTime - playingTime) / movePathTime);
                else MovingMono.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(frontPos, backPos, (currChangeTime - playingTime) / movePathTime);
            } else {
                if(currDir == "left-right") MovingMono.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(rightPos, leftPos, (currChangeTime - playingTime) / movePathTime);
                else MovingMono.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(backPos, frontPos, (currChangeTime - playingTime) / movePathTime);
            } 
            OscMessage message = new OscMessage();
            message.address = "/UpdateXYZ";
            message.values.Add(1); // monoIndex
            message.values.Add(MovingMono.GetComponent<RectTransform>().anchoredPosition.x); // x
            message.values.Add(MovingMono.GetComponent<RectTransform>().anchoredPosition.y); // y
            message.values.Add(0); // z
            osc.Send(message); 
        }
    }

    void StartOnClick(){
        if(isPlaying) return;
        
        dir = Random.Range(0, 2) >= 1 ? true : false;
        if(currTestMoved == 0){
            startBtnText.text = "Next";
        } else if(currTestMoved == testPerDist-1){
            startBtnText.text = "Next Test Dist.";
        } else if(currTestMoved == testPerDist){
            // ++round;
            mText.text = "Start Test";
            startBtnText.text = "Start";
            TestcaseInit();
            return;
        }

        mText.text = "Test dist " + currDist + ", test dir " + currDir + ", test pos " + currPos + " test case " + (currTestMoved + 1);
        MovingMono.GetComponent<RectTransform>().anchoredPosition = monosPos[currPos];
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
        m_Dropdown.enabled = false;
        SelectDist_Dropdown.enabled = false;
        SelectPos_Dropdown.enabled = false;
	}
    void TestcaseInit(){
        currTestMoved = 0;
        // if(round >= testPerDist) round = 0;
        for (int t = 0; t < testPerDist; ++t){
            bool temp = Moved[t];
            int r = Random.Range(t, testPerDist);
            Moved[t] = Moved[r];
            Moved[r] = temp;
        }
        originPosition = monosPos[currPos];
        frontPos = new Vector2(originPosition.x, originPosition.y - currDist);
        backPos = new Vector2(originPosition.x, originPosition.y + currDist);
        leftPos = new Vector2(originPosition.x - currDist, originPosition.y);
        rightPos = new Vector2(originPosition.x + currDist, originPosition.y);
        StreamWriter writer = new StreamWriter(path + "JND9test.txt", true);
        
        writer.WriteLine("\nPos" + currPos + ", Dist: " + currDist + ", Direction: " + currDir + "\n");
            
        for (int t = 0; t < testPerDist; ++t){
            Debug.Log("Dist " + currDist + ", Pos" + currPos + ", testcase " + (t+1) + ": " + (Moved[t] ? "moving" : "static"));
            writer.WriteLine(t+1 + ": " + (Moved[t] ? "moving" : "static") + ", ");
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

    void DropdownInitPos(){
        SelectPos_Dropdown.ClearOptions();
        TMP_Dropdown.OptionData m_NewData;
        List<TMP_Dropdown.OptionData> m_Messages = new List<TMP_Dropdown.OptionData>();

        foreach(int p in posList){
            m_NewData = new TMP_Dropdown.OptionData();
            m_NewData.text = p.ToString();
            m_Messages.Add(m_NewData);
        }
        //Take each entry in the message List
        foreach (TMP_Dropdown.OptionData message in m_Messages)
        {
            //Add each entry to the Dropdown
            SelectPos_Dropdown.options.Add(message);
            //Make the index equal to the total number of entries
        }
    }
    void DropdownValueChangedPos(TMP_Dropdown change)
    {
        currPos = posList[change.value];
        Debug.Log("currPos: " + currPos);
        TestcaseInit();
    }

    void DropdownInitDist(){
        Debug.Log("DropdownInitDist");
        SelectDist_Dropdown.ClearOptions();
        TMP_Dropdown.OptionData m_NewData;
        List<TMP_Dropdown.OptionData> m_Messages = new List<TMP_Dropdown.OptionData>();

        foreach(int d in dist){
            m_NewData = new TMP_Dropdown.OptionData();
            m_NewData.text = d.ToString();
            m_Messages.Add(m_NewData);
        }
        //Take each entry in the message List
        foreach (TMP_Dropdown.OptionData message in m_Messages)
        {
            //Add each entry to the Dropdown
            SelectDist_Dropdown.options.Add(message);
            //Make the index equal to the total number of entries
        }
    }
    void DropdownValueChangedDist(TMP_Dropdown change)
    {
        currDist = dist[change.value];
        TestcaseInit();
    }

    void DropdownInitDir(){
        // Debug.Log("DropdownInitDir");
        SelectDir_Dropdown.ClearOptions();
        TMP_Dropdown.OptionData m_NewData;
        List<TMP_Dropdown.OptionData> m_Messages = new List<TMP_Dropdown.OptionData>();

        foreach(string d in dir2){
            m_NewData = new TMP_Dropdown.OptionData();
            m_NewData.text = d.ToString();
            m_Messages.Add(m_NewData);
        }
        //Take each entry in the message List
        foreach (TMP_Dropdown.OptionData message in m_Messages)
        {
            //Add each entry to the Dropdown
            SelectDir_Dropdown.options.Add(message);
            //Make the index equal to the total number of entries
        }
    }
    void DropdownValueChangedDir(TMP_Dropdown change)
    {
        currDir = dir2[change.value];
        TestcaseInit();
    }
}

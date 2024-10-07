using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System;

/// Distributed by Richard Lemarchand for CTIN-532, with special thanks to original authors Riley Pietsch, Jung-Ho Sohn, 
/// Mariana Cacique, Chen Qian, and Rong Deng.
///
/// <summary>
/// This script is supposed to collect metrics from the game. Attach it to an empty object in your first scene.
/// It considers 2 types of variable collection:
/// Event based - to collect when something happens in the game. E.g. when the playerCharacter picks an option, or when they defeat a boss.
/// Time based - to collect in fixed time intervals. EX: Every 5 minutes, collect the playerCharacter’s health.
/// The main difference is when it makes sense to collect certain variables.
/// </summary>
/// 
/// <remarks>
/// To add Event based collecting for a variable, call the LogString function from the appropriate code. Like that:
/// if (MetricManagerScript.instance != null)
///            {
///                MetricManagerScript.instance.LogString("Variable name", "Data Point");
///            }
///Remember to never collect a variable on every frame
///
///To add Time based collecting, create a CollectData function in the appropriate script that contains the calls to the LogString function. 
///Then, on Start, subscribe the Collect Data function to MetricsEvents.OnDataCollect. Like that:
///void Start(){
///         MetricsEvents.OnDataCollect += this.CollectData;
///         }
///public void CollectData(){
///     if (MetricManagerScript.instance != null){
///         MetricManagerScript.instance.LogString("Variable name", "Data Point");
///         }
/// }
/// </remarks>


public class MetricManagerScript : MonoBehaviour
{
    public static MetricManagerScript instance;

    //timers 
    [SerializeField]
    [Tooltip("How many seconds between collecting timed variables")]
    float _intervalBetweenTimedVariables = 5.0f;
    [SerializeField]
    [Tooltip("How many seconds between writing to the file")]
    float _intervalBetweenWritingToFile = 60.0f;
    [SerializeField]
    [Tooltip("How many seconds between writing to the file")]
    bool _collectDataOnEditor = false;
    [Tooltip("Debug metrics entry on the editor as well?")]
    bool _DebugOnEditor = false;

    float _variableCollectTimer;
    float _writeToFileTimer;

    StringBuilder _dataToWriteToFile;
    string _fileName;
    string _timeStamp; //the file name will have the time stamp of when the playerCharacter started playing
    FileInfo _file;
    StreamWriter _fileStreamWriter;
    bool ShouldCollectData
    {
        get { return !Application.isEditor || _collectDataOnEditor; }
    }

    void Start()
    {

        if (ShouldCollectData) //Collect on build, only collect on Editor if _collectDataOnEditor is true. This is to avoid trash files
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(instance.gameObject);
            }

            //define first line of the csv file
            _dataToWriteToFile = new StringBuilder();
            _dataToWriteToFile.Append("Variable name,Time in Play,Data Point"); //separate the title for each collumn with a comma
            string tempLine = Environment.NewLine;
            _dataToWriteToFile.Append(tempLine);

            //get timestamp for file name 
            _timeStamp = DateTime.Now.ToString();
            _timeStamp = _timeStamp.Replace("/", "-");
            _timeStamp = _timeStamp.Replace(" ", "_");
            _timeStamp = _timeStamp.Replace(":", "-");

            string tempDevice = SystemInfo.deviceName;

            _fileName = "DeepWorld_Metrics_" + _timeStamp + "_" + tempDevice + ".csv";
            string pathName = "./";
            if (Application.isEditor)
            {
               pathName = "./Assets/Metrics/";

            }

            try
            {
                _fileStreamWriter = File.CreateText(pathName + _fileName);
            }
            catch
            {
                Debug.Log("Could not create telemetry file " + pathName + _fileName);
            } //use try and catch to avoid errors if the file path doesn't exist
        }
    }

    void Update()
    {
        if (ShouldCollectData)
        {
            //update timers here
            _variableCollectTimer += Time.deltaTime;
            _writeToFileTimer += Time.deltaTime;

            //then active timed functions
            if (_variableCollectTimer >= _intervalBetweenTimedVariables)
            {
                MetricsEvents.CollectTimedData();
                _variableCollectTimer = 0f;
            }

            if (_writeToFileTimer >= _intervalBetweenWritingToFile)
            {
                WriteToFile();
                _writeToFileTimer = 0;
            }
        }
    }
    private void OnApplicationQuit()
    {
        if (ShouldCollectData)
        {
            //To collect the final state of the game and also know how long the playerCharacter played
            MetricsEvents.CollectTimedData();
            WriteToFile();
            if (_fileStreamWriter != null)
            {
                _fileStreamWriter.Close();
                _fileStreamWriter = null;
            }
        }
    }
    void WriteToFile()//call it every XX minutes (tunable)
    {
        if (_fileStreamWriter != null)
        {
            _fileStreamWriter.Write(_dataToWriteToFile.ToString());
        }
        _dataToWriteToFile.Length = 0;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    //function to log different kinds of data. If data is not a string, use .ToString()
    //if you want to see all the places in code that are collecting data, right click on this function and click on "Find all references"
    public void LogString(string variableName, string dataToLog)
    {
        float timeInSeconds = Time.timeSinceLevelLoad;

        float secondsDisplay = timeInSeconds % 60;
        float minutesDisplay = Mathf.Floor((timeInSeconds / 60));
        string dataLine = variableName + "," + minutesDisplay.ToString() + ":" + secondsDisplay.ToString("F2") + "," + dataToLog.ToString() + Environment.NewLine;

        _dataToWriteToFile.Append(dataLine);
        if(_DebugOnEditor)
            Debug.Log("Metric saved: " + variableName + " - " + dataToLog);
    }

    public void LogChaseMode(bool chaseBegins)
    {
        /*if(chaseBegins && GameManager.instance.NumMonstersChasing == 1)
        {
            LogString("Chase", "Begins");
        }
        else if ( !chaseBegins && GameManager.instance.NumMonstersChasing == 0)
        {
            LogString("Chase", "Ends");
        }*/
    }

}

public static class MetricsEvents
{
    public delegate void MetricsEventHandler();

    public static event MetricsEventHandler OnDataCollect;

    public static void CollectTimedData()
    {
        if (OnDataCollect != null && MetricManagerScript.instance != null)
        {
            OnDataCollect();
        }
    }
}
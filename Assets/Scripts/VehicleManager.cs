using System.Collections;
using UnityEngine;
using System.Data.SqlClient;
using System.IO;
using TMPro;
using UnityEngine.UI;

public class VehicleManager : MonoBehaviour
{
    [SerializeField] private Material roadMat;
    [SerializeField] private GameObject paver;
    [SerializeField] private GameObject roller;
    [SerializeField] private Transform milestoneParent;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text progressText;

    private SqlConnection connection;
    private SqlCommand fetchQuery;
    private SqlCommand updateQuery;
    private string sql;
    private float prevLat;
    private float prevLong;
    [SerializeField] private bool firstFetch = true;
    [SerializeField] private bool paverMoving = false;
    [SerializeField] private bool rollerMoving = false;
    [SerializeField] private float progress = 0f;
    [SerializeField] private System.DateTime currentTime;
    [SerializeField] private System.DateTime initialTime;
    [SerializeField] private System.TimeSpan duration;
    private float idleTime = 0f;
    private System.Globalization.CultureInfo timeFormat = new System.Globalization.CultureInfo("en-US");
    private SqlDataReader reader;
    private bool simulation = false;
    void Awake()
    {
        //CreateMilestones();
        //string json = File.ReadAllText(Application.dataPath + "\\config.json"); //Path to json file containing database name,user name, password and other configurations.
        //DatabaseConfiguration loadedConfig = JsonUtility.FromJson<DatabaseConfiguration>(json);
        //try
        //{
        //    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
        //    builder.DataSource = loadedConfig.Server;
        //    builder.UserID = loadedConfig.UserID;
        //    builder.Password = loadedConfig.Password;
        //    builder.InitialCatalog = loadedConfig.Database;
        //    builder.MultipleActiveResultSets = true;

        //    connection = new SqlConnection(builder.ConnectionString);
        //    Debug.Log("Database Connected");
        //}
        //catch (SqlException e)
        //{
        //    Debug.Log(e.ToString());
        //}
        //try
        //{
        //    float xLoc = float.Parse(File.ReadAllText(Application.dataPath + "\\PaverLoc.txt"));
        //    paver.transform.position = new Vector3(xLoc, paver.transform.position.y, paver.transform.position.z);
        //}
        //catch
        //{
        //    paver.transform.position = new Vector3(0, paver.transform.position.y, paver.transform.position.z);
        //}
        //roadMat.SetFloat("Vector1_D07200D5", paver.transform.position.x + 7);
        //roller.transform.position = new Vector3(paver.transform.position.x, roller.transform.position.y, roller.transform.position.z);
    }

    //public void StopSimulation()
    //{
    //    if (simulation)
    //    {
    //        Debug.Log("Stopping Simulation ...");
    //        StopAllCoroutines();
    //        string query = "INSERT INTO [dbo].[dynamicmodelhistory] VALUES('";
    //        for (int i = 0; i < 10; i++)
    //        {
    //            if (i == 5)
    //            {
    //                query += "Paving','";
    //            }
    //            else
    //            {
    //                query += reader[i].ToString() + "','";
    //            }
    //        }
    //        query += reader[10].ToString() + "')";
    //        updateQuery = new SqlCommand(query, connection);
    //        updateQuery.ExecuteReaderAsync();
    //        reader.Close();
    //        connection.Close();
    //        StreamWriter stream = new StreamWriter(Application.dataPath + "\\PaverLoc.txt", false);
    //        stream.WriteLine((paver.transform.position.x).ToString());
    //        stream.Close();
    //        paverMoving = false;
    //        rollerMoving = false;
    //        simulation = false;
    //    }
    //}

    //public void StartSimulation()
    //{
    //    if (!simulation)
    //    {
    //        Debug.Log("Starting Simulation ...");
    //        connection.Open();
    //        sql = "SELECT * FROM [dbo].[dynamicmodel]";
    //        fetchQuery = new SqlCommand(sql, connection);
    //        simulation = true;
    //        StartCoroutine(FetchData());
    //    }
    //}
    private void CreateMilestones()
    {
        GameObject milestoneObject = new GameObject("Milestone");
        TextMeshPro text = milestoneObject.AddComponent<TextMeshPro>();
        text.fontSize = 20;
        text.alignment = TextAlignmentOptions.Center;
        milestoneObject.transform.eulerAngles = new Vector3(0f, 0f, 0f);
        for (int i = 0; i < 100; i += 50)
        {
            text.text = i.ToString();
            Instantiate(milestoneObject, new Vector3(8f + i, 5f, 10f), milestoneObject.transform.rotation, milestoneParent);
        }
        Destroy(milestoneObject);
    }


    //private IEnumerator MovePaver(float x)
    //{
    //    paverMoving = true;
    //    Vector3 m_initpos = paver.transform.position;
    //    float m_time = 0f;
    //    Vector3 m_target = new Vector3(m_initpos.x + x, m_initpos.y, m_initpos.z);
    //    while (m_time < 1f)
    //    {
    //        m_time += Time.deltaTime / 15f;
    //        paver.transform.position = Vector3.Lerp(m_initpos, m_target, m_time);
    //        roadMat.SetFloat("Vector1_D07200D5", paver.transform.position.x + 7);
    //        yield return null;
    //    }
    //    paverMoving = false;
    //}

    //private IEnumerator MoveRoller(Vector3 start, Vector3 end, int rounds = 1)
    //{

    //    float m_time = 0f;
    //    while (m_time < 1f)
    //    {
    //        m_time += Time.deltaTime / 5f;
    //        roller.transform.position = Vector3.Lerp(start, end, m_time);
    //        if (rounds == 1)
    //        {
    //            yield return null;
    //        }
    //        else
    //        {
    //            yield return StartCoroutine(MoveRoller(end, start, rounds - 1));
    //        }
    //    }
    //   if(rounds == 3)
    //    {
    //        rollerMoving = false;
    //    }
    //}

    //private void RollerShouldMove()
    //{
    //    rollerMoving = true;
    //    Vector3 m_rollerCurrentPos = roller.transform.position;
    //    Vector3 m_paverCurrentPos = paver.transform.position;
    //    StartCoroutine(MoveRoller(m_rollerCurrentPos, new Vector3(m_paverCurrentPos.x, m_rollerCurrentPos.y, m_rollerCurrentPos.z), 3));
    //}

    //private void PaverShouldMove()
    //{
    //    float lat1 = Mathf.Deg2Rad * (float.Parse(reader[0].ToString()));
    //    Debug.Log("Degree lat = " + reader[0]);
    //    Debug.Log("Radian lat = " + lat1);
    //    float lat2 = Mathf.Deg2Rad * prevLat;
    //    float latDiff = lat1 - lat2;
    //    float long1 = Mathf.Deg2Rad * (float.Parse(reader[1].ToString()));
    //    float long2 = Mathf.Deg2Rad * prevLong;
    //    float longDiff = long1 - long2;
    //    float angle = Mathf.Pow(Mathf.Sin(latDiff / 2f), 2f) + Mathf.Cos(lat2) * Mathf.Cos(lat1) * Mathf.Pow(Mathf.Sin(longDiff / 2f), 2f);
    //    float distance = 6373f * (2f * Mathf.Atan2(Mathf.Sqrt(angle), Mathf.Sqrt(1f - angle))) * 1000f;
    //    Debug.Log("Distance = " + distance);
    //    if (Mathf.Approximately(distance, 0f))
    //    {
    //        return;
    //    }
    //    StartCoroutine(MovePaver(distance));
    //    prevLat = float.Parse(reader[0].ToString());
    //    prevLong = float.Parse(reader[1].ToString());
    //    progress += distance;
    //}

    //private IEnumerator FetchData()
    //{
    //    while (true)
    //    {
    //        if (reader != null)
    //        { 
    //            reader.Close();
    //        }
    //        Debug.Log("Fetching Data...");
    //        reader = fetchQuery.ExecuteReader();

    //            if (reader.Read())
    //            {
    //                if (firstFetch)
    //                {
    //                    prevLat = float.Parse(reader[0].ToString());
    //                    prevLong = float.Parse(reader[1].ToString());
    //                    firstFetch = false;
    //                    initialTime = System.Convert.ToDateTime(reader[7].ToString(), timeFormat);
    //                    Debug.Log("Initial Time = " + initialTime);
    //                    progress = float.Parse(reader[8].ToString());
    //                }
    //                currentTime = System.Convert.ToDateTime(reader[7].ToString(), timeFormat);
    //                Debug.Log("Current Time = " + currentTime);
    //                if (reader[5].ToString() == "Asphalt Paver")
    //                {
    //                    Debug.Log("Paver should move.");
    //                    if (!paverMoving)
    //                    {
    //                        Debug.Log("Paver Moving...");
    //                        PaverShouldMove();
    //                    }
    //                }
    //                else if (reader[5].ToString() == "Asphalt Roller")
    //                {
    //                    idleTime += 15f;

    //                    if (!rollerMoving)
    //                    {
    //                        Debug.Log("Roller Moving...");
    //                        RollerShouldMove();
    //                    }
    //                }
    //            }
    //        duration = currentTime - initialTime;
    //        Debug.Log("Idle time in minutes = "+(idleTime / 60f));
    //        Debug.Log("Duration in minutes = "+duration.TotalMinutes);
    //        updateQuery = new SqlCommand("UPDATE [dbo].[dynamicmodel] SET Progress='" + progress.ToString() + "',Idle_Time='"+ (idleTime / 60f)+ "',Duration='" + duration.TotalMinutes + "'", connection);
    //        updateQuery.ExecuteReader();
    //        progressSlider.value = progress * 0.001f;
    //        progressText.text = (Mathf.Round(progressSlider.value * 10000f)*0.01f).ToString() + "%";
    //        yield return new WaitForSeconds(15f);
    //    }
    //}

    //private class DatabaseConfiguration
    //{
    //    public string Database;
    //    public string UserID;
    //    public string Password;
    //    public string Server;
    //}

    private void Update()
    {
        roadMat.SetFloat("Vector1_D07200D5", paver.transform.position.x + 7);
    }
}

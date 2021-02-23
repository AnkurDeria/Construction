using System.Collections;
using UnityEngine;
using System.Data.SqlClient;
using System.IO;
using TMPro;

public class VehicleManager : MonoBehaviour
{
    [SerializeField] private Material roadMat;
    [SerializeField] private GameObject paver;
    [SerializeField] private GameObject roller;
    [SerializeField] private Transform milestoneParent;

    private SqlConnection connection;
    private SqlCommand fetchQuery;
    private SqlCommand updateQuery;
    private string sql;
    private float prevLat;
    private float prevLong;
    private bool firstFetch = true;
    private bool paverMoving = false;
    private bool rollerMoving = false;
    private float progress = 0f;
    private System.DateTime currentTime;
    private System.DateTime initialTime;
    private System.TimeSpan duration;
    private float idleTime = 0f;
    private System.Globalization.CultureInfo timeFormat = new System.Globalization.CultureInfo("en-US");
    private SqlDataReader reader;
    void Awake()
    {
        CreateMilestones();
        string json = File.ReadAllText(Application.dataPath + "\\config.json"); //Path to json file containing database name,user name, password and other configurations.
        DatabaseConfiguration loadedConfig = JsonUtility.FromJson<DatabaseConfiguration>(json);
        try
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = loadedConfig.Server;
            builder.UserID = loadedConfig.UserID;
            builder.Password = loadedConfig.Password;
            builder.InitialCatalog = loadedConfig.Database;
            builder.MultipleActiveResultSets = true;

            connection = new SqlConnection(builder.ConnectionString);
            Debug.Log("Database Connected");
        }
        catch (SqlException e)
        {
            Debug.Log(e.ToString());
        }
    }

    public void StopSimulation()
    {
        Debug.Log("Stopping Simulation ...");
        StopAllCoroutines();
        string query = "INSERT INTO [dbo].[dynamicmodelhistory] VALUES('";
        for(int i = 0; i < 10; i++)
        {
            query += reader[i].ToString() + "','";
        }
        query += reader[10].ToString() + "')";
        updateQuery = new SqlCommand(query, connection);
        updateQuery.ExecuteReaderAsync();
        reader.Close();
        connection.Close();
        paverMoving = false;
        rollerMoving = false;
    }

    public void StartSimulation()
    {
        Debug.Log("Starting Simulation ...");
        connection.Open();
        sql = "SELECT * FROM [dbo].[dynamicmodel]";
        fetchQuery = new SqlCommand(sql, connection);
        StartCoroutine(FetchData());
    }
    private void CreateMilestones()
    {
        GameObject milestoneObject = new GameObject("Milestone");
        TextMeshPro text = milestoneObject.AddComponent<TextMeshPro>();
        text.fontSize = 20;
        text.alignment = TextAlignmentOptions.Center;
        milestoneObject.transform.eulerAngles = new Vector3(0f, 180f, 0f);
        for (int i = 0; i < 1001; i += 50)
        {
            text.text = i.ToString();
            Instantiate(milestoneObject, new Vector3(8f+i, 5f, 0f), milestoneObject.transform.rotation, milestoneParent);
        }
        Destroy(milestoneObject);
    }
 

    private IEnumerator MovePaver(float x)
    {
        paverMoving = true;
        Vector3 m_initpos = paver.transform.position;
        float m_time = 0f;
        Vector3 m_target = new Vector3(m_initpos.x + x, m_initpos.y, m_initpos.z);
        while (paver.transform.position.x < (m_initpos.x + x))
        {
            m_time += Time.deltaTime / 15f;
            paver.transform.position = Vector3.Lerp(m_initpos, m_target, m_time);
            yield return null;
        }
        paverMoving = false;
    }

    private IEnumerator MoveRoller(Vector3 start, Vector3 end, int rounds = 1)
    {
        
        float m_time = 0f;
        while (roller.transform.position.x < end.x)
        {
            m_time += Time.deltaTime / 5f;
            roller.transform.position = Vector3.Lerp(start, end, m_time);
            if (rounds == 1)
            {
                yield return null;
            }
            else
            {
                yield return StartCoroutine(MoveRoller(end, start, rounds - 1));
            }
        }
       if(rounds == 3)
        {
            rollerMoving = false;
        }
    }

    private void RollerShouldMove()
    {
        rollerMoving = true;
        Vector3 m_rollerCurrentPos = roller.transform.position;
        Vector3 m_paverCurrentPos = paver.transform.position;
        StartCoroutine(MoveRoller(m_rollerCurrentPos, new Vector3(m_paverCurrentPos.x, m_rollerCurrentPos.y, m_rollerCurrentPos.z), 3));
    }

    private void PaverShouldMove()
    {
        float lat1 = Mathf.Deg2Rad * (float.Parse(reader[0].ToString()));
        Debug.Log("Degree lat = " + reader[0]);
        Debug.Log("Radian lat = " + lat1);
        float lat2 = Mathf.Deg2Rad * prevLat;
        float latDiff = lat1 - lat2;
        float long1 = Mathf.Deg2Rad * (float.Parse(reader[1].ToString()));
        float long2 = Mathf.Deg2Rad * prevLong;
        float longDiff = long1 - long2;
        float angle = Mathf.Pow(Mathf.Sin(latDiff / 2f), 2f) + Mathf.Cos(lat2) * Mathf.Cos(lat1) * Mathf.Pow(Mathf.Sin(longDiff / 2f), 2f);
        float distance = 6373f * (2f * Mathf.Atan2(Mathf.Sqrt(angle), Mathf.Sqrt(1f - angle)));
        if (Mathf.Approximately(distance, 0f))
        {
            return;
        }
        StartCoroutine(MovePaver(distance));
        
        prevLat = float.Parse(reader[0].ToString());
        prevLong = float.Parse(reader[1].ToString());
        progress += distance;
    }

    private IEnumerator FetchData()
    {
        while (true)
        {
            if (reader != null)
            { 
                reader.Close();
            }
            reader = fetchQuery.ExecuteReader();
            
                if (reader.Read())
                {
                    if (firstFetch)
                    {
                        prevLat = float.Parse(reader[0].ToString());
                        prevLong = float.Parse(reader[1].ToString());
                        firstFetch = false;
                        initialTime = System.Convert.ToDateTime(reader[7].ToString(), timeFormat);
                        progress = float.Parse(reader[8].ToString());
                    }
                    currentTime = System.Convert.ToDateTime(reader[7].ToString(), timeFormat);
                    if (reader[5].ToString() == "Asphalt Paver")
                    {
                        
                        if (!paverMoving)
                        {
                            Debug.Log("Paver Moving...");
                            PaverShouldMove();
                        }
                    }
                    else if (reader[5].ToString() == "Asphalt Roller")
                    {
                        idleTime += 15f;
                        
                        if (!rollerMoving)
                        {
                            Debug.Log("Roller Moving...");
                            RollerShouldMove();
                        }
                    }
                }
            
            updateQuery = new SqlCommand("UPDATE [dbo].[dynamicmodel] SET Progress='" + progress.ToString() + "',Idle_Time='"+ (idleTime / 60f).ToString() + "',Duration='" + (duration.TotalMinutes).ToString()+"'", connection);
            updateQuery.ExecuteReaderAsync();
            yield return new WaitForSeconds(15f);
        }
    }
    


    private class DatabaseConfiguration
    {
        public string Database;
        public string UserID;
        public string Password;
        public string Server;
    }
}

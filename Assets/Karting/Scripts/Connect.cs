using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

public class Connect : MonoBehaviour
{

    static private Connect S;
    public static string firstName;
    public static string secondName;

    void Start()
    {
        Debug.Log("------------");

    }


    public void Click(bool active)
    {
        Debug.Log("------------");
        Network.PostData("bybyn", new Vector3(0, 1, 2), new Vector3(0, 1, 2));
        Network.GetData("bybyn");
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}

public class Network
{
    public static async Task<PositionCollider> GetData(string nick)
    {
        string url = "https://fastdostavka.ga/api/CarsConroller/"+nick;
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";
        var webResponse = request.GetResponse();
        var webStream = webResponse.GetResponseStream();
        var responseReader = new StreamReader(webStream);
        string response = responseReader.ReadToEnd();
        Debug.Log("------------" + response);
        PositionCollider pc = JsonConvert.DeserializeObject<PositionCollider>(response);
        responseReader.Close();
        return pc;
    }

    public static void PostData(string nick, Vector3 pos, Vector3 velocity)
    {
        var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://fastdostavka.ga/api/CarsConroller");
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";
        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            PositionCollider pc = new PositionCollider
            {
                Nick = nick,
                pos = new PosVextor3 { X = pos.x, Y = pos.y, Z = pos.z },
                velocity = new PosVextor3 { X = velocity.x, Y = velocity.y, Z = velocity.z }
            };
            string json = JsonConvert.SerializeObject(pc);
            streamWriter.Write(json);
        }
        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            var result = streamReader.ReadToEnd();
        }
    }
}

public class PositionCollider
{
    public string Nick { get; set; }
    public PosVextor3 pos { get; set; }
    public PosVextor3 velocity { get; set; }

}

public class PosVextor3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Text;
using UnityEngine.UI;

public class Connect : MonoBehaviour
{

    static private Connect S;
    public static string firstName;
    public static string secondName;

    void Start()
    {
        Debug.Log("------------");

    }


    public Text InputMyName;
    public void Click(bool active)
    {
        //then drag and drop the Username_field
      

        IPAddress ip = IPAddress.Parse("95.214.10.36");//IPAddress.Parse("127.0.0.1"); //Dns.GetHostAddresses("google.com.ua")[0];
        IPEndPoint ep = new IPEndPoint(ip, 560);
        Socket s = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.IP);
        try
        {
            s.Connect(ep);
            if (s.Connected)
            {
                string strSend = "Привіт. Я debil. ya kablan\r\n\r\n";
                s.Send(Encoding.UTF8.GetBytes(strSend));
                byte[] buffer = new byte[1024];
                int l;
                do
                {
                    l = s.Receive(buffer);
                    //txtMesssage.Text += Encoding.UTF8.GetString(buffer, 0, l);
                } while (l > 0);
                //txtMesssage.Text = "Connected good";
            }

        }
        catch (Exception ex)
        {
        }
        finally
        {
            s.Shutdown(SocketShutdown.Both);
            s.Close();
        }
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
        string url = "https://fastdostavka.ga/api/CarsConroller/" + nick;
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

    public static async Task PostData(string nick, Vector3 pos, Vector3 velocity)
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
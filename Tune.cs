using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace Tune
{
    public class TuneBlade
    {

        // TuneBlade web url
        public static string url = "http://127.0.0.1:54412";

        public static HttpClient client = new(new HttpClientHandler()
        {
            UseDefaultCredentials = true
        })
        {
            Timeout = TimeSpan.FromSeconds(1),
            BaseAddress = new Uri(url)
        };

        

        public static List<TuneDevice> GetDevices()
        {
            return client.GetFromJsonAsync<List<TuneDevice>>("devices").Result!;
        }

        public static TuneDevice GetDevice(string ID)
        {
            return GetDevices().Find(x => x.ID == ID)!;
        }


        public static bool IsWebActive()
        {
            try
            {
                _ = client.GetAsync("devices").Result;

                return true;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }


            return false;
        }

        public static Streaming GetStreaming()
        {
            return client.GetFromJsonAsync<Streaming>("StreamingMode").Result!;
        }

        public static void SetStreaming(Streaming streaming)
        {
            //client.PutAsJsonAsync("StreamingMode", streaming);

            client.PutAsync("StreamingMode", new StringContent(JsonSerializer.Serialize(streaming)));
        }


    }
    public class Streaming
    {
        public string? StreamingMode { get; set; }
        public int? BufferSize { get; set; }

    }

    public class TuneDevice
    {

        public string? ID { get; set; }
        public string? Name { get; set; }
        public int? Volume { get; set; }
        public string? Status { get; set; }
        public string? Substate { get; set; }
        public bool? Buffering { get; set; }
        public int? BufferingPercent { get; set; }

        //public TuneDevice(string ID, string Name, int Volume, string Status, string Substate, bool Buffering, int BufferingPercent)
        //{
        //    this.ID = ID;
        //    this.Name = Name;
        //    this.Volume = Volume;
        //    this.Status = Status;
        //    this.Substate = Substate;
        //    this.Buffering = Buffering;
        //    this.BufferingPercent = BufferingPercent;
        //}


        public void connect()
        {
            Status = "Connect";

            string[] arr = { "Status" };
            push(arr);
        }

        public void disconnect()
        {
            Status = "Disconnect";

            string[] arr = { "Status" };
            push(arr);
        }


        public void push()
        {
            string[] arr = { };
            push(arr);
        }

        public void push(string[] filter, bool ignore = false)
        {
            var node = JsonNode.Parse(JsonSerializer.Serialize(this))!;
            var json = node.AsObject();
            
            if (ignore)
            {
                foreach (var i in filter)
                {
                    json.Remove(i);
                }
            }
            else
            {
                // 极不优雅
                string[] arr = { "ID", "Name", "Volume", "Status", "Substate", "Buffering", "BufferingPercent" };
                foreach (var i in arr)
                {
                    if (filter.Contains(i))
                    {
                        continue;
                    }
                    json.Remove(i);
                }
            }



            //TuneBlade.client.PutAsJsonAsync("devices/" + ID, this);
            TuneBlade.client.PutAsync("devices/" + ID, new StringContent(json.ToJsonString()));
            
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

    }
}

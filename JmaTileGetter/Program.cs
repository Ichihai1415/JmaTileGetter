using System.Drawing;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JmaTileGetter
{
    [SupportedOSPlatform("windows7.0")]
    internal class Program
    {
        internal static HttpClient client = new();
        internal static string[] types = ["land", "inund", "flood_mesh"];
        internal static string[] typesJa = ["土砂災害", "浸水", "洪水"];

        /*
<land>
L1=130983
L2=83
L3=6
L4=0
L5=0

<inund>
L1=65521
L2=8
L3=4
L4=3
L5=0

<flood_mesh>
L1=131025
L2=40
L3=1
L4=6
L5=0

<flood>
L1=0
L2=0
L3=0
L4=0
L5=0

<designated_river>
L1=0
L2=0
L3=0
L4=0
L5=0

<inland_flood>
L1=0
L2=0
L3=0
L4=0
L5=0

<designated_river_nation>
L1=0
L2=0
L3=0
L4=0
L5=0

<flood_riskline>
L1=0
L2=0
L3=0
L4=0
L5=0
         */

        static void Main(string[] args)
        {
            var targetTimesUrl = "https://www.jma.go.jp/bosai/jmatile/data/risk/targetTimes.json";
            var targetTimesSt = client.GetStringAsync(targetTimesUrl).Result;
            var targetTimes = JsonSerializer.Deserialize<RiskTargetTime_single[]>(targetTimesSt);

            var z = 4;

            //types = targetTimes[0].Elements;
            foreach (var type in types)
            {
                Console.WriteLine("<" + type + ">");
                var imgs = new List<Bitmap>();
                for (var x = 13; x <= 14; x++)
                    for (var y = 5; y <= 6; y++)
                    {
                        var dataTime = targetTimes[0].Validtime;
                        //ex. https://www.jma.go.jp/bosai/jmatile/data/risk/20250710105000/immed0/20250710105000/surf/inund/4/14/6.png
                        var url = $"https://www.jma.go.jp/bosai/jmatile/data/risk/{dataTime}/immed0/{dataTime}/surf/{type}/{z}/{x}/{y}.png";
                        var imgSm = client.GetStreamAsync(url).Result;
                        imgs.Add(new Bitmap(imgSm));
                    }

                //var colors = new Dictionary<Color, int>();
                var riskCount = new RiskTileCount();

                /*ex.
    Color [A=0, R=0, G=0, B=0] : 131072  //地域外
    Color [A=0, R=255, G=255, B=255] : 131042  //発表なし(1)
    Color [A=255, R=242, G=231, B=0] : 14  //注意(2)
    Color [A=255, R=170, G=0, B=170] : 4  //危険(4)
    Color [A=255, R=255, G=40, B=0] : 8  //警戒(3)
    Color [A=255, R=12, G=0, B=12] : 4  //災害切迫(5)
                */
                foreach (var img in imgs)
                {
                    for (var x = 0; x < img.Width; x++)
                        for (var y = 0; y < img.Height; y++)
                        {
                            var c = img.GetPixel(x, y);
                            //if (!colors.TryGetValue(c, out int value))
                            //    colors.Add(c, 1);
                            //else
                            //    colors[c]++;
                            switch (c.A)
                            {
                                case 0:
                                    //switch (c.R)
                                    //{
                                    //    case 0:
                                    //        break;
                                    //    case 255:
                                    //        riskCount.L1++;
                                    //        break;
                                    //    default:
                                    //        throw new Exception("R値が不正です: " + c.R);
                                    //}
                                    break;
                                case 255:
                                    switch (c.R)
                                    {
                                        case 242:
                                            riskCount.L2++;
                                            break;
                                        case 255:
                                            riskCount.L3++;
                                            break;
                                        case 170:
                                            riskCount.L4++;
                                            break;
                                        case 12:
                                            riskCount.L5++;
                                            break;
                                        default:
                                            throw new Exception("R値が不正です: " + c.R);
                                    }
                                    break;
                                default:
                                    throw new Exception("A値が不正です: " + c.A);
                            }

                        }
                }
                //foreach (var cn in colors)
                //{
                //    Console.WriteLine(cn.Key + " : " + cn.Value);
                //}
                //Console.WriteLine("L1=" + riskCount.L1);
                Console.WriteLine("L2=" + riskCount.L2);
                Console.WriteLine("L3=" + riskCount.L3);
                Console.WriteLine("L4=" + riskCount.L4);
                Console.WriteLine("L5=" + riskCount.L5);
                Console.WriteLine();
            }
        }
    }

    // Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
    public class RiskTargetTime_single
    {
        [JsonPropertyName("basetime")]
        public required string Basetime { get; set; }

        [JsonPropertyName("validtime")]
        public required string Validtime { get; set; }

        [JsonPropertyName("member")]
        public required string Member { get; set; }

        [JsonPropertyName("elements")]
        public required string[] Elements { get; set; }
    }

    public class RiskTileCount
    {
        public int L1 { get; set; } = 0;
        public int L2 { get; set; } = 0;
        public int L3 { get; set; } = 0;
        public int L4 { get; set; } = 0;
        public int L5 { get; set; } = 0;
    }

}

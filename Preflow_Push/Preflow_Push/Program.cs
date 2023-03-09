using System;
using System.Collections.Generic;
using System.IO;

namespace Preflow_Push
{
    public class Preflow_Push
    {
        public struct Map   //儲存原始資料
        {
            public int tail, head, flow;
            public Map(int tail, int head, int flow)
            {
                this.tail = tail;
                this.head = head;
                this.flow = flow;
            }
        }
        public struct Table //儲存forward star 轉換後的資料
        {
            public int node, point;
            public Table(int node, int point)
            {
                this.node = node;
                this.point = point;
            }
        }
        public static List<Table> transfer(List<Map> map)   //進行forward star
        {
            int now = 0;
            List<Table> table = new List<Table>();

            int line = 0;
            foreach (var i in map)
            {
                line++;
                if (i.tail == now + 1)
                {
                    now = i.tail;
                    table.Add(new Table(i.tail, line));
                }
                else if (i.tail > now + 1)
                {
                    for (int j = now + 1; j < i.tail; j++)
                    {
                        table.Add(new Table(j, line));
                    }
                    table.Add(new Table(i.tail, line));
                    now = i.tail;
                }
            }
            var num = map.Count;
            table.Add(new Table(now + 1, num + 1));
            return table;
        }
        public static List<Map> load(string FileName)   //讀檔
        {
            List<Map> map = new List<Map>();
            using (StreamReader sr = File.OpenText(FileName))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    string[] a = s.Split(',');
                    map.Add(new Map(Int32.Parse(a[0]), Int32.Parse(a[1]), Int32.Parse(a[2])));
                }
            }
            return map;
        }
        public static int GetIndexOfMap(int tail, int head, List<Map> map, List<Table> table)  //取得指定節線的索引
        {
            for (int i = table[tail - 1].point - 1; i < map.Count && i >= 0 && i < table[tail].point - 1; i++)
            {
                if (map[i].head == head)
                    return i;
            }
            return -1;

        }
        public static List<Map> CreateReverseNetWork(List<Map> map) //建立雙向網路
        {
            List<Table> table = transfer(map);
            List<Map> reversedMap = new List<Map>();

            foreach (var i in map)
            {
                if (GetIndexOfMap(i.head, i.tail, map, table) == -1)//若反向路徑不存在
                    reversedMap.Add(new Map(i.head, i.tail, 0));
            }

            return reversedMap;
        }
        public static new int FindMaxFlow(List<Map> map, List<Table> table, int start, int end)//找最大流量
        {
            int node = start;
            List<Map> m = load_reverse("Philadelphia_network.csv");
            int[] d = CalculateHeight(end, start, m);//計算每個點的高度，即對終點的距離

            int mm = 0;
            for (int i = 0; i < d.Length; i++)
            {
                if (d[i] > mm)
                    mm = d[i];
            }
            Console.WriteLine("{0}", mm);

            d[start - 1] = mm + 1;//設定起點的高度

            Queue<int> q = new Queue<int>();
            int[] e = new int[table.Count];

            for (int i = table[start - 1].point - 1; i < map.Count && i >= 0 && i < table[start].point - 1; i++)//預流
            {
                if (map[i].flow <= 0)
                    continue;

                e[map[i].head - 1] = map[i].flow;

                int f = map[i].flow;
                int j;
                j = GetIndexOfMap(map[i].tail, map[i].head, map, table);
                Map tmp = map[j];
                tmp.flow -= f;
                map[j] = tmp;

                j = GetIndexOfMap(map[i].head, map[i].tail, map, table);
                tmp = map[j];
                tmp.flow += f;
                map[j] = tmp;


                q.Enqueue(map[i].head);
            }
            while (true)
            {
                int not0 = 0;
                for (int k = 0; k < e.Length; k++)  //遍歷每個點看是否還有excess
                {
                    bool relabel = true;
                    if (e[k] > 0 && k != start - 1 && k != end - 1) //若excess大於0且此點不是起點也不是終點
                    {
                        not0++;
                        for (int i = table[k].point - 1; i >= 0 && i < map.Count && i < table[k + 1].point - 1; i++)//遍歷連向的點
                        {
                            if (map[i].flow <= 0)
                                continue;
                            int head = map[i].head - 1;
                            if (d[k] == d[head] + 1)//若連向的點的高度洽比此點小1
                            {
                                relabel = false;//不relabel此點
                                int f = e[k] < map[i].flow ? e[k] : map[i].flow;//決定流量
                                e[k] -= f;
                                e[head] += f;
                                //改變雙向節線的值
                                int j;
                                j = GetIndexOfMap(map[i].tail, map[i].head, map, table);
                                Map tmp = map[j];
                                tmp.flow -= f;
                                map[j] = tmp;

                                j = GetIndexOfMap(map[i].head, map[i].tail, map, table);
                                tmp = map[j];
                                tmp.flow += f;
                                map[j] = tmp;
                            }
                        }
                        if (relabel == true)//若需要relabel
                        {
                            int min = Int32.MaxValue;
                            for (int i = table[k].point - 1; i >= 0 && i < map.Count && i < table[k + 1].point - 1; i++)
                            {

                                if (map[i].flow > 0 && min > d[map[i].head - 1])
                                {
                                    min = d[map[i].head - 1];
                                }
                            }
                            d[k] = min + 1;//將高度設為所有連向的點的高度的最小值加一
                        }

                    }
                }
                if (not0 == 0)   //當除了起點與終點其他點的excess皆為0即結束
                {
                    break;
                }
            }
            return e[end - 1];
        }
        public static List<Map> load_reverse(string FileName)   //為了計算height而在讀取檔案時將tail跟head顛倒
        {
            List<Map> map = new List<Map>();
            using (StreamReader sr = File.OpenText(FileName))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    string[] a = s.Split(',');
                    map.Add(new Map(Int32.Parse(a[1]), Int32.Parse(a[0]), Int32.Parse(a[2])));
                }
            }
            map.Sort((x, y) => { return x.tail.CompareTo(y.tail); });//排序
            return map;
        }
        public static int[] CalculateHeight(int start, int end, List<Map> map)    //計算每個點到終點的距離
        {
            List<Table> table = transfer(map);

            Queue<int> queue = new Queue<int>();

            int node = start;
            queue.Enqueue(start);

            bool[] marked = new bool[table.Count];
            for (int i = 0; i < marked.Length; i++)
            {
                marked[i] = false;
            }
            marked[start - 1] = true;

            int[] d = new int[table.Count];


            while (queue.Count != 0)
            {
                node = queue.Dequeue();

                for (int i = table[node - 1].point - 1; i < map.Count && i >= 0 && i < table[node].point - 1; i++)
                {
                    if (marked[map[i].head - 1] || map[i].flow <= 0)
                        continue;

                    d[map[i].head - 1] = d[node - 1] + 1;

                    queue.Enqueue(map[i].head);
                    marked[map[i].head - 1] = true;
                }
            }
            return d;
        }
        public new static void Main()
        {
            List<Map> map = load("Philadelphia_network.csv");
            List<Map> reversedMap = CreateReverseNetWork(map);
            List<Map> mergedMap = new List<Map>();

            mergedMap.AddRange(map);
            mergedMap.AddRange(reversedMap);
            mergedMap.Sort((x, y) => { return x.tail.CompareTo(y.tail); });//排序整個雙向網路
            
            int maxFlow = 0;

            Console.Write("start:");
            int start = Int32.Parse(Console.ReadLine());
            Console.Write("end:");
            int end = Int32.Parse(Console.ReadLine());
            Console.WriteLine();

            List<Table> mergedTable = transfer(mergedMap);

            maxFlow = FindMaxFlow(mergedMap, mergedTable, start, end);
            Console.WriteLine("max flow: {0}", maxFlow);

            Console.Read();
        }
    }
}

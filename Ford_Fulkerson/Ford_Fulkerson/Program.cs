using System;
using System.Collections.Generic;
using System.IO;

namespace Ford_Fulkerson
{
    public class Ford_Fulkerson
    {
        public struct Map   //存原始資料
        {
            public int tail, head, flow;
            public Map(int tail, int head, int flow)
            {
                this.tail = tail;
                this.head = head;
                this.flow = flow;
            }
        }
        public struct Table //存forward star轉換後的資料
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
        public static List<Map> CreateReverseNetWork(List<Map> map) //建雙向網路
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
        public static int FindMaxFlow(List<Map> map, List<Table> table, int start, int end)//找一條路徑並求出該路徑最大流
        {
            int[] previous = new int[table.Count];
            Queue<int> queue = new Queue<int>();

            int node = start;
            queue.Enqueue(start);

            bool[] marked = new bool[table.Count];
            for (int i = 0; i < marked.Length; i++)//將所有點設為未標記
            {
                marked[i] = false;
            }
            marked[start - 1] = true;

            while (queue.Count != 0)//找一條路徑
            {
                node = queue.Dequeue();
                if (node == end)
                    break;
                for (int i = table[node - 1].point - 1; i < map.Count && i >= 0 && i < table[node].point - 1; i++)
                {
                    if (marked[map[i].head - 1] || map[i].flow <= 0)//若點已被標記，忽略此點
                        continue;

                    queue.Enqueue(map[i].head);
                    marked[map[i].head - 1] = true;

                    previous[map[i].head - 1] = node;

                    if (map[i].head == end)
                        break;
                }
            }

            int n = end;
            int max = Int32.MaxValue;
            for (int i = previous[n - 1]; i > 0 && i < previous.Length; i = previous[i - 1])//找出路徑的最大流
            {
                for (int j = table[i - 1].point - 1; j < map.Count && j >= 0 && j < table[i].point - 1; j++)
                {
                    if (map[j].head == n)
                    {
                        if (map[j].flow < max)
                        {
                            max = map[j].flow;
                        }
                        break;
                    }
                }
                n = i;
            }

            Stack<int> stack = new Stack<int>();
            stack.Push(end);

            n = end;
            for (int i = previous[n - 1]; i > 0 && i < previous.Length; i = previous[i - 1])//更改雙向流的值
            {
                stack.Push(i);
                int j;
                j = GetIndexOfMap(i, n, map, table);
                Map tmp = map[j];
                tmp.flow -= max;//minus flow
                map[j] = tmp;

                j = GetIndexOfMap(n, i, map, table);
                tmp = map[j];
                tmp.flow += max;//add flow
                map[j] = tmp;

                n = i;
            }
            while (stack.Count != 0)
            {
                Console.Write("{0} ", stack.Pop());
            }
            Console.WriteLine();

            if (max == Int32.MaxValue)
                return 0;
            return max;
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
        public static void Main()
        {
            List<Map> map = load("Philadelphia_network.csv");
            List<Map> reversedMap = CreateReverseNetWork(map);
            List<Map> mergedMap = new List<Map>();

            mergedMap.AddRange(map);
            mergedMap.AddRange(reversedMap);
            mergedMap.Sort((x, y) => { return x.tail.CompareTo(y.tail); }); //對整個雙向網路排序
            
            int total = 0;
            int maxFlow = 0;

            Console.Write("start:");
            int start = Int32.Parse(Console.ReadLine());
            Console.Write("end:");
            int end = Int32.Parse(Console.ReadLine());
            Console.WriteLine();

            List<Table> mergedTable = transfer(mergedMap);

            while ((maxFlow = FindMaxFlow(mergedMap, mergedTable, start, end)) != 0)
            {
                Console.WriteLine("max flow: {0}\n", maxFlow);
                total += maxFlow;
            }
            Console.WriteLine("total = {0}", total);

            Console.Read();
        }
    }
}

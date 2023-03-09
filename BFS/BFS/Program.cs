using System;
using System.Collections.Generic;
using System.IO;

namespace BFS
{
    public class BFS
    {
        public struct Map   //存原始資料
        {
            public int tail, head;
            public Map(int tail, int head)
            {
                this.tail = tail;
                this.head = head;
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
                if (i.tail == now + 1)    //若未跳號
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
        public static List<Map> load(string FileName)   //讀檔案並存入
        {
            List<Map> map = new List<Map>();
            using (StreamReader sr = File.OpenText(FileName))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    string[] a = s.Split(',');
                    map.Add(new Map(Int32.Parse(a[0]), Int32.Parse(a[1])));
                }
            }
            return map;
        }
        public static void traverse(List<Map> map, List<Table> table, int start)    //進行BFS
        {
            Queue<int> queue = new Queue<int>();
            bool[] visited = new bool[table.Count];
            for (int i = 0; i < visited.Length; i++) //將所有點設為未拜訪
            {
                visited[i] = false;
            }
            int node = start;
            queue.Enqueue(start);
            Console.Write("{0} ", start);
            visited[start - 1] = true;//設起點為已拜訪

            while (queue.Count != 0)//直到queue為空
            {
                node = queue.Dequeue();
                for (int i = table[node - 1].point - 1; i < map.Count && i >= 0 && i < table[node].point - 1; i++)//看所有連向的點
                {
                    if (visited[map[i].head - 1] == false)//若連向的點未拜訪過
                    {
                        queue.Enqueue(map[i].head);//加入queue
                        visited[map[i].head - 1] = true;//設為已拜訪
                        Console.Write("{0} ", map[i].head);
                    }
                }
            }
            Console.WriteLine("\ntraverse finished");
        }
        public static void Main()
        {
            List<Map> map = load("Philadelphia_network.csv");
            List<Table> table = transfer(map);
            Console.Write("Start:");
            string s = Console.ReadLine();
            BFS.traverse(map, table, Int32.Parse(s));
            Console.Read();
        }
    }
}

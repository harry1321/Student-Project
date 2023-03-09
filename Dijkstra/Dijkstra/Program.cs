using System;
using System.Collections.Generic;
using System.IO;

namespace Dijkstra
{
    public class Dijkstra
    {
        public new struct Map   //存原始資料
        {
            public int head, tail, cost;
            public Map(int tail, int head, int cost)
            {
                this.tail = tail;
                this.head = head;
                this.cost = cost;
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
        public struct Node  //存節點資料
        {
            public int distance, previous;
            public Node(int distance, int previous)
            {
                this.distance = distance;
                this.previous = previous;
            }
        }
        public static new List<Map> load(string FileName)   //讀檔
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
        public static void traverse(List<Map> map, List<Table> table, int start, int end)   //進行djikstra
        {
            Node[] node = new Node[table.Count];
            for (int i = 0; i < node.Length; i++)//將所有點的距離設為無限大，前一個節點設為0
            {
                node[i].distance = Int32.MaxValue;
                node[i].previous = 0;
            }
            
            List<int> marked = new List<int>(table.Count);//紀錄已被mark的節點
            List<int> unmarked = new List<int>(table.Count);//紀錄未被mark的節點
            for (int i = 1; i < table.Count; i++)
            {
                unmarked.Add(i);
            }
            node[start - 1].distance = 0;
            node[start - 1].previous = 0;

            while (unmarked.Count != 0)
            {
                int min = Int32.MaxValue;
                int nodeNum = -1;
                
                foreach (var i in unmarked)  //找當前cost最小的點
                {
                    if (node[i - 1].distance < min)
                    {
                        min = node[i - 1].distance;
                        nodeNum = i;
                    }
                }
                if (nodeNum == -1)  //若未找到最小點
                {
                    Console.Write("error");
                    break;
                }

                marked.Add(nodeNum);//標記當前最小點
                unmarked.Remove(nodeNum);

                for (int i = table[nodeNum - 1].point - 1; i >= 0 && i < map.Count && i < table[nodeNum].point - 1; i++)//看當前最小點連向的所有點
                {
                    if (node[map[i].head - 1].distance > node[map[i].tail - 1].distance + map[i].cost && unmarked.Contains(map[i].head))//若新的distance小於原本的
                    {
                        node[map[i].head - 1].distance = node[map[i].tail - 1].distance + map[i].cost;//更新distance
                        node[map[i].head - 1].previous = nodeNum;//更新前一個點
                    }
                }

                if (nodeNum == end)
                    break;
            }
            //輸出結果
            Stack<int> stack = new Stack<int>();
            for (int i = end; i != start; i = node[i - 1].previous)
            {
                stack.Push(i);
            }
            stack.Push(start);
            while (stack.Count != 0)
            {
                Console.Write("{0} ", stack.Pop());
            }
            Console.WriteLine("\ntotal cost = {0}", node[end - 1].distance);

        }
        public static new void Main()
        {
            List<Map> map = load("Philadelphia_network.csv");
            List<Table> table = transfer(map);

            Console.Write("start:");
            int start = Int32.Parse(Console.ReadLine());
            Console.Write("end:");
            int end = Int32.Parse(Console.ReadLine());
            traverse(map, table, start, end);
            Console.Read();
        }
    }
}

using System.Net.Http.Headers;
using System.Text;

namespace Task1
{
    // Необходимо заменить на более подходящий тип (коллекцию), позволяющий
    // эффективно искать диапазон по заданному IP-адресу
    using IPRangesDatabase = List<Task1.IPRange>;

    public class Task1
    {
        /*
        * Объекты этого класса создаются из строки, но хранят внутри помимо строки
        * ещё и целочисленное значение соответствующего адреса. Например, для адреса
         * 127.0.0.1 должно храниться число 1 + 0 * 2^8 + 0 * 2^16 + 127 * 2^24 = 2130706433.
        */
        internal record IPv4Addr (string StrValue) : IComparable<IPv4Addr>
        {
            internal uint IntValue = Ipstr2Int(StrValue);

            private static uint Ipstr2Int(string strValue)
            {
                var stringValues = strValue.Split('.');
                uint intValue = 0;
                for (int i = 0; i < stringValues.Length; ++i)
                    intValue += UInt32.Parse(stringValues[i]) << (8 * (3 - i));
                return intValue;
            }

            // Благодаря этому методу мы можем сравнивать два значения IPv4Addr
            public int CompareTo(IPv4Addr other)
            {
                return IntValue.CompareTo(other.IntValue);
            }

            public override string ToString()
            {
                return StrValue;
            }
        }

        internal record class IPRange(IPv4Addr IpFrom, IPv4Addr IpTo)
        {
            public override string ToString()
            {
                return $"{IpFrom},{IpTo}";
            }
        }

        internal record class IPLookupArgs(string IpsFile, List<string> IprsFiles);
       
        internal static IPLookupArgs? ParseArgs(string[] args)
        {
            if (args.Length < 2)
                return null;
            for (int i = 0; i < args.Length; ++i)
            {
                if (!File.Exists(args[i]))
                    return null;
            }

            return new IPLookupArgs(args[0], args.Skip(1).ToList());
        }

        internal static List<string> LoadQuery(string filename)
        {
            return File.ReadAllLines(filename, Encoding.UTF8).ToList();
        }

        internal static IPRangesDatabase LoadRanges(List<String> filenames)
        {
            var rangesData = new IPRangesDatabase();
            foreach (var filename in filenames)
            {
                string[] lines = File.ReadAllLines(filename, Encoding.UTF8);
                foreach (var range in lines)
                {
                    string[] ends = range.Split(',');
                    rangesData.Add(new IPRange(new IPv4Addr(ends[0]), new IPv4Addr(ends[1])));
                }
            }
            return rangesData;
        }

        internal static IPRange? FindRange(IPRangesDatabase ranges, IPv4Addr query) {
            foreach (var range in ranges)
                if (range.IpFrom.IntValue <= query.IntValue && query.IntValue <= range.IpTo.IntValue)
                    return range;

            return null;
        }
        
        public static void Main(string[] args)
        {
            var ipLookupArgs = ParseArgs(args);
            if (ipLookupArgs == null)
            {
                return;
            }

            var queries = LoadQuery(ipLookupArgs.IpsFile);
            var ranges = LoadRanges(ipLookupArgs.IprsFiles);
            foreach (var ip in queries)
            {
                var findRange = FindRange(ranges, new IPv4Addr(ip));
                var result = findRange != null ? "NO" : "YES";
                Console.WriteLine($"{ip}: {result}");
            }
        }
    }
}
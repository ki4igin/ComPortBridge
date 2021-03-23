using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace ComPortBridge
{
    class Program
    {
        static SerialPort serialPortIn;
        static SerialPort serialPortOut;
        private static bool isRun;

        static void Main(string[] args)
        {
            Console.WriteLine("Привет");
            string[] portNames = SerialPort.GetPortNames();
            Console.WriteLine("\nВведите имя входного порта");
            int ind = -1;
            string portIn = string.Empty;
            while (true)
            {
                portIn = Console.ReadLine().ToUpper();
                ind = Array.FindIndex(portNames, name => name == portIn);
                if (ind == -1)
                {
                    Console.WriteLine("порт не найден, попробуйте ввести еще раз");
                }
                else
                {
                    break;
                }
            }

            Console.WriteLine("\nВведите имя выходного порта");
            string portOut = string.Empty;
            while (true)
            {
                portOut = Console.ReadLine().ToUpper();
                ind = Array.FindIndex(portNames, name => name == portIn);
                if (ind == -1)
                {
                    Console.WriteLine("порт не найден, попробуйте ввести еще раз");
                }
                else
                {
                    break;
                }
            }

            Console.WriteLine("\nВведите количество стоп бит");
            StopBits stopBits = StopBits.None;
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out int res) && (res == 1 || res == 2))
                {
                    stopBits = res switch
                    {
                        1 => StopBits.One,
                        2 => StopBits.Two,
                        _ => StopBits.None
                    };
                    break;
                }
                else
                {
                    Console.WriteLine("количество стоп бит неверно, попробуйте ввести еще раз");
                }
            }

            serialPortIn = new()
            {
                BaudRate = 115200,
                PortName = portIn,
                StopBits = stopBits,
                ReadTimeout = 2000,
                WriteTimeout = 2000
            };
            if (portIn == portOut)
            {
                serialPortOut = serialPortIn;
            }
            else
            {
                serialPortOut = new()
                {
                    BaudRate = 115200,
                    PortName = portOut,
                    StopBits = stopBits,
                    ReadTimeout = 2000,
                    WriteTimeout = 2000
                };
            }

            Console.WriteLine("\nЗапустить?");
            Console.ReadLine();

        run:
            var task = Task.Run(() => ReadProcess());
            Console.ReadLine();
            isRun = false;
            task.Wait();
            goto run;
        }

        internal static void ReadProcess()
        {
            byte[] buf = new byte[26];
            isRun = true;
            if (serialPortIn.IsOpen == false)
            {
                serialPortIn.Open();
            }
            if (serialPortOut.IsOpen == false)
            {
                serialPortOut.Open();
            }
            serialPortIn.DiscardInBuffer();
            serialPortOut.DiscardInBuffer();
            while (isRun)
            {
                if (ReadBuf(ref buf, 26))
                {
                    serialPortOut.Write(buf, 0, 26);
                    string str = BitConverter.ToString(buf);
                    Console.WriteLine(str);
                }
                Thread.Sleep(5);
            }
            serialPortIn.Close();
            serialPortOut.Close();
        }

        internal static bool ReadBuf(ref byte[] buf, int len)
        {
            try
            {
                int nBytes = len;
                do
                {
                    nBytes -= serialPortIn.Read(buf, len - nBytes, nBytes);
                } while (nBytes > 0);
                return true;
            }
            catch (TimeoutException)
            {
                return false;
            }

        }
    }
}

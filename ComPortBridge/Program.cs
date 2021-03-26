using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace ComPortBridge
{
    class Program
    {
        static SerialPort serialPortIn;
        static SerialPort[] serialPortOut;
        private static bool isRun;
        static int portNum = 0;

        static void Main(string[] args)
        {
            Console.WriteLine("Привет");
            string[] portNames = SerialPort.GetPortNames();
            portNum = portNames.Length - 1;
            while (portNum <= 0)
            {
                Console.WriteLine("Порты не обнаружены, попробывать еще раз?");
                Console.ReadLine();
                portNames = SerialPort.GetPortNames();
                portNum = portNames.Length - 1;
            }
            if (portNum > 3)
            {
                portNum = 3;
            }
            serialPortOut = new SerialPort[portNum];
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

            string[] portOut = new string[portNum];
            for (int i = 0; i < portNum; i++)
            {
                Console.WriteLine($"\nВведите имя выходного порта №{i + 1}");
            while (true)
            {
                    string name = Console.ReadLine().ToUpper();
                    ind = Array.FindIndex(portNames, t => t == name);
                if (ind == -1)
                {
                    Console.WriteLine("порт не найден, попробуйте ввести еще раз");
                }
                    else if (Array.FindIndex(portOut, t => t == name) != -1 || name == portIn)
                    {
                        Console.WriteLine("порт уже используется, попробуйте ввести еще раз");
                    }
                else
                {
                        portOut[i] = name;
                    break;
                        
                }
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

            for (int i = 0; i < portNum; i++)
            {
                serialPortOut[i] = new()
                {
                    BaudRate = 115200,
                    PortName = portOut[i],
                    StopBits = stopBits,
                    ReadTimeout = 2000,
                    WriteTimeout = 2000
                };
            }

            Console.WriteLine("\nЗапустить?");
            Console.ReadLine();

        run:
            Task.Run(() => ReadProcess());
            Console.ReadLine();
            isRun = false;
            Console.WriteLine("Перезапуск......");
            Thread.Sleep(2000);
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
            serialPortIn.DiscardInBuffer();
            for (int i = 0; i < portNum; i++)
            {
                if (serialPortOut[i].IsOpen == false)
            {
                    serialPortOut[i].Open();
                }
                serialPortOut[i].DiscardInBuffer();
            }


            while (isRun)
            {
                if (ReadBuf(ref buf, 26))
                {
                    for (int i = 0; i < portNum; i++)
                    {
                        serialPortOut[i].Write(buf, 0, 26);
                    }
                    string str = BitConverter.ToString(buf);
                    Console.WriteLine(str);
                    serialPortOut.Write(buf, 0, 26);
                }
                Thread.Sleep(5);
            }
            serialPortIn.Close();

            for (int i = 0; i < portNum; i++)
            {
                serialPortOut[i].Close();
            }
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

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AccessComPort
{
    class Program
    {


        static void Main(string[] args)
        {
            if (args.Length == 0) {
                Console.WriteLine("Enter a COM port, e.g. COM1 or ALL check all avaliable.");
                return;
            }

            var userArg = args[0].ToString();

            if (userArg.ToLower() == "all") {
                //show list of valid com ports
                foreach (string s in SerialPort.GetPortNames())
                {
                    VerifyComPort(s);   
                }  
            }
            else {
                VerifyComPort(userArg);
                //OpenComPort(userArg);
            }



            Console.ReadKey();


        }

        private static void OpenComPort(string port)
        {
             try
            {
            var comPort = new SerialPort {PortName = port};
            Console.WriteLine("Opening COM Port {0} on thread {1}", comPort, Thread.CurrentThread.ManagedThreadId);
            comPort.Open();
            }
             catch (UnauthorizedAccessException uae)
             {
                 Console.WriteLine(string.Format("Com Port {0} is INVALID.  Access is denied/COM Port may be open by another process.",
                                 port), uae);
              
             }
             catch (ArgumentOutOfRangeException are)
             {
                 Console.WriteLine(string.Format("Com Port {0} is INVALID.  Port parameters may be incorrect.",
                                 port), are);
              
             }
             catch (ArgumentException ae)
             {
                 Console.WriteLine(string.Format("Com Port {0} is INVALID. Port Name is invalid.",
                                 port), ae);
              
             }
             catch (IOException ioe)
             {
                 Console.WriteLine(string.Format("Com Port {0} is INVALID.  Port is in an invalid state/port parameters may be incorrect.",
                                 port), ioe);
              
             }
             catch (InvalidOperationException ive)
             {
                 Console.WriteLine(string.Format("Com Port {0} is INVALID.  COM Port is already open in the instance of COMPORT.",
                                 port), ive);
              
             }
             catch (Exception e)
             {
                 Console.WriteLine(string.Format("Com Port {0} is INVALID.  Access is denied/COM Port may be open by another process.",
                                 port), e);
              
             }
        }


        private static bool VerifyComPort(string port)
        {
            try
            {
                using (var comPort = new SerialPort())
                {
                    // When data is recieved through the port, call this method
                    /*comPort.ReadTimeout = 500;
                    comPort.WriteTimeout = 500;
                    comPort.BaudRate = 4800;
                    comPort.DataBits = 8;
                    comPort.StopBits = StopBits.One;
                    comPort.Parity = Parity.None;*/
                    comPort.PortName = port;
                    
                    Console.WriteLine("\nOpening COM Port {0} on thread {1}", port, Thread.CurrentThread.ManagedThreadId);
                    comPort.Open();
                    Console.WriteLine("Port Opened");
                    comPort.Close();
                    Console.WriteLine("Port Closed");
                }
                Console.WriteLine("Com Port {0} is VALID", port);
            }
            catch (UnauthorizedAccessException uae)
            {
                Console.WriteLine(string.Format("Com Port {0} is INVALID.  Access is denied/COM Port may be open by another process.",
                                port), uae);
                return false;
            }
            catch (ArgumentOutOfRangeException are)
            {
                Console.WriteLine(string.Format("Com Port {0} is INVALID.  Port parameters may be incorrect.",
                                port), are);
                return false;
            }
            catch (ArgumentException ae)
            {
                Console.WriteLine(string.Format("Com Port {0} is INVALID. Port Name is invalid.",
                                port), ae);
                return false;
            }
            catch (IOException ioe)
            {
                Console.WriteLine(string.Format("Com Port {0} is INVALID.  Port is in an invalid state/port parameters may be incorrect.",
                                port), ioe);
                return false;
            }
            catch (InvalidOperationException ive)
            {
                Console.WriteLine(string.Format("Com Port {0} is INVALID.  COM Port is already open in the instance of COMPORT.",
                                port), ive);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("Com Port {0} is INVALID.  Access is denied/COM Port may be open by another process.",
                                port), e);
                return false;
            }


            return true;

        }
    }
}

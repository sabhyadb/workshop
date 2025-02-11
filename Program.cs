using System;
using System.IO.Ports;
using System.Threading;
using InfluxDB.Client;
using InfluxDB.Client.Writes;
using InfluxDB.Client.Api.Domain;


class Program
{
    // Serial port settings
    private static readonly string[] SerialPorts = { "COM5"}; // Replace with your com port retrieved from device manager
    private const int BaudRate = 115200;

    // InfluxDB settings
    private const string InfluxUrl = "http://localhost:8086";
    private const string InfluxToken = "initialtokenestablishedinInfluxdb"; // Replace with your token
    private const string InfluxOrg = "Sabhya";
    private const string InfluxBucket = "2ESPSC";

    static void Main(string[] args)
    {
        // Initialize InfluxDB client using factory method
        using var influxClient = InfluxDBClientFactory.Create(InfluxUrl, InfluxToken.ToCharArray());
        var writeApi = influxClient.GetWriteApi();

        try
        {
            // Start a thread for each serial port
            foreach (string port in SerialPorts)
            {
                Thread thread = new Thread(() => CollectData(port, writeApi));
                thread.IsBackground = true; // Ensure thread exits with the program
                thread.Start();
            }

            Console.WriteLine("Press Ctrl+C to exit...");
            while (true)
            {
                Thread.Sleep(1000); // Keep the main thread alive
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            writeApi.Dispose();
            influxClient.Dispose();
            Console.WriteLine("InfluxDB client closed.");
        }
    }

        static void CollectData(string portName, WriteApi writeApi)
    {
        SerialPort? serialPort = null;

        try
        {
            serialPort = new SerialPort(portName, BaudRate);
            serialPort.Open();
            Console.WriteLine($"Listening for Serial data on {portName}...");

            while (true)
            {
                try
                {
                    if (serialPort.IsOpen)
                    {
                        string line = serialPort.ReadLine().Trim();
                        Console.WriteLine($"[{portName}] Received: {line}");

                        var point = PointData
                            .Measurement("task_status")
                            .Field("data", line)
                            .Field("port", portName)
                            .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

                        writeApi.WritePoint(InfluxBucket, InfluxOrg, point);
                        Console.WriteLine($"[{portName}] Data written to InfluxDB.");
                    }
                    else
                    {
                        Console.WriteLine($"[{portName}] Serial port is closed unexpectedly.");
                        break;
                    }
                }
                catch (TimeoutException)
                {
                    Console.WriteLine($"[{portName}] Timeout occurred.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{portName}] Error processing data: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{portName}] Failed to initialize or read from serial port: {ex.Message}");
        }
        finally
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                Console.WriteLine($"[{portName}] Serial port closed.");
            }
        }
    }

}

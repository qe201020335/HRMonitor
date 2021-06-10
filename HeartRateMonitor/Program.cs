using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using System.Timers;

namespace HeartRateMonitor
{
    internal class Program
    {
        
        private DeviceWatcher deviceWatcher;
        private BluetoothLEDevice ble;
        private DeviceInformation ble_info;
        private Timer timer;
        private string devName;  // Polar H10 7B3C0520
        private bool finished_searching = false;
        private List<Task> tasks = new List<Task>();

        public static async Task Main(string[] args)
        {
            var program = new Program();
            await program.main();
        }

        private Program()
        {
            Console.WriteLine("Enter device name:");
            devName = Console.ReadLine();
        }

        private async Task main()
        {
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };
            
            string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

            string devFromName = BluetoothLEDevice.GetDeviceSelectorFromDeviceName(devName);

            deviceWatcher = DeviceInformation.CreateWatcher(devFromName, requestedProperties, DeviceInformationKind.AssociationEndpoint);

            deviceWatcher.Added += DeviceAdded;
            deviceWatcher.Removed += DeviceRemoved;
            deviceWatcher.Updated += DeviceUpdated;
            deviceWatcher.EnumerationCompleted += DeviceEnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcherStopped;
            
            deviceWatcher.Start();
            timer = new Timer(5000);
            

            StopWatcher();
            
            
        }

        private void OnTimerExpired(Object sender, ElapsedEventArgs e)
        {
            if (sender != timer)
            {
                return;
            }
            
        }

        private async void DeviceGot()
        {
            ble = await BluetoothLEDevice.FromIdAsync(ble_info.Id);
            Console.WriteLine(ble.GattServices);
            
            
            Console.WriteLine("Done");
        }

        private void DeviceAdded(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            Console.WriteLine("Added");
            Console.WriteLine(deviceInfo.Id);
            Console.WriteLine(deviceInfo.Name);
            ble_info = deviceInfo;
            StopWatcher();
        }
        
        private void DeviceUpdated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            Console.WriteLine("Updated");
            Console.WriteLine(deviceInfoUpdate.ToString());
        }

        private void DeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            Console.WriteLine("Removed");
            Console.WriteLine(deviceInfoUpdate.ToString());
        }

        private void DeviceEnumerationCompleted(DeviceWatcher sender, object e)
        {
            Console.WriteLine("Complete");
            
        }

        private void DeviceWatcherStopped(DeviceWatcher sender, object e)
        {
            Console.WriteLine("Stopped");
        }

        private void StopWatcher()
        {   
            deviceWatcher.Added -= DeviceAdded;
            deviceWatcher.Removed -= DeviceRemoved;
            deviceWatcher.Updated -= DeviceUpdated;
            deviceWatcher.EnumerationCompleted += DeviceEnumerationCompleted;
            deviceWatcher.Stopped -= DeviceWatcherStopped;
            deviceWatcher.Stop();
        }

        
    }
}
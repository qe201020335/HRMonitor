using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Timer = System.Timers.Timer;

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
        private TaskCompletionSource<bool> connected = new TaskCompletionSource<bool>();

        public static async Task Main(string[] args)
        {
            var program = new Program();
            bool found = await program.search_entry();
            if (!found)
            {
                Console.WriteLine("Device Not Found");
                return;
            }
            program.Connect();
            Task.WaitAll(program.connected.Task);
            Console.WriteLine("Done");
        }

        private Program()
        {
            Console.WriteLine("Enter device name:");
            devName = Console.ReadLine();
        }

        private async Task<bool> search_entry()
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
            await Task.Delay(5000);
            if (!finished_searching)
            {
                StopWatcher();
                finished_searching = true;
            }
            if (ble_info == null)
            {
                return false;
            }
            return true;
        }
        
        private async void Connect()
        {
            ble = await BluetoothLEDevice.FromIdAsync(ble_info.Id);
            
            Console.WriteLine(ble.GattServices.Count);

            foreach (GattDeviceService service in ble.GattServices)
            {
                Console.WriteLine(service.Uuid);
            }
            Console.WriteLine("Connected");
            Console.Out.Flush();
            connected.TrySetResult(true);
            
        }

        private void DeviceAdded(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            Console.WriteLine("Added");
            Console.WriteLine(deviceInfo.Id);
            Console.WriteLine(deviceInfo.Name);
            ble_info = deviceInfo;
            StopWatcher();
            finished_searching = true;
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
            deviceWatcher.EnumerationCompleted -= DeviceEnumerationCompleted;
            deviceWatcher.Stopped -= DeviceWatcherStopped;
            deviceWatcher.Stop();
        }

        
    }
}
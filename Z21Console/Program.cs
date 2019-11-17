using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Z21;
using Z21.API;
using Z21.Domain;

namespace Z21Console {
  internal class Program {
    private static async Task Main() {
      var ipAdress = new IPAddress(new byte[] { 192, 168, 0, 111 });
      var endpoint = new IPEndPoint(ipAdress, 21105);
      using (var client = new UdpClient(new System.Net.Sockets.UdpClient(12345), endpoint))
      using (var z21Client = new Z21Client(client)) {
        Console.WriteLine(z21Client.GetSerialNumber(new SerialNumberRequest()));
        z21Client.SetBroadcastFlags(new SetBroadcastFlagsRequest { BroadcastFlags = BroadcastFlags.DrivingAndSwitching | BroadcastFlags.AllLocs | BroadcastFlags.Z21SystemState });
        z21Client.TrackStatusChanged += TrackStatusPrinter;
        //z21Client.SystemStateChanged += TrackStatusPrinter;
        z21Client.LocomotiveInformationChanged += TrackStatusPrinter;

        z21Client.SetTrainSpeed(new TrainSpeedRequest { TrainAddress = 3, TrainSpeed = new TrainSpeed(SpeedStepSetting.Step128, DrivingDirection.Forward, (Speed)25) });
        z21Client.SetTrainSpeed(new TrainSpeedRequest { TrainAddress = 24, TrainSpeed = new TrainSpeed(SpeedStepSetting.Step28, DrivingDirection.Forward, (Speed)20) });

        Console.ReadLine();

        z21Client.SetTrainSpeed(new TrainSpeedRequest { TrainAddress = 3, TrainSpeed = new TrainSpeed(SpeedStepSetting.Step128, DrivingDirection.Forward, Speed.Stop) });
        z21Client.SetTrainSpeed(new TrainSpeedRequest { TrainAddress = 24, TrainSpeed = new TrainSpeed(SpeedStepSetting.Step28, DrivingDirection.Forward, Speed.Stop) });
        Thread.Sleep(1000);
      }



    }

    public static void MessageHandler(object sender, byte[] message) {
      Console.WriteLine(string.Join(", ", message));
    }

    public static void TrackStatusPrinter<T>(object sender, T trackStatus) {
      Console.WriteLine(trackStatus);
    }
  }
}

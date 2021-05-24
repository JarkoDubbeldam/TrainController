using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TrainControllerArduinoTester {
  class Program {
    static UdpClient udpClient = new UdpClient(21105);
    static int counter = 0;
    static IEnumerator<byte> occupancies = ((IEnumerable<byte>) new byte[] { 0, 1, 3, 2, 6, 4, 8 | 4, 8, 0 }).GetEnumerator();
    static async Task Main(string[] args) {
      
      while (true) {
        await ListenForRequest();
      }
    }

    static async Task ListenForRequest() {
      var request = await udpClient.ReceiveAsync();
      Console.WriteLine($"{counter++}: {string.Join(' ', request.Buffer)}");
      if (!occupancies.MoveNext()) {
        occupancies.Reset();
        occupancies.MoveNext();
      }
      await udpClient.SendAsync(new byte[] { 15, 0, 128, 0, 1, occupancies.Current, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 15, request.RemoteEndPoint);
    }
  }
}

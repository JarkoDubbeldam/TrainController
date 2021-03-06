﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Z21 {
//  public class UdpMessageHandler : IDisposable {
//    private readonly IUdpClient udpClient;
//    private readonly CancellationTokenSource source;
//    private readonly Task listenForMessagesTask;

//    public UdpMessageHandler(IUdpClient udpClient) {
//      this.udpClient = udpClient;
//      source = new CancellationTokenSource();
//      listenForMessagesTask = Task.Run(() => ListenForMessages(source.Token), source.Token);
//    }

//    public event EventHandler<byte[]> MessageReceived;


//    private Task ListenForMessages(CancellationToken token) {
//      while (true) {
//        if (token.IsCancellationRequested) return Task.FromCanceled(token);
//        var message = udpClient.ReceiveBytes();
//        foreach (var subMessage in SplitMessages(message)) {
//          Task.Run(() => MessageReceived?.Invoke(this, subMessage));
//        }
//      }
//    }

//    private IEnumerable<byte[]> SplitMessages(byte[] message) {
//      var index = 0;
//      while (index < message.Length) {
//        var messageLength = message[index];
//        var subMessage = message.Skip(index).Take(messageLength).ToArray();
//        yield return subMessage;
//        index += messageLength;
//      }
//    }

//    #region IDisposable Support
//    private bool disposedValue = false; // To detect redundant calls

//    protected virtual void Dispose(bool disposing) {
//      if (!disposedValue) {
//        if (disposing) {
//          source.Cancel();
//          source.Dispose();
//        }

//        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
//        // TODO: set large fields to null.

//        disposedValue = true;
//      }
//    }

//    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
//    // ~UdpMessageHandler()
//    // {
//    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
//    //   Dispose(false);
//    // }

//    // This code added to correctly implement the disposable pattern.
//    public void Dispose() {
//      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
//      Dispose(true);
//      // TODO: uncomment the following line if the finalizer is overridden above.
//      // GC.SuppressFinalize(this);
//    }
//    #endregion


//  }
//}

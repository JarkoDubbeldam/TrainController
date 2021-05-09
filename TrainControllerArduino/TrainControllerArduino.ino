/*
 Name:		TrainControllerArduino.ino
 Created:	1/19/2021 12:40:08 PM
 Author:	Jarko

*/


//#define DEBUG_MESSAGES

#include <Ethernet.h>
#include <EthernetUdp.h>
#include "TrackLoopRelayHandler.h"

#include <SPI.h>

#define PIN_RELAY_1 3
#define PIN_RELAY_2 7
#define PIN_RELAY_3 8
#define PIN_RELAY_4 9

#define POLL_DELAY_IN_MS 250


constexpr byte mac[] = { 0xA8, 0x61, 0x0A, 0xAE, 0x8A, 0x8E };

IPAddress z21IpAddress(192, 168, 0, 111);
int z21Port = 21105;

unsigned int localPort = 8888;      // local port to listen on

// buffers for receiving and sending data
char packetBuffer[UDP_TX_PACKET_MAX_SIZE];  // buffer to hold incoming packet

// An EthernetUDP instance to let us send and receive packets over UDP
EthernetUDP Udp;

#define HANDLER_COUNT 1
TrackLoopRelayHandler* handlers;



void setup() {
  Ethernet.init(10); 


  Serial.begin(9600);
  while (!Serial);
  // start the Ethernet connection:
  Serial.println("Initialize Ethernet with DHCP:");
  if (Ethernet.begin(mac) == 0) {
    Serial.println("Failed to configure Ethernet using DHCP");
    if (Ethernet.hardwareStatus() == EthernetNoHardware) {
      Serial.println("Ethernet shield was not found.  Sorry, can't run without hardware. :(");
    } else if (Ethernet.linkStatus() == LinkOFF) {
      Serial.println("Ethernet cable is not connected.");
    }
    // no point in carrying on, so do nothing forevermore:
    while (true) {
      delay(1);
    }
  }

  // print your local IP address:
  Serial.print("My IP address: ");
  Serial.println(Ethernet.localIP());

  pinMode(PIN_RELAY_1, OUTPUT);
  pinMode(PIN_RELAY_2, OUTPUT);
  pinMode(PIN_RELAY_3, OUTPUT);
  pinMode(PIN_RELAY_4, OUTPUT);

  digitalWrite(PIN_RELAY_1, LOW);
  digitalWrite(PIN_RELAY_2, LOW);
  digitalWrite(PIN_RELAY_3, LOW);
  digitalWrite(PIN_RELAY_4, LOW);

  // start UDP
  Udp.begin(localPort);

  
  handlers = new TrackLoopRelayHandler[HANDLER_COUNT] {
    TrackLoopRelayHandler(TrackLoopListener(TrackSection(4, 8), TrackSection(3, 2), TrackSection(3, 1), TrackSection(4, 5)), PIN_RELAY_1, PIN_RELAY_2)
  };

  for (int i = 0; i < HANDLER_COUNT; i++){
    handlers[i].print();
  }
}

void loop() {
  Ethernet.maintain();
  SendRequest();
  delay(20);
  ReceiveResponse();
  delay(POLL_DELAY_IN_MS);
}



constexpr byte Request[] = { 0x05, 0x00, 0x81, 0x00, 0x00 };
const int RequestLength = 5;
void SendRequest() {
    Udp.beginPacket(z21IpAddress, z21Port);
    Udp.write(Request, RequestLength);
    Udp.endPacket();
#ifdef DEBUG_MESSAGES
    Serial.println("Sent request to z21.");
#endif
}

void ReceiveResponse() {
  int packetSize = Udp.parsePacket();
  if (packetSize) {
    // read the packet into packetBufffer
    Udp.read(packetBuffer, UDP_TX_PACKET_MAX_SIZE);

#ifdef DEBUG_MESSAGES
    for (int i = 0; i < packetSize; i++) {
        Serial.print((byte)packetBuffer[i]);
        Serial.print(" ");
    }
    Serial.println();
#endif

    // Response is the size we expect.
    if (packetSize == 15) {
        // Response has the identifying bytes we expect.
        if ((byte)packetBuffer[0] == 0x0F && (byte)packetBuffer[2] == 0x80) {
            byte occupancybuffer[10]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < 10; i++) {
                occupancybuffer[i] = (byte)packetBuffer[5 + i];
            }
            for (int i = 0; i < HANDLER_COUNT; i++){
                handlers[i].updateRelays(occupancybuffer);
            }
        }
    }
  }
}

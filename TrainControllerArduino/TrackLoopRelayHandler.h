#pragma once

#include "TrackLoopListener.h"


class TrackLoopRelayHandler
{
public:
    TrackLoopRelayHandler(TrackLoopListener&& listener, int relay1, int relay2) : listener(listener), relay1(relay1), relay2(relay2) {}
    void updateRelays(const byte occupancyBuffer[10]) const;

private:
    TrackLoopListener& listener;
    int relay1;
    int relay2;
};


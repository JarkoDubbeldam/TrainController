#include "TrackLoopRelayHandler.h"

void TrackLoopRelayHandler::updateRelays(const byte occupancyBuffer[10]) const
{
    auto requiredChange = listener.handleTrackStatusUpdate(occupancyBuffer);
    switch (requiredChange) {
    case INVERT:
        digitalWrite(relay1, HIGH);
        digitalWrite(relay2, HIGH);
        break;
    case REVERT:
        digitalWrite(relay1, LOW);
        digitalWrite(relay2, LOW);
    }
}

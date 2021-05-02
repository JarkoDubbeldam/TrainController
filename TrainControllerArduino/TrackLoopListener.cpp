// 
// 
// 

#include "TrackLoopListener.h"

bool getOccupancyStatus(const byte occupancyBytes[10], const TrackSection& section) {
    auto group = occupancyBytes[section.Group - 1];
    return (group & (1 << (section.Id - 1))) > 0;
}

TrackLoopListener::TrackLoopListener(const TrackSection entrance, const TrackSection loop1, const TrackSection loop2, const TrackSection exit) :
    entrance(entrance), loop1(loop1), loop2(loop2), exit(exit), trackStatus(NORMAL), loopStatus(EMPTY){}

TrackLoopAction TrackLoopListener::handleTrackStatusUpdate(const byte occupancyBytes[10])
{
    auto loopStatus = parseStatusFromOccupancy(occupancyBytes);
#ifdef DEBUG_MESSAGES
    Serial.print("Loop status is: ");
    Serial.println(loopStatus);
#endif
    if (loopStatus == this->loopStatus) {
        return NOTHING;
    }
    this->loopStatus = loopStatus;

    //Serial.print("Loop status changed to: ");
    //Serial.println(loopStatus);

    if (loopStatus == APPROACH_EXIT) {
        return INVERT;
    }
    if (loopStatus == EMPTY) {
        return REVERT;
    }

    return NOTHING;
}

TrackLoopStatus TrackLoopListener::parseStatusFromOccupancy(const byte occupancyBytes[10]) const
{
    const auto entranceOccupied = getOccupancyStatus(occupancyBytes, entrance);
    const auto loop1Occupied = getOccupancyStatus(occupancyBytes, loop1);
    const auto loop2Occupied = getOccupancyStatus(occupancyBytes, loop2);
    const auto exitOccupied = getOccupancyStatus(occupancyBytes, exit);
    
    auto trackStatus = (entranceOccupied ? 1 : 0) +
        (loop1Occupied ? 2 : 0) +
        (loop2Occupied ? 4 : 0) +
        (exitOccupied ? 8 : 0);

    switch (trackStatus) {
    case 0: // None occupied
        return EMPTY;
    case 1: // Just Entrance Occupied
        return APPROACH_ENTRANCE;
    case 2: // Just Loop 1 Occupied
        return OCCUPIED;
    case 3: // Entrance and Loop 1 Occupied
        return ENTERING;
    case 4: // Just Loop 2 Occupied
        return APPROACH_EXIT;
    case 5: // Entrance and Loop 2
        return FAULTED;
    case 6: // Loop 1 and 2 Occupied
        return APPROACH_EXIT;
    case 7: // Entrance, Loop 1 and Loop 2 Occupied
        return ENTERING;
    case 8: // Just Exit Occupied
        return EMPTY;
    case 9: // Entrance and Exit Occupied
        return FAULTED;
    case 10: // Loop 1 and Exit Occupied
        return FAULTED;
    case 11: // Entrance, Loop 1 and Exit Occupied
        return FAULTED;
    case 12: // Loop 2 and Exit Occupied
        return EXITING;
    case 13: // Entrance, Loop 2 and Exit 
        return FAULTED;
    case 14: // Loop 1, Loop 2 and Exit
        return EXITING;
    case 15: // All Occupied
        return FAULTED;
    };

    // Should be unreachable.
    return FAULTED;
}

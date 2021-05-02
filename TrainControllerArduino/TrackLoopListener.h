// TrackLoopController.h

#ifndef _TrackLoopController_h
#define _TrackLoopController_h

#if defined(ARDUINO) && ARDUINO >= 100
	#include "arduino.h"
#else
	#include "WProgram.h"
#endif

enum TrackLoopStatus {
		EMPTY,
		APPROACH_ENTRANCE,
		ENTERING,
		OCCUPIED,
		APPROACH_EXIT,
		EXITING,
		FAULTED
};

enum TrackLoopAction {
		NOTHING,
		INVERT,
		REVERT
};

struct TrackSection {
		int Group;
		int Id;
		TrackSection(int group, int id) : Group(group), Id(id) {}
};

class TrackLoopListener {
public:
		TrackLoopListener(TrackSection entrance, TrackSection loop1, TrackSection loop2, TrackSection exit);

		TrackLoopAction handleTrackStatusUpdate(const byte occupancyBytes[10]);

private:
		enum InternalTrackStatus {
				NORMAL,
				INVERTED
		}; 
		TrackSection entrance; 
		TrackSection loop1; 
		TrackSection loop2; 
		TrackSection exit;
		TrackLoopStatus loopStatus;
		InternalTrackStatus trackStatus;
		TrackLoopStatus parseStatusFromOccupancy(const byte occupancyBytes[10]) const;
		
};

#endif


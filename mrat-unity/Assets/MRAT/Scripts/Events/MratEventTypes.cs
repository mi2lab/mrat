namespace MRAT
{
    public enum MratEventTypes
    {
        Custom = 1,
        AppStart,
		FocusStart,
		FocusStop,
	    InputClickWithTarget,
	    InputClickWithoutTarget,
		InputNoClick,
		InputSourceDetected,
		InputSourceLost,
		ModeUpdate,
	    ObjectUpdate,
        ObjectFixate,
	    ObjectStartedTracking,
	    ObjectDestroyed,
		PerformanceWarning,
	    PhotoCaptured,
		Screenshot,
	    StatusUpdate,
		TaskFound,
		TaskStarted,
		TaskSkipped,
		TaskCompleted,
		TaskUpdate,
		VoiceCommand,
		CustomStart,
		CustomEnd,
        MarkerFound, // log as MratEventSimple in Vuforia/Scripts/DefaultTrackableEventHandler
        MarkerLost // log as MratEventSimple in Vuforia/Scripts/DefaultTrackableEventHandler
    }
}

using System;
using System.Diagnostics;
using System.Timers;

namespace LunaGB.Core.RTC {

public class RTCTimer : Timer
{
	Stopwatch sw;
	double remainingTime;
	double interval;
	bool resumed;
	public delegate void OnTimerElapsedEvent();
	public event OnTimerElapsedEvent OnTimerElapsed;

	public RTCTimer(double interval) : base(interval){
		sw = new Stopwatch();
		Elapsed += OnElapsed;
		this.interval = interval;
	}

	public new void Start(){
		ResetStopwatch();
		base.Start();
	}

	private void OnElapsed(object? sender, ElapsedEventArgs e){
		if(resumed){
			resumed = false;
			Stop();
			Interval = interval;
			Start();
		}else{
			ResetStopwatch();
		}
		OnTimerElapsed?.Invoke();
	}

	void ResetStopwatch(){
		sw.Reset();
		sw.Start();
	}

	public void Pause(){
		Stop();
		sw.Stop();
		remainingTime = interval - sw.Elapsed.TotalMilliseconds;
	}

	public void Resume(){
		resumed = true;
		if(remainingTime > 0){
			Interval = remainingTime;
			remainingTime = 0;
		}
		Start();
	}
}

}

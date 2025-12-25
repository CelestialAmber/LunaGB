using System;
using System.Collections.Generic;
using System.Linq;
using Tsukimi.Utils;

namespace LunaGB.RTC {

public class RealTimeClock
{
	int latchedSecs;
	int latchedMins;
	int latchedHours;
	int latchedDays;
	int secs;
	int mins;
	int hours;
	int days;
	bool halted;
	bool latched;
	long startTimeMs;
	long millisecondsCount;
	RTCTimer timer;

	public bool IsHalted() => halted;
	public bool overflow = false;

	public RealTimeClock(){
		timer = new RTCTimer(500);
		timer.OnTimerElapsed += Update;
		timer.AutoReset = true;
	}

	public void Init(){
		latchedSecs = 0;
		latchedMins = 0;
		latchedHours = 0;
		latchedDays = 0;
		secs = 0;
		mins = 0;
		hours = 0;
		days = 0;
		halted = false;
		latched = false;
		updating = false;
		overflow = false;
		millisecondsCount = 0;
		startTimeMs = GetCurrentTimeMs();
	}

	public void Start(){
		timer.Start();
	}

	public void Pause(){
		timer.Pause();
	}

	public void Resume(){
		timer.Resume();
	}

	public void Stop(){
		timer.Stop();
	}

	bool updating = false;

	void Update(){
		if(!updating){
			updating = true;
			long passedMs = GetCurrentTimeMs() - startTimeMs;
			startTimeMs = GetCurrentTimeMs();
			millisecondsCount += passedMs;
			AddMillisecondsToClock(millisecondsCount);
			updating = false;
		}
	}

	void IncrementSeconds(){
		secs++;
		if(secs == 60){
			secs = 0;
			IncrementMinutes();
		}
	}

	void IncrementMinutes(){
		mins++;
		if(mins == 60){
			mins = 0;
			IncrementHours();
		}
	}

	void IncrementHours(){
		hours++;
		if(hours == 24){
			hours = 0;
			IncrementDays();
		}
	}

	void IncrementDays(){
		days++;
		if(days > 511){
			days = 0;
			overflow = true;
		}
	}

	public void Latch(){
		latched = true;
		latchedSecs = secs;
		latchedMins = mins;
		latchedHours = hours;
		latchedDays = days;
	}

	public void Unlatch(){
		latched = false;
	}

	public void SetHalt(bool state){
		if(!halted && state == true){
			Pause();
		}else if(halted && state == false){
			Resume();
			startTimeMs = GetCurrentTimeMs();
		}

		halted = state;
	}

	public int GetSeconds(){
		if(latched) return latchedSecs;
		else return secs;
	}

	public void SetSeconds(int secs){
		this.secs = secs;
	}

	public int GetMinutes(){
		if(latched) return latchedMins;
		else return mins;
	}

	public void SetMinutes(int mins){
		this.mins = mins;
	}

	public int GetHours(){
		if(latched) return latchedHours;
		else return hours;
	}

	public void SetHours(int hours){
		this.hours = hours;
	}

	public int GetDays(){
		if(latched) return latchedDays;
		else return days;
	}

	public void SetDays(int days){
		this.days = days;
	}

	long GetCurrentTimeMs(){
		return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
	}

	//Returns the RTC data in the BGB RTC save format.
	public byte[] ToByteArray(){
		List<byte> data = new List<byte>();
		int daysLow = days & 0xFF;
		int daysHigh = (days >> 8) & 1;
		int latchedDaysLow = latchedDays & 0xFF;
		int latchedDaysHigh = (latchedDays >> 8) & 1;
		long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		data.AddRange(secs.ToByteArray());
		data.AddRange(mins.ToByteArray());
		data.AddRange(hours.ToByteArray());
		data.AddRange(daysLow.ToByteArray());
		data.AddRange(daysHigh.ToByteArray());
		data.AddRange(latchedSecs.ToByteArray());
		data.AddRange(latchedMins.ToByteArray());
		data.AddRange(latchedHours.ToByteArray());
		data.AddRange(latchedDaysLow.ToByteArray());
		data.AddRange(latchedDaysHigh.ToByteArray());
		data.AddRange(timestamp.ToByteArray());
		return data.ToArray();
	}

	//Loads RTC data from the save file.
	public void LoadSaveRTCData(byte[] data){
		secs = ReadInt32(data, 0);
		mins = ReadInt32(data, 4);
		hours = ReadInt32(data, 8);
		int daysLow = ReadInt32(data, 12);
		int daysHigh = ReadInt32(data, 16);
		days = daysLow + (daysHigh << 8);
		long timestamp = ReadInt64(data, 40);
		//Calculate how much time has passed since the save file was saved, and add that much time to the clock.
		long passedMs = GetCurrentTimeMs() - (timestamp * 1000);
		AddMillisecondsToClock(passedMs);
	}

	void AddMillisecondsToClock(long ms){
		while(ms >= 1000 * 60 * 60 * 24){
			ms -= 1000 * 60 * 60 * 24;
			IncrementDays();
		}
		while(ms >= 1000 * 60 * 60){
			ms -= 1000 * 60 * 60;
			IncrementHours();
		}
		while(ms >= 1000 * 60 * 60 * 24){
			ms -= 1000 * 60;
			IncrementDays();
		}
		while(ms >= 1000){
			ms -= 1000;
			IncrementSeconds();
		}
		//Put the remaining milliseconds in the counter
		millisecondsCount = ms;
	}

	int ReadInt32(byte[] array, int offset){
		byte[] bytes = array.Skip(offset).Take(4).ToArray();
		if(!BitConverter.IsLittleEndian) Array.Reverse(bytes);
		return BitConverter.ToInt32(bytes);
	}

	long ReadInt64(byte[] array, int offset){
		byte[] bytes = array.Skip(offset).Take(8).ToArray();
		if(!BitConverter.IsLittleEndian) Array.Reverse(bytes);
		return BitConverter.ToInt64(bytes);
	}
}

}
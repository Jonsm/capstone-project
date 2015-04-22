using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class SpectrumAnalyzer {
	//each band will be checked when looking at beats per minute
	private int[][] bands = {
		new int[] {0, 200}, 
		new int[] {200, 400}, 
		new int[] {400, 800}, 
		new int[] {800, 1600},
		new int[] {1600, 3200},
		new int[] {3200, 20000}
	};

	private int fudgeFactor = 16; //will use 1/fudgefactor signals, must be power of 2 <= 16
	private int threads = 4; //how many ways to split the samples
	private float idealSampleRate = .25f; //ideal sample rate, in hertz
	private float hanningTime = .05f; //falloff time for hanning window
	private int smoothWindow = 2; //window around array value to take average in smoothing
	private int beatSection = 4; //what fraction of the song to sample for beats (power of 2)
	private float harmonicTolerance = .75f; //what fraction of the max harmonics must be to count as beats
	private int maxHarmonic = 4; //largest harmonic to search

	private int bpmWindow = 32; //size of window used to check for beats
	private int [] bpmRange = new int[] {40, 200}; //min and max beats per minute to look for

	private int sampleLength;
	private float time;
	private float [] song;
	private double [][] samples_RE;
	private double [][] samples_IM;
	private double [][][] bandSamples_RE;
	private double [][][] bandSamples_IM;

	private double [] AWeightings;
	private double [] hanning_RE;
	private double [] hanning_IM;
	private float [][] pCharPitches;
	//              ^ hehe lol

	public float sampleTime; //time of each sample (for charPitches and volumes)
	public SortedDictionary <float, float> [] bandBeats; //hashes beat time to power for each band
	public float [] beatTotalPower; //total power for each band during beats, use to rate "matching"
	public float [] charPitches; //list of characteristic pitches for each sample
	public float [] volumes; //list of voumes for each sample
	public bool done = false; //for sync

	public SpectrumAnalyzer (float [] song_in, float time_in) {
		time = time_in;
		song = song_in;

		//calculate sample rate and samples amount
		int nonP2SampleLength = (int) ((float) song.Length / time / idealSampleRate);
		int upper = (int) Mathf.Pow (2, Mathf.Ceil (Mathf.Log (nonP2SampleLength, 2)));
		int lower = (int) Mathf.Pow (2, Mathf.Floor (Mathf.Log (nonP2SampleLength, 2)));
		sampleLength = (Mathf.Abs (upper - nonP2SampleLength) > Mathf.Abs (lower - nonP2SampleLength)) ?
			           lower : upper;
		sampleTime = ((float) sampleLength / song.Length) * time;

		int numSamples = song.Length / sampleLength;
		samples_RE = new double [numSamples][];
		samples_IM = new double [numSamples][];
		bandSamples_RE = new double [bands.Length][][];
		bandSamples_IM = new double [bands.Length][][];
		for (int i = 0; i < bandSamples_RE.Length; i++) {
			bandSamples_RE [i] = new double [numSamples][];
			bandSamples_IM [i] = new double [numSamples][];
		}
		bandBeats = new SortedDictionary <float, float> [bands.Length];
		for (int i = 0; i < bandBeats.Length; i++)
			bandBeats [i] = new SortedDictionary<float, float> ();
		beatTotalPower = new float [bands.Length];

		pCharPitches = new float [numSamples][];
		charPitches = new float [numSamples];
		volumes = new float [numSamples];
	}

	//start run in another thread so it doesn't delay anything
	public void Run () {
		Thread t = new Thread (RunThread);
		t.Start ();
	}

	//Runs EVERYTHING
	public void RunThread () {
		//run FFTs on each sample
		List <Thread> threadsList = new List <Thread> ();
		for (int i = 0; i < samples_RE.Length; i+= (samples_RE.Length / threads)) {
			int end = Mathf.Min (i + (samples_RE.Length / threads), samples_RE.Length);
			Thread thread = new Thread (ThreadFourier);
			threadsList.Add (thread);
			thread.Start (new int[] {i, end});
		}
		foreach (Thread t in threadsList) t.Join ();

		//split it into frequency bands
		threadsList.Clear ();
		for (int i = 0; i < bands.Length; i++) {
			for (int j = 0; j < samples_RE.Length; j += (samples_RE.Length / threads)) {
				int end = Mathf.Min (j + (samples_RE.Length / threads), samples_RE.Length);
				Thread thread = new Thread (BandPass);
				threadsList.Add (thread);
				thread.Start (new int[] {bands [i][0], bands [i][1], i, j, end});
			}
		}
		foreach (Thread t in threadsList) t.Join ();
	
		//get volume and characteristic frequency data
		MakeAWeighting ();
		threadsList.Clear ();
		for (int i = 0; i < bands.Length; i++) {
			Thread thread = new Thread (AddVolumeCharFreq);
			threadsList.Add (thread);
			thread.Start (new int[] {i});
		}
		foreach (Thread t in threadsList) t.Join ();

		//take inverse fourier transform
		InverseFourierList (threadsList);

		//rectify the samples
		threadsList.Clear ();
		for (int i = 0; i < bands.Length; i++) {
			for (int j = 0; j < samples_RE.Length; j += (samples_RE.Length / threads)) {
				int end = Mathf.Min (j + (samples_RE.Length / threads), samples_RE.Length);
				Thread thread = new Thread (FullWaveRectify);
				threadsList.Add (thread);
				thread.Start (new int[] {i, j, end});
			}
		}
		foreach (Thread t in threadsList) t.Join ();

		//take fourier transform
		FourierList (threadsList);

		//convolve with hanning window (to find envelope) while in freq. space
		MakeHanningWindow ();
		threadsList.Clear ();
		for (int i = 0; i < bands.Length; i++) {
			for (int j = 0; j < samples_RE.Length; j += (samples_RE.Length / threads)) {
				int end = Mathf.Min (j + (samples_RE.Length / threads), samples_RE.Length);
				Thread thread = new Thread (Envelope);
				threadsList.Add (thread);
				thread.Start (new int[] {i, j, end});
			}
		}
		foreach (Thread t in threadsList) t.Join ();

		//take inverse fourier transform
		InverseFourierList (threadsList);

		//take the derivative, and half-wave rectify
		threadsList.Clear ();
		for (int i = 0; i < bands.Length; i++) {
			for (int j = 0; j < samples_RE.Length; j += (samples_RE.Length / threads)) {
				int end = Mathf.Min (i + (samples_RE.Length / threads), samples_RE.Length);
				Thread thread = new Thread (TotalDerivative);
				threadsList.Add (thread);
				thread.Start (new int[] {i, j, end});
			}
		}
		foreach (Thread t in threadsList) t.Join ();

		//beats me
		threadsList.Clear ();
		for (int i = 0; i < bands.Length; i++) {
			Thread thread = new Thread (FindBeatsIV);
			threadsList.Add (thread);
			thread.Start (new int[] {i});
		}
		foreach (Thread t in threadsList) t.Join ();

		//smooth charpitches and volumes array
		for (int i = 0; i < pCharPitches.Length; i++) {
			charPitches [i] = pCharPitches [i][0];
		}
		charPitches = SmoothArray (charPitches);
		volumes = SmoothArray (volumes);

		done = true;
		Debug.Log ("done");
	}

	//starts all the threads to take inverse fourier transform
	private void InverseFourierList (List <Thread> threadsList) {
		threadsList.Clear ();
		for (int i = 0; i < bands.Length; i++) {
			for (int j = 0; j < samples_RE.Length; j += (samples_RE.Length / threads)) {
				int end = Mathf.Min (i + (samples_RE.Length / threads), samples_RE.Length);
				Thread thread = new Thread (Fourier);
				threadsList.Add (thread);
				thread.Start (new int[] {i, j, end, 0});
			}
		}
		foreach (Thread t in threadsList) t.Join ();
	}

	//same but regular fourier transform
	private void FourierList (List <Thread> threadsList) {
		threadsList.Clear ();
		for (int i = 0; i < bands.Length; i++) {
			for (int j = 0; j < samples_RE.Length; j += (samples_RE.Length / threads)) {
				int end = Mathf.Min (i + (samples_RE.Length / threads), samples_RE.Length);
				Thread thread = new Thread (Fourier);
				threadsList.Add (thread);
				thread.Start (new int[] {i, j, end, 1});
			}
		}
		foreach (Thread t in threadsList) t.Join ();
	}

	//Splits the array and does FFTs. start and end are indices of the samples array, not of the song.
	private void ThreadFourier (object obj) {
		int [] arr = (int[]) obj;
		int start = arr [0];
		int end = arr [1];
		FFT fft = new FFT ();
		fft.init ((uint) Mathf.Log (sampleLength / fudgeFactor, 2));

		for (int i = start; i < end; i++) {
			samples_RE [i] = new double [sampleLength / fudgeFactor];
			samples_IM [i] = new double [sampleLength / fudgeFactor];
			for (int j = 0; j < samples_RE [i].Length; j++) {
				samples_RE [i][j] = song [i * sampleLength + j * fudgeFactor];
			}
			fft.run (samples_RE [i], samples_IM [i]);
		}
	}

	//Runs an inverse FFT on each of the bands
	private void Fourier (object obj) {
		int [] arr = (int[]) obj;
		int pos = arr [0];
		int start = arr [1];
		int end = arr [2];
		int inverse = arr [3]; // 0 for false, 1 for true
		FFT fft = new FFT ();
		fft.init ((uint) Mathf.Log (bandSamples_RE [pos][0].Length, 2));

		for (int i = start; i < end; i++) {
			fft.run (bandSamples_RE [pos][i], bandSamples_IM [pos][i], (inverse == 0));
		}
	}

	//splits the array of FFT'd samples into an array for each band, then applies filter
	private void BandPass (object obj) {
		int [] arr = (int[]) obj;
		int top = arr [1];
		int bottom = arr [0];
		int pos = arr [2];
		int start = arr [3];
		int end = arr [4];
		bool lowPass = (top < 16000);
		bool highPass = (bottom > 0);

		//compute the filter gain for each sample so it doesn't have to be computed each run
		float [] gains = new float [samples_RE [0].Length]; //get ripped

		for (int i = 0; i < gains.Length / 2; i++) {
			if (lowPass) {
				float freq = (float) i / sampleTime;
				float lowGain = (freq > top) ? 0 : 1;
				gains [i] = lowGain;
				gains [gains.Length - i - 1] = lowGain;
			} else gains [i] = 1;
			if (highPass) {
				float freq = (float) i / sampleTime;
				float highGain = (freq < bottom) ? 0 : 1;
				gains [i] *= highGain;
				gains [gains.Length - i - 1] *= highGain;
			}
		}

		//apply the filter
		for (int i = start; i < end; i++) {
		  	bandSamples_RE [pos][i] = new double [samples_RE [i].Length];
			bandSamples_IM [pos][i] = new double [samples_RE [i].Length];
			for (int j = 0; j < samples_RE [i].Length; j++) {
				bandSamples_RE [pos][i][j] = gains [j] * samples_RE [i][j];
				bandSamples_IM [pos][i][j] = gains [j] * samples_IM [i][j];
			}
		}
	}

	//full wave rectifies (makes all values positive) the sample
	private void FullWaveRectify (object obj) {
		int [] arr = (int[]) obj;
		int pos = arr [0];
		int start = arr [1];
		int end = arr [2];

		for (int i = start; i < end; i++) {
			for (int j = start; j < bandSamples_RE [pos][i].Length; j++) {
				bandSamples_RE [pos][i][j] = (double) Mathf.Abs ((float) bandSamples_RE [pos][i][j]);
			}
		}
	}

	//gets the envelope of a samples in frequency space
	private void Envelope (object obj) {
		int [] arr = (int[]) obj;
		int pos = arr [0];
		int start = arr [1];
		int end = arr [2];

		for (int i = start; i < end; i++) {
			for (int j = 0; j < bandSamples_RE [pos][i].Length; j++) {
				//do a complex multiplication
				double re = bandSamples_RE [pos][i][j];
				double im = bandSamples_IM [pos][i][j];
				bandSamples_RE [pos][i][j] = re * hanning_RE [j] -
											 hanning_IM [j] * im;
				bandSamples_IM [pos][i][j] = re * hanning_IM [j] +
											 hanning_RE [j] * im;
			}
		}
	}

	//Takes the d/dt of the envelope and half-wave rectifies it
	private void TotalDerivative (object obj) {
		int [] arr = (int[]) obj;
		int pos = arr [0];
		int start = arr [1];
		int end = arr [2];

		for (int i = start; i < end; i++) {
			double [] tmp = new double [bandSamples_RE [pos][i].Length];
			for (int j = 0; j < bandSamples_RE [pos][i].Length; j++) {
				tmp [j] = finiteDiff (pos, i, j);
				if (tmp [j] < 0) tmp [j] = 0; //rectifying step
			}
			bandSamples_RE [pos][i] = tmp;
		}
	}

	//convolves large part of song with itself (or at least the largest power of 2) and find peaks
	private void FindBeatsIV (object obj) {
		int [] arr = (int []) obj;
		int pos = arr [0];
		int l1 = bandSamples_RE [pos].Length * bandSamples_RE [pos][0].Length;
		uint log = (uint) Mathf.Log (l1, 2);
		int length = (int) Mathf.Pow (2, log);
		log -= (uint) Mathf.Log (beatSection, 2);
		length /= beatSection;

		//copy all stuff to new arrays
		double [] x_RE = new double [length];
		double [] x_IM = new double [length];
		for (int i = 0; i < length; i++) x_RE [i] = accessor (pos, i);

		//FFT and store copy so it doesn't have to be made twice
		FFT fft = new FFT ();
		fft.init (log);
		fft.run (x_RE, x_IM, false);
		double [] x_freq_RE = new double [length];
		double [] x_freq_IM = new double [length];
		for (int i = 0; i < length; i++) {
			x_freq_IM [i] = x_IM [i];
			x_freq_RE [i] = x_RE [i];
		}

		//convolve by complex multiplying in frequency space
		for (int i = 0; i < length; i++) {
			double re = x_RE [i];
			double im = x_IM [i];
			x_RE [i] = re * re - im * im;
			x_IM [i] = 2 * re * im;
		}
		fft.init (log);
		fft.run (x_RE, x_IM, true);

		//find max displacement (distance between beats)
		float ratio = 0;
		int maxDis = findHarmonics (x_RE, out ratio);
		beatTotalPower [pos] = ratio;
		if (maxDis == 0) return;

		//make comb filter and FFT it
		double [] comb_RE = MakeCombFilter (length, maxDis);
		double [] comb_IM = new double [length];
		fft.init (log);
		fft.run (comb_RE, comb_IM, false);

		//convolve comb filter with FFT'd song
		for (int i = 0; i < length; i++) {
			double re = x_freq_RE [i];
			double im = x_freq_IM [i];
			x_freq_RE [i] = re * comb_RE [i] - im * comb_IM [i];
			x_freq_IM [i] = im * comb_RE [i] + re * comb_IM [i];
		}
		fft.init (log);
		fft.run (x_freq_RE, x_freq_IM, true);

		//find max comb displacement (delay of first beat)
		int maxStart = 0;
		float maxPow = 0;
		for (int i = 0; i < length; i++) {
			if (Mathf.Abs ((float) x_freq_RE [i]) > maxPow) {
				maxPow = Mathf.Abs ((float) x_freq_RE [i]);
				maxStart = i;
			}
		}

		int offset = (int) (maxStart / maxDis);
		int time = maxStart - maxDis * offset;
		while (time < l1) {
			float pow = 0;
			int end = (int) Mathf.Min (l1, time + bpmWindow / 2);
			for (int i = (int) Mathf.Max (0, time - bpmWindow / 2); i < end; i++) {
				pow += (float) accessor (pos, i);
			}

			float actualTime = sampleTime * (float) time / bandSamples_RE [pos][0].Length;
			bandBeats [pos].Add (actualTime, pow);
			time += maxDis;
		}
	}

	//Finds the total volume and characteristic frequency for each sample
	private void AddVolumeCharFreq (object obj) {
		int [] arr = (int[]) obj;
		int pos = arr [0];

		for (int i = 0; i < bandSamples_RE [pos].Length; i++) {
			float localVolume = 0;
			float localMaxFreq = 0;
			float localMaxPow = -1;
			float localPow = -1;

			//check values over each sample
			for (int j = 0; j < bandSamples_RE [pos][i].Length / 2; j++) {
				localPow = Mathf.Pow ((float) bandSamples_RE [pos][i][j], 2) + 
					Mathf.Pow ((float) bandSamples_IM [pos][i][j], 2);
				localPow *= (float) AWeightings [j]; //multiply to adjust for human hearing range
				if (localPow > localMaxPow) {
					localMaxPow = localPow;
					localMaxFreq = j;
				}
				localVolume += localPow;
			}

			//add volume of this frequency band
			lock (volumes) {
				volumes [i] += localVolume; 
			}

			//find max in all frequency bands
			lock (pCharPitches) {
				if (pCharPitches [i] == null) pCharPitches [i] = new float [2];

				if (pCharPitches [i][1] < localMaxPow) {
					pCharPitches [i][1] = localMaxPow;
					float realFreq = localMaxFreq / sampleTime;
					pCharPitches [i][0] = realFreq;
				}
			}
		}
	}

	//takes an already convolved signal and finds not only the max but the
	//closest harmonic of the max offset, the out parameter is the peak strength
	private int findHarmonics (double [] l, out float ratio) {
		float maxBeatTime = 60f / bpmRange [0];
		int maxBeatLength = (int) ((float) bandSamples_RE [0][0].Length * maxBeatTime / sampleTime);
		float minBeatTime = 60f / bpmRange [1];
		int minBeatLength = (int) ((float) bandSamples_RE [0][0].Length * minBeatTime / sampleTime);

		//find the largest peak
		int maxDis = 0;
		float maxPow = 0;
		float avg = 0;
		for (int i = minBeatLength; i < maxBeatLength; i++) {
			avg += Mathf.Abs ((float) l [i]);
			if (Mathf.Abs ((float) l [i]) > maxPow) {
				maxPow = Mathf.Abs ((float) l [i]);
				maxDis = i;
			}
		}
		avg /= l.Length;

		//look for harmonics above a certain percentage of the max value
		int top = maxDis;
		for (int i = 2; i < maxHarmonic; i++) {
			int center = top / i;
			if (center < minBeatLength) continue;
			for (int j = center - bpmWindow / 2; j < center + bpmWindow / 2; j++) {
				if (Mathf.Abs ((float) l [j]) > harmonicTolerance * maxPow) maxDis = j;
			}
		}

		//ratio = maxPow / avg;
		if (maxDis == 0) ratio = -1 * float.MaxValue;
		else ratio = -1 * maxDis;
		return maxDis;
	}

	//makes a comb filter in time domain with certain length and distance dist between peaks
	//spikes are of width bpmwindow
	private double [] MakeCombFilter (int length, int dist) {
		double [] comb = new double [length];
		//make first part of comb filter
		for (int i = 0; i <= bpmWindow / 2; i++) comb [i] = Mathf.Lerp (0, 1, 2 * i / bpmWindow);
		for (int i = bpmWindow / 2; i < bpmWindow; i++) comb [i] = Mathf.Lerp (1, 0, 2 * i / bpmWindow - 1);

		//copy it; - dist is to prevent half-combs
		for (int i = dist; i < length - dist; i++) {
			comb [i] = comb [i - dist];
		}

		return comb;
	}

	//calculates the Fourier transform of the relevantly sized Hanning window
	private void MakeHanningWindow () {
		hanning_RE = new double [samples_RE [0].Length];
		hanning_IM = new double [hanning_RE.Length];

		//create time-domain hanning window
		int cutoff = (int) ((hanningTime / sampleTime) * (float) hanning_RE.Length);
		for (int i = 0; i <= cutoff; i++) {
			hanning_RE [i] = .5f * (1 - Mathf.Cos (2 * Mathf.PI * (i + cutoff) / (cutoff - 1)));
		}

		//calculate fourier transform
		FFT fft = new FFT ();
		fft.init ((uint)Mathf.Log (hanning_RE.Length, 2));
		fft.run (hanning_RE, hanning_IM);
	}

	//creates A-weighting curve for sample size
	private void MakeAWeighting () {
		AWeightings = new double [samples_RE [0].Length];
		double c1 = 12200 * 12200;
		double c2 = 20.6 * 20.6;
		double c3 = 107.7 * 107.7;
		double c4 = 737.9 * 737.9;
	
		for (int i = 0; i < AWeightings.Length; i++) {
			double f2 = i * i / sampleTime / sampleTime;
			AWeightings [i] = (c1 * f2 * f2) / ((f2 + c2) * Mathf.Sqrt ((float) ((f2 + c3) * (f2 + c4))) * (f2 + c1));
		}
	}

	//takes the 1st-order finite difference derivative of point j in sample i in band pos
	private double finiteDiff (int pos, int i, int j) {
		if (i == 0 && j == 0) {
			return (bandSamples_RE [pos][i][j + 1] - bandSamples_RE [pos][i][j]);
		} else if (i == bandSamples_RE [pos][i].Length - 1 && 
		           j == bandSamples_RE [pos].Length) {
			return (bandSamples_RE [pos][i][j] - bandSamples_RE [pos][i][j-1]);
		} else {
			double prev = 0, next = 0;
			if (j == bandSamples_RE [pos][i].Length - 1)
				next = bandSamples_RE [pos][i + 1][0];
			else next = bandSamples_RE [pos][i][j + 1];

			if (j == 0) 
				prev = bandSamples_RE [pos][i - 1][bandSamples_RE [pos][i - 1].Length - 1];
			else prev = bandSamples_RE [pos][i][j - 1];
			
			return (next - prev) / 2f;
		}
	}

	//smooths array (for volume and characteristic pitch)
	private float [] SmoothArray (float [] arr) {
		float [] newArr = new float [arr.Length];

		for (int i = 0; i < arr.Length; i++) {
			int start = i - smoothWindow;
			int end = i + smoothWindow;

			//adjust for boundary conditions
			while (start < 0) start++;
			while (end >= arr.Length) end--;

			float avg = 0;
			for (int j = start; j <= end; j++) avg += arr [j];
			avg /= (end - start + 1);
			newArr [i] = avg;
		}

		return newArr;
	}

	//finds the ith element (regardless of sample) in band pos
	private double accessor (int pos, int i) {
		int sample = i / bandSamples_RE [pos][0].Length;
		int j = i % bandSamples_RE [pos][0].Length;
		return bandSamples_RE [pos][sample][j];
	}

	/*//find beats by checking similarity with several general beat patterns
	private void FindBeatsII (object obj) {
		int [] arr = (int[]) obj;
		int pos = arr [0];
		int length = bandSamples_RE [pos].Length * bandSamples_RE [pos][0].Length;
		
		//find the max value in the band
		double max = 0;
		int maxPos = 0;
		for (int i = 0; i < length; i++) {
			double curr = accessor (pos, i);
			if (curr > max) {
				max = curr;
				maxPos = i;
			}
		}
		
		//convert bpm to discrete sample sizes
		float maxBeatTime = (float) 60 / bpmRange [1];
		float minBeatTime = (float) 60 / bpmRange [0];
		int shortBeatLength = (int) (maxBeatTime * (float) length / time);
		int longBeatLength = (int) (minBeatTime * (float) length / time);
		
		//space other beats evenly around max
		int maxBeatLength = shortBeatLength;
		double maxBeatPower = 0;
		List <int> maxBeats = new List <int> ();
		List <int> vals = new List <int> ();
		List <double> pows = new List <double> ();
		for (int i = shortBeatLength; i < longBeatLength; i ++) {
			//add all points that correspond to beats
			vals.Clear ();
			pows.Clear ();
			int checker = maxPos;
			while (checker + bpmWindow / 2 < length) {
				vals.Add (checker);
				checker += i;
			}
			checker = maxPos;
			do {
				checker -= i;
				vals.Add (checker);
			} while (checker > i + bpmWindow / 2);
			
			//find their total
			foreach (int val in vals) {
				double sum = 0;
				for (int j = val - bpmWindow / 2; j < val + bpmWindow / 2; j++) {
					pows.Add (sum);
					sum += accessor (pos, j);
				}
			}
			
			double pow = 0;
			for (int k = 2; k < vals.Count - 2; k++) {
				pow += pows [k - 2] * pows [k - 1] * pows [k] * pows [k + 1] * pows [k + 2];
			}
			
			if (pow > maxBeatPower) {
				maxBeatPower = pow;
				maxBeatLength = i;
				maxBeats.Clear ();
				maxBeats.AddRange (vals);
			}
		}
		
		//put max beat power into array so beats can be compared by match
		beatTotalPower [pos] = (float) maxBeatPower;
		
		//add each beat time to array
		foreach (int i in maxBeats) {
			float currTime = (float) i * time / length;
			float pow = (float) accessor (pos, i);
			bandBeats [pos].Add (currTime, pow);
		}
	}*/

	/*//for debug: makes a certain band of the song playable (only works if fudgefactor is 1)
	private void MakeBand (int bandNum) {
		band = new float [song.Length];
		for (int i = 0; i < bandSamples_RE [bandNum].Length; i++) {
			for (int j = 0; j < bandSamples_RE [bandNum][i].Length; j++) {
				band [i * sampleLength + j] = (float) bandSamples_RE [bandNum][i][j];
			}
		}
	}*/

	/*//looks for peaks above a certain threshold
	private void FindBeats (object obj) {
		int [] arr = (int[]) obj;
		int pos = arr [0];
		
		//find the max value in the band
		double max = 0;
		for (int i = 0; i < bandSamples_RE [pos].Length; i++) {
			for (int j = 0; j < bandSamples_RE [pos][i].Length; j++) {
				if (bandSamples_RE [pos][i][j] > max) max = bandSamples_RE [pos][i][j];
			}
		}
		
		//find all beats > maxfactor*max
		bool onset = false;
		for (int i = 0; i < bandSamples_RE [pos].Length; i++) {
			for (int j = 0; j < bandSamples_RE [pos][i].Length; j++) {
				double current = bandSamples_RE [pos][i][j];
				float time = ((float) j / bandSamples_RE [pos][i].Length) * sampleTime + i * sampleTime;
				if (!onset && current > max * maxFactor) {
					onset = true;
					lock (beats) {
						beats.Add (time, (float) current);
					}
				} else if (onset && current < max * maxFactor) {
					onset = false;
				}
			}
		}
	}*/

	/*
	private void FindBeatsIII (object obj) {
		int [] arr = (int[]) obj;
		int pos = arr [0];
		int length = bandSamples_RE [pos].Length * bandSamples_RE [pos][0].Length;

		//params (might be promoted to global variables)
		int pulseWindow = 32; //how large of a range around each potential beat to check
		int beatIncrement = 4; //how much time to add to beat each iteration
		int lookAhead = 2; //how many beats to look ahead for
		float constrainMax = .125f; //how many seconds to look around beat for max value
		int lookBehind = 4;

		//convert bpm to discrete sample sizes
		float maxBeatTime = (float) 60 / bpmRange [1];
		float minBeatTime = (float) 60 / bpmRange [0];
		int shortBeatLength = (int) (maxBeatTime * (float) length / time);
		int longBeatLength = (int) (minBeatTime * (float) length / time);
		int cmLength = (int) (constrainMax * (float) length / time);

		//iterate through the array, looking at the next five points for each beat length
		//between short and long. Find the one with maximum resonance. Then go to the next
		//point.
		int end = length - lookAhead * longBeatLength - pulseWindow / 2 - cmLength / 2;
		int predictedBeat = lookBehind * longBeatLength + pulseWindow / 2 + cmLength / 2;
		double max_pow = 0, pow = 0, sum = 0;
		int sstart = 0, send = 0;
		int max_length = 0;
		while (predictedBeat < end) {
			max_pow = 0;
			int ssend = predictedBeat + cmLength / 2;
			for (int i = predictedBeat - cmLength / 2; i < ssend; i++) {
				if (accessor (pos, i) > max_pow) {
					max_pow = accessor (pos, i);
					predictedBeat = i;
				}
			}

			max_pow = 0;
			max_length = shortBeatLength;

			for (int i = shortBeatLength; i < longBeatLength; i += beatIncrement) {
				pow = 1;

				for (int j = -1 * lookBehind; j < lookAhead; j++) {
					sum = 0;
					sstart = (predictedBeat + j * i) - pulseWindow / 2;
					send = sstart + pulseWindow;
					for (int k = sstart; k < send; k++) sum += accessor (pos, k);
					pow *= (1 + sum);
				}

				if (pow > max_pow) {
					max_pow = pow;
					max_length = i;
				}
			}

			float currTime = (float) predictedBeat * time / length;
			float power = (float) accessor (pos, predictedBeat);
			bandBeats [pos].Add (currTime, power);

			predictedBeat += max_length;
		}
	}
	*/

	/*//Computes gain for 5th order chebyshev filter
	private float Chebyshev (float frq, float cutoff, float ripple) {
		float ww0 = frq / cutoff; // w/w0
		float polynomial = 16 * Mathf.Pow (ww0, 5) - 20 * Mathf.Pow (ww0, 3) + 5 * ww0;
		return 1 / Mathf.Sqrt (1 + Mathf.Pow (ripple, 2) * Mathf.Pow (polynomial, 2));
	}
	
	//Same but type II chebyshev filter
	private float ChebyshevII (float frq, float cutoff, float ripple) {
		float w0w = cutoff / frq; // w0/w
		float polynomial = 16 * Mathf.Pow (w0w, 5) - 20 * Mathf.Pow (w0w, 3) + 5 * w0w;
		return 1 / Mathf.Sqrt (1 + 1 / (Mathf.Pow (ripple, 2) * Mathf.Pow (polynomial, 2)));
	}
	
	//performs nth order butterworth filter
	private float Butterworth (float frq, float cutoff, float n) {
		return 1 / (1 + Mathf.Pow (frq / cutoff, n * 2));
	}*/

	//computes power from bottom to top in sample i
	/*private float BandPower (int i, int bottom, int top) {
		float total = 0;
		for (int j = bottom; j < top; j++) {
			total += Mathf.Pow ((float) samples_IM [i][j], 2) + Mathf.Pow ((float) samples_RE [i][j], 2);
		}
		return total;
	}*/

	//goes over a certain frequency range looking for beats and characteristic frequency.
	/*private void BandPass (object obj) {
		int [] arr = (int[]) obj;
		int top = arr [1];
		int bottom = arr [0];

		//set up average of past samplesBackcheck values
		Queue <float> formerValues = new Queue <float> ();
		float average = 0;
		for (int i = 0; i < samplesBackcheck; i++) {
			float pow = BandPower (i, bottom, top);
			average += pow;
			formerValues.Enqueue (pow);
		}
		average /= samplesBackcheck;

		//find beats by comparing with the average of past values
		float sampleTime = time * (float) sampleLength / song.Length;
		for (int i = samplesBackcheck; i < samples_RE.Length; i++) {
			float pow = BandPower (i, bottom, top);
			//comparison here
			if (pow > c * average) {
				beats.Add (i * sampleTime, pow);
			}

			//update average
			float toRemove = formerValues.Dequeue ();
			formerValues.Enqueue (pow);
			average += pow / samplesBackcheck;
			average -= toRemove / samplesBackcheck;
		}
	}*/
	
	//fills out the characteristic pitch array with values from each sample
	/*private void CharPitch () {

	}*/

	/*private void RemoveCloseBeats () {
		float last = -1;
		List <float> ToRemove = new List <float> ();
		foreach (float f in beats.Keys) {
			if (f - last < closeBeatThreshold) ToRemove.Add (f);
			last = f;
		}
		
		foreach (float f in ToRemove) beats.Remove (f);
	}*/

	//private float maxFactor = .1f; // % of max it needs to be to be considered a peak
	//private float rippleFactor = 2; //ripple factor for chebyshev filter
	//private int bandNum = 64;
	//private int samplesBackcheck = 15; //amount of past samples to compare to when doing beats
	//private int c = 5; //factor pow > c*pow_avg for it to be a beat
	//private float closeBeatThreshold = 1f / 32;
	//private int sampleLength = 1024; //length of each sample to be used with FFT, must be power of 2
	//public float [] band;
}

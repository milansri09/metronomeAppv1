namespace MetronomeApp;
using Plugin.Maui.Audio;

public partial class MainPage : ContentPage
{
	int count = 0;
	bool play_sound = false;
	AudioPlayer Beat1Player;
	AudioPlayer EmphasizedBeat1Player;
	DateTime LastBeatPlayed = DateTime.Now;
	int TriangleFlashCount = 0;
	int BeatsPerMeasure = 4;
	List <string> Dropdown = new List <string> ();
	List <AudioPlayer> BeatSounds = new List <AudioPlayer> ();
	List<bool> IsEmphasized = new List<bool>();

	public MainPage()
	{
		Dropdown.Add("Beat 1");
        Dropdown.Add("Beat 2");
        Dropdown.Add("Beat 3");
        Dropdown.Add("Beat 4");

		IsEmphasized.Add(true);
		IsEmphasized.Add(false);
        IsEmphasized.Add(false);
        IsEmphasized.Add(false);

        InitializeComponent();
		BeatSelector.ItemsSource = Dropdown;
        Device.StartTimer(TimeSpan.FromMilliseconds(10), () =>
		{
			if(play_sound)
			{
				DateTime now = DateTime.Now;
				TimeSpan time_span = now - LastBeatPlayed;
				int SliderTime = 60000 / (int)(SliderTempo.Value);
			if (time_span.TotalMilliseconds >= SliderTime)
			{
				Beat1Player.Stop();
				EmphasizedBeat1Player.Stop();
				if (BeatSounds[TriangleFlashCount % BeatsPerMeasure] == Beat1Player)
				{
					Beat1Player.Play();
				}
				else
				{
					if (IsEmphasized[TriangleFlashCount % BeatsPerMeasure])
					{
						EmphasizedBeat1Player.Play();
					}
					else
					{
						Beat1Player.Play();
					}
				}

					TriangleFlashCount++;
					LastBeatPlayed = now;
					Device.BeginInvokeOnMainThread(async () =>
					{
						if (IsEmphasized[(TriangleFlashCount - 1) % BeatsPerMeasure])
						{
							TriangleBig.IsVisible = false;
							TriangleEmphasizedBig.IsVisible = true;
							TriangleFlash.IsVisible = false;
							//float time = 1200 + ((1200 - 125) / (10 - 300)) * (int)(SliderTempo.Value);
							await Task.Delay(100);
							TriangleBig.IsVisible = false;
							TriangleEmphasizedBig.IsVisible = false;
							TriangleFlash.IsVisible = true;
						}
						else {
                            TriangleBig.IsVisible = true;
                            TriangleFlash.IsVisible = false;
                            //float time = 1200 + ((1200 - 125) / (10 - 300)) * (int)(SliderTempo.Value);
                            await Task.Delay(100);
                            TriangleBig.IsVisible = false;
                            TriangleFlash.IsVisible = true;
                        }
                    });
                }
			}
			return true;
		});
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        Beat1Player = (AudioPlayer)AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("Beat1.55.wav"));
        EmphasizedBeat1Player = (AudioPlayer)AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("EmphasizedBeat1.wav"));
		BeatSounds.Add(EmphasizedBeat1Player);
		BeatSounds.Add(Beat1Player);
        BeatSounds.Add(Beat1Player);
        BeatSounds.Add(Beat1Player);
    }

	int taps = 0;
	DateTime LastTap;
	List<float> MultipleTimeElapsed = new List<float>();
    private async void Beats(object sender, TappedEventArgs e)
    {
		if(ModeSelector.SelectedIndex <= 0)
		{
            if (play_sound)
            {
                play_sound = false;
            }
            else
            {
                play_sound = true;
                TriangleFlashCount = 0;
            }
        }
		else
		{
			if(taps == 0)
			{
				LastTap = DateTime.Now;
				taps++;
			}
			else
			{
				float TimeElapsed = (float)(DateTime.Now - LastTap).TotalSeconds;
				MultipleTimeElapsed.Add(TimeElapsed);
				taps++;
				LastTap = DateTime.Now;

				if(MultipleTimeElapsed.Count > (.1 * SliderTempo.Value))
				{
					MultipleTimeElapsed.RemoveAt(0);
				}

				float TimeSum = MultipleTimeElapsed.Sum();
				float AverageTimeElapsed = TimeSum / MultipleTimeElapsed.Count();
				
				if(TimeElapsed >= 1.5*AverageTimeElapsed)
				{
					MultipleTimeElapsed.Clear();
					LastTap = DateTime.Now;
					taps = 1;
					return;
				}

				float BeatsPerMinute = 60 / AverageTimeElapsed;
				if(BeatsPerMinute > 300)
				{
					BeatsPerMinute = 300;
				}
				SliderBeats.Text = ((int)(BeatsPerMinute)).ToString();
			}
		}
    }

    private void SliderTempo_ValueChanged(object sender, ValueChangedEventArgs e)
    {
		SliderTempo.Value = (int)(SliderTempo.Value);
		SliderBeats.Text = SliderTempo.Value.ToString();
    }

    private void SliderBeats_TextChanged(object sender, TextChangedEventArgs e)
    {
		try
		{
            SliderTempo.Value = int.Parse(SliderBeats.Text);
        }

		catch (Exception ex)
		{

		}
    }

    private void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
		Entry box = (Entry)sender;
		if(box.Text != "")
		{
            BeatsPerMeasure = int.Parse(box.Text);
			for(int i = BeatSounds.Count();  i < BeatsPerMeasure; i++) {
				BeatSounds.Add(Beat1Player);
			}

			Dropdown = new List<string> ();
			for(int i = 0; i < BeatsPerMeasure; i++)
			{
				Dropdown.Add("Beat " + (i + 1).ToString());
			}
			BeatSelector.ItemsSource = Dropdown;

            for (int i = IsEmphasized.Count(); i < BeatsPerMeasure; i++)
            {
                IsEmphasized.Add(false);
            }
        }
    }

    private void Entry_TextChanged_1(object sender, TextChangedEventArgs e)
    {

    }

    private void BeatSelector_SelectedIndexChanged(object sender, EventArgs e)
    {
		int index = BeatSelector.SelectedIndex;
		if(index == -1) { return; }
        Soundbox1.BackgroundColor = Color.FromHex("#ffffff");
        Soundbox2.BackgroundColor = Color.FromHex("#ffffff");

        IsEmphasized[index] = BeatEmphasizer.IsToggled;

		if (IsEmphasized[index])
		{
            if (BeatSounds[index] == Beat1Player)
            {
                Soundbox1.BackgroundColor = Color.FromHex("#6bfc03");
            }
            else if (BeatSounds[index] == EmphasizedBeat1Player)
            {
                Soundbox2.BackgroundColor = Color.FromHex("#6bfc03");
            }
        }
		else
		{
            if (BeatSounds[index] == Beat1Player)
            {
                Soundbox1.BackgroundColor = Color.FromHex("#e80707");
            }
            else if (BeatSounds[index] == EmphasizedBeat1Player)
            {
                Soundbox2.BackgroundColor = Color.FromHex("#e80707");
            }
        }
    }

    private void ChooseSoundbox(object sender, TappedEventArgs e)
    {
		Frame ClickedSoundbox = (Frame)sender;
		int index = BeatSelector.SelectedIndex;
        if (index == -1) { return; }

        if (ClickedSoundbox.ClassId == "Soundbox1")
		{
			BeatSounds[index] = Beat1Player;
		}
		else if(ClickedSoundbox.ClassId == "Soundbox2")
		{
			BeatSounds[index] = EmphasizedBeat1Player;
		}

		BeatSelector_SelectedIndexChanged(null, null);
    }

    private void BeatEmphasizer_Toggled(object sender, ToggledEventArgs e)
    {

    }
}


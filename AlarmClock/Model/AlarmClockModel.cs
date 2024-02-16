using AlarmClock.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace AlarmClock.Model
{
    public partial class AlarmClockModel : ObservableObject
    {        

        [ObservableProperty]
        private string? _timeWakeUpModel;

        [ObservableProperty]
        private string? _currentTimeModel;

        [ObservableProperty]
        private string? _clockStatusModel;

        [ObservableProperty]
        private string? _musicLocationModel;

        [ObservableProperty]
        private string? _musicLocationShowModel;

        [ObservableProperty]
        private bool? _startButtonEnabledModel;

        [ObservableProperty]
        private bool? _stopButtonEnabledModel;

        [ObservableProperty]
        private bool? _setOffButtonEnabledModel;

        [ObservableProperty]
        private bool? _isDropDownOpenModel;

        [ObservableProperty]
        private bool? _isToggleButtonCheckedModel;

        [ObservableProperty]
        private bool _alarmClockIsSelectedModel;

        [ObservableProperty]
        private int _timeToPostponeModel;

        [ObservableProperty]
        private DispatcherTimer? _timerAlarmModel;

        [ObservableProperty]
        private DispatcherTimer? _timerSetOffModel;

        [ObservableProperty]
        private MediaPlayer? _soundPlayerModel;


        [RelayCommand]
        private void StartAlarmClock()
        {
            ClockStatusModel = $" Pognali \r\n ostanovlus v - {TimeWakeUpModel},\r\n tekushchee vremia -";

            if (IsToggleButtonCheckedModel == false)
            {
                StopAlarm();
                return;
            }

            TimerAlarmModel!.Interval = TimeSpan.FromSeconds(1);
            TimerAlarmModel.Tick += AlarmStartModel!;
            TimerAlarmModel.Start();            
        }

        [RelayCommand]
        private void StopAlarmClockModel()
        {
            ClockStatusModel = "Stape";

            StopAlarm();
        }        

        [RelayCommand]
        private void SetOffTheAlarmModel()
        {
            ClockStatusModel = $"The alarm has been postponed for {TimeToPostponeModel} minuts";

            SetOffButtonEnabledModel = false;
            SoundPlayerModel!.Stop();

            TimerSetOffModel!.Interval = TimeSpan.FromSeconds(TimeToPostponeModel);
            TimerSetOffModel.Tick += AlarmSoundModel;
            TimerSetOffModel.Start();
        }


        private void StopAlarm()
        {
            StopButtonEnabledModel = false;
            IsDropDownOpenModel = false;
            SetOffButtonEnabledModel = false;

            TimerSetOffModel!.Stop();
            SoundPlayerModel!.Stop();
        }

        private void AlarmStartModel(object sender, EventArgs e)
        {
            var wakeUpTime = DateTime.Parse(TimeWakeUpModel!);
            var currentTime = DateTime.Now;
            CurrentTimeModel = currentTime.ToString("HH:mm:ss");

            if (currentTime.Hour == wakeUpTime.Hour && currentTime.Minute == wakeUpTime.Minute && currentTime.Second == wakeUpTime.Second)
            {
                if (IsToggleButtonCheckedModel == true)
                {
                    AlarmSoundPlayerModel();
                }
                return;
            }
        }

        private void AlarmSoundPlayerModel()
        {
            StopButtonEnabledModel = true;
            SetOffButtonEnabledModel = true;

            SoundPlayerModel!.Open(new Uri(MusicLocationModel!.Trim('"')));
            SoundPlayerModel.Play();
            SoundPlayerModel.MediaEnded += (sender, e) =>
            {
                SoundPlayerModel.Position = TimeSpan.Zero;
                SoundPlayerModel.Play();
            };
        }        

        private void AlarmSoundModel(object? sender, EventArgs e)
        {
            AlarmSoundPlayerModel();
        }
    }
}

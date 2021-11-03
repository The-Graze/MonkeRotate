
namespace MonkeSwim.Tools.Averages
{
    public struct SwimSettingsAverage
    {
        private readonly float maxSpeed;
        private readonly float acceleration;
        private readonly float resistence;

        private readonly uint settingsAmount;

        public float MaxSpeed { 
            get { return Average(maxSpeed, settingsAmount); }
            private set { }
        }

        public float Acceleration {
            get { return Average(acceleration, settingsAmount); }
            private set { }
        }

        public float Resistence {
            get { return Average(resistence, settingsAmount); }
            private set { }
        }

        public uint Amount {
            get { return settingsAmount; }
            private set { }
        }

        public static SwimSettingsAverage Zero { 
            get { return new SwimSettingsAverage(0f, 0f, 0f, 0); }
            private set { }
        } 

        public SwimSettingsAverage(float mSpeed, float accel, float resist, uint setAmount = 1)
        {
            maxSpeed = mSpeed;
            acceleration = accel;
            resistence = resist;
            settingsAmount = setAmount;
        }

        public static SwimSettingsAverage operator +(SwimSettingsAverage settings, Config.SwimSettings newSettings)
        {
            return new SwimSettingsAverage(settings.maxSpeed + newSettings.MaxSpeed,
                                           settings.acceleration + newSettings.Acceleration,
                                           settings.settingsAmount + 1);
        }

        public static SwimSettingsAverage operator -(SwimSettingsAverage settings, Config.SwimSettings newSettings)
        {
            return new SwimSettingsAverage(settings.maxSpeed - newSettings.MaxSpeed,
                                           settings.acceleration - newSettings.Acceleration,
                                           settings.settingsAmount - 1);
        }

        private static float Average(float toAverage, uint amount)
        {
            return (amount > 1 && toAverage != 0f) ? toAverage / amount : toAverage;
        }
    }
}
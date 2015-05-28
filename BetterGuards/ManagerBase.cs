using BahaTurret;
using UnityEngine;

namespace BetterGuards
{
    public class ManagerBase : PartModule
    {
        public AudioSource AudioSource;
        public AudioSource WarningAudioSource;

        #region guibuttons
        [KSPField(guiActiveEditor = true, isPersistant = true, guiActive = true, guiName = "Team: "),
            UI_Toggle(disabledText = "A", enabledText = "B")]
        public bool Team = false;
        #endregion

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            AudioSource = InitializeAudioSource();
            WarningAudioSource = InitializeAudioSource();

        }

        #region actions
        [KSPAction("Toggle Team")]
        public void AgToggleTeam(KSPActionParam param)
        {
            PlaySound("click");
            Team = !Team;

            Debug.Log("Toggle Team: " + Team);
        }
        #endregion

        #region audio
        public AudioSource InitializeAudioSource()
        {
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.minDistance = 500;
            audioSource.maxDistance = 1000;
            audioSource.dopplerLevel = 0;
            audioSource.volume = Mathf.Sqrt(GameSettings.UI_VOLUME);

            return audioSource;
        }

        public void PlaySound(string name, bool warning = false)
        {
            var clip = GameDatabase.Instance.GetAudioClip("BetterGuards/Sounds/" + name);

            if (warning)
            {
                WarningAudioSource.PlayOneShot(clip);
            }
            else
            {
                AudioSource.PlayOneShot(clip);
            }

            Debug.Log("Sound play: " + name);
        }
        #endregion
    }
}

using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Sounds
{
    public class DarkestSoundManager : MonoBehaviour
    {
        public static DarkestSoundManager Instanse { get; private set; }
        private FMOD.Studio.System _studio;

        private EventInstance _dungeonInstanse;
        private EventInstance _battleInstanse;
        private EventInstance _campingInstanse;
        private EventInstance _campingMusicInstanse;
        private EventInstance _townInstanse;
        private EventInstance _townMusicInstanse;
        public static EventInstance TitleMusicInstanse { get; private set; }
        public static EventInstance StatueAudioInstanse { get; private set; }

        public static List<EventInstance> NarrationQueue { get; private set; }
        public static EventInstance CurrentNarration { get; private set; }
        private static PLAYBACK_STATE narrationState;

        private static float _soundLevel = 0.02F;

        public float SoundLevelGet()
        {
            return _soundLevel;
        }

        private void SoundSetLevel(EventInstance eventInstance)
        {
            Debug.Log("DarkestSoundManager | SoundSetLevel");
            AudioListener.volume = _soundLevel;
            if (eventInstance != null)
            {
                var result = eventInstance.setVolume(_soundLevel);
                Debug.Log(result);
            }
        }

        void Awake()
        {
            if (Instanse == null)
            {
                Instanse = this;
                _studio = RuntimeManager.StudioSystem;
                NarrationQueue = new List<EventInstance>();
            }
        }

        void Update()
        {
            if (CurrentNarration == null)
            {
                if (NarrationQueue.Count > 0)
                {
                    CurrentNarration = NarrationQueue[0];
                    CurrentNarration.setVolume(_soundLevel);
                    CurrentNarration.start();
                }
            }
            else
            {
                Debug.Log("CurrentNarration != null | Update");
                CurrentNarration.getPlaybackState(out narrationState);
                if (narrationState == PLAYBACK_STATE.STOPPED || narrationState == PLAYBACK_STATE.STOPPING)
                {
                    CurrentNarration.release();
                    NarrationQueue.Remove(CurrentNarration);
                    CurrentNarration = null;
                }
            }
        }

        public void ExecuteNarration(string id, NarrationPlace place, params string[] tags)
        {
            // + enter_hallway + half_health_half_stress
            if (!DarkestDungeonManager.Data.Narration.ContainsKey(id))
                return;

            NarrationEntry narrationEntry = DarkestDungeonManager.Data.Narration[id];

            if (!RandomSolver.CheckSuccess(narrationEntry.Chance))
                return;

            var possibleEvents = narrationEntry.AudioEvents.FindAll(audioEvent => audioEvent.IsPossible(place, tags));
            if (possibleEvents.Count == 0)
                return;

            float maxPriority = possibleEvents.Max(audio => audio.Priority);
            possibleEvents.RemoveAll(lowPriorityEvent => lowPriorityEvent.Priority < maxPriority);

            NarrationAudioEvent narrationEvent = id == "combat_start" ?
                possibleEvents[0] : possibleEvents[RandomSolver.Next(possibleEvents.Count)];

            if (narrationEvent.QueueOnlyOnEmpty && NarrationQueue.Count > 0)
                return;

            if (id == "town_visit_start")
            {
                for (int i = 0; i < 3; i++)
                {
                    if (RandomSolver.CheckSuccess(narrationEvent.Chance))
                        break;
                    else
                        narrationEvent = possibleEvents[RandomSolver.Next(possibleEvents.Count)];

                    if (i == 2)
                        return;
                }
            }

            else if (!RandomSolver.CheckSuccess(narrationEvent.Chance))
                return;

            var narrationInstanse = RuntimeManager.CreateInstance("event:" + narrationEvent.AudioEvent);
            if (narrationInstanse != null)
                NarrationQueue.Add(narrationInstanse);

            switch (place)
            {
                case NarrationPlace.Campaign:
                    if (narrationEvent.MaxCampaignOccurrences > 0)
                    {
                        if (!DarkestDungeonManager.Campaign.NarrationCampaignInfo.ContainsKey(narrationEvent.AudioEvent))
                            DarkestDungeonManager.Campaign.NarrationCampaignInfo.Add(narrationEvent.AudioEvent, 0);

                        DarkestDungeonManager.Campaign.NarrationCampaignInfo[narrationEvent.AudioEvent]++;
                    }
                    break;
                case NarrationPlace.Raid:
                    if (narrationEvent.MaxRaidOccurrences > 0)
                    {
                        if (!DarkestDungeonManager.Campaign.NarrationRaidInfo.ContainsKey(narrationEvent.AudioEvent))
                            DarkestDungeonManager.Campaign.NarrationRaidInfo.Add(narrationEvent.AudioEvent, 0);

                        DarkestDungeonManager.Campaign.NarrationRaidInfo[narrationEvent.AudioEvent]++;
                    }
                    goto case NarrationPlace.Campaign;
                case NarrationPlace.Town:
                    if (narrationEvent.MaxTownVisitOccurrences > 0)
                    {
                        if (!DarkestDungeonManager.Campaign.NarrationTownInfo.ContainsKey(narrationEvent.AudioEvent))
                            DarkestDungeonManager.Campaign.NarrationTownInfo.Add(narrationEvent.AudioEvent, 0);

                        DarkestDungeonManager.Campaign.NarrationTownInfo[narrationEvent.AudioEvent]++;
                    }
                    goto case NarrationPlace.Campaign;
            }
        }

        public void PlayStatueAudioEntry(string id)
        {
            if (CurrentNarration != null && NarrationQueue.Count > 0)
                return;

            var narrationInstanse = RuntimeManager.CreateInstance(id);
            if (narrationInstanse != null)
                NarrationQueue.Add(narrationInstanse);
        }

        public void SilenceNarrator()
        {
            if (CurrentNarration != null)
            {
                CurrentNarration.stop(STOP_MODE.ALLOWFADEOUT);
                CurrentNarration.release();
                CurrentNarration = null;
                NarrationQueue.Clear();
            }
        }

        private readonly float _oneShotSoundLevel = 0.07F;

        // TODO: Not all sounds volume are controlled, need to be fixed
        private EventInstance _oneShotAudioInstanse;
        public void PlayOneShot(string eventId)
        {
            Debug.Log("PlayOneShot: " + eventId);
            _oneShotAudioInstanse = RuntimeManager.CreateInstance(eventId);
            _oneShotAudioInstanse.setVolume(_oneShotSoundLevel);
            _oneShotAudioInstanse.start();
        }

        public void PlayOneShot(string eventId, Vector3 position)
        {
            Debug.Log("PlayOneShot: " + eventId + " | position: " + position);

            _oneShotAudioInstanse = RuntimeManager.CreateInstance(eventId);
            _oneShotAudioInstanse.setVolume(_oneShotSoundLevel);
            _oneShotAudioInstanse.set3DAttributes(position.To3DAttributes());
            _oneShotAudioInstanse.start();
        }

        public void PlayTitleMusic(bool isIntro)
        {
            StopTitleMusic();

            Debug.Log("PlayTitleMusic");

            if (isIntro)
                TitleMusicInstanse = RuntimeManager.CreateInstance("event:/music/_music_assets/title_intro");
            else
                TitleMusicInstanse = RuntimeManager.CreateInstance("event:/music/_music_assets/title_outro");

            SoundSetLevel(TitleMusicInstanse);

            if (TitleMusicInstanse != null)
                TitleMusicInstanse.start();
        }

        public void StopTitleMusic()
        {
            if (TitleMusicInstanse != null)
            {
                TitleMusicInstanse.stop(STOP_MODE.ALLOWFADEOUT);
                TitleMusicInstanse.release();
            }
        }

        public void StartDungeonSoundtrack(string dungeonName)
        {
            StopDungeonSoundtrack();

            if (dungeonName == "darkestdungeon")
                _dungeonInstanse = RuntimeManager.CreateInstance("event:/ambience/dungeon/quest/" + RaidSceneManager.Raid.Quest.Id);
            else
                _dungeonInstanse = RuntimeManager.CreateInstance("event:/ambience/dungeon/" + dungeonName);

            SoundSetLevel(_dungeonInstanse);
            if (_dungeonInstanse != null)
                _dungeonInstanse.start();
        }

        public void ContinueDungeonSoundtrack(string dungeonName)
        {
            if (_dungeonInstanse != null)
                _dungeonInstanse.setPaused(false);
            else
                StartDungeonSoundtrack(dungeonName);
        }

        public void PauseDungeonSoundtrack()
        {
            if (_dungeonInstanse != null)
                _dungeonInstanse.setPaused(true);
        }

        public void StopDungeonSoundtrack()
        {
            if (_dungeonInstanse != null)
            {
                _dungeonInstanse.stop(STOP_MODE.ALLOWFADEOUT);
                _dungeonInstanse.release();
            }
        }

        public void StartBattleSoundtrack(string dungeonName, bool isRoom)
        {
            StopBattleSoundtrack();

            _battleInstanse = RuntimeManager.CreateInstance("event:/music/mus_battle_" +
                dungeonName + (isRoom ? "_room" : "_hallway"));
            SoundSetLevel(_battleInstanse);

            if (_battleInstanse != null)
                _battleInstanse.start();
        }

        public void StopBattleSoundtrack()
        {
            if (_battleInstanse != null)
            {
                _battleInstanse.stop(STOP_MODE.ALLOWFADEOUT);
                _battleInstanse.release();
            }
        }

        public void StartCampingSoundtrack()
        {
            StopCampingSoundtrack();

            _campingInstanse = RuntimeManager.CreateInstance("event:/ambience/local/campfire");
            SoundSetLevel(_campingInstanse);
            if (_campingInstanse != null)
                _campingInstanse.start();

            _campingMusicInstanse = RuntimeManager.CreateInstance("event:/music/mus_camp");
            SoundSetLevel(_campingMusicInstanse);
            if (_campingMusicInstanse != null)
                _campingMusicInstanse.start();

        }

        public void StopCampingSoundtrack()
        {
            if (_campingInstanse != null)
            {
                _campingInstanse.stop(STOP_MODE.ALLOWFADEOUT);
                _campingInstanse.release();
            }
            if (_campingMusicInstanse != null)
            {
                _campingMusicInstanse.stop(STOP_MODE.ALLOWFADEOUT);
                _campingMusicInstanse.release();
            }
        }

        public void StartTownSoundtrack()
        {
            StopTownSoundtrack();

            _townInstanse = RuntimeManager.CreateInstance("event:/ambience/town/general");
            SoundSetLevel(_townInstanse);
            if (_townInstanse != null)
                _townInstanse.start();

            _townMusicInstanse = RuntimeManager.CreateInstance("event:/music/mus_town");
            SoundSetLevel(_townMusicInstanse);
            if (_townMusicInstanse != null)
                _townMusicInstanse.start();
        }

        public void StopTownSoundtrack()
        {
            if (_townInstanse != null)
            {
                _townInstanse.stop(STOP_MODE.ALLOWFADEOUT);
                _townInstanse.release();
            }
            if (_townMusicInstanse != null)
            {
                _townMusicInstanse.stop(STOP_MODE.ALLOWFADEOUT);
                _townMusicInstanse.release();
            }
        }

    }
}
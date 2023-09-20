using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour//we need multiple instances of this. store sound[] in separate GO then reference that i suppose
{
    public Sound[] sounds;
    private Sound[] soundList;//its bad that we're sharing the same list actually.... maybe make a copy on awake?
    private Sound[] newSoundList;
    private bool soundListLoaded = false;

    // Start is called before the first frame update
    void Awake()
    {
        soundList = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<SoundsList>().sounds;
    }

    public void SetListener(GameObject _obj)
    {
        foreach (Sound s in newSoundList)
        {
            s.source = _obj.GetComponent<AudioSource>();
        }
    }

    public void Play(string name, GameObject _objectSource, bool isMusic = false)
    {
        if (!soundListLoaded)
        {
            int i = 0;
            List<Sound> tempList = new List<Sound>();
            foreach (Sound sound in soundList)
            {
                Sound newSound = new Sound { clip = sound.clip };
                newSound.source = gameObject.AddComponent<AudioSource>();
                newSound.source.clip = sound.clip;

                newSound.source.volume = sound.volume;
                newSound.source.pitch = sound.pitch;
                newSound.source.loop = sound.loop;
                newSound.source.dopplerLevel = 0;
                newSound.name = sound.name;
                tempList.Add(newSound);
                i++;
            }
            newSoundList = tempList.ToArray();
            soundListLoaded = true;
        }



        Sound s = Array.Find(newSoundList, sound => sound.name == name);
        if (!isMusic)
        {
            //SetListener(_objectSource); nope dont work
            s.source.pitch = Random.Range(.75f, 1.25f);
            s.source.spatialBlend = 1;
            s.source.rolloffMode = AudioRolloffMode.Linear;
            s.source.minDistance = 5;
            s.source.maxDistance = 50;
            s.source.spread = 180;
            s.source.volume = .25f;
        }
        else
        {
            s.source.pitch = 1f;
        }
        s.source.Play();
    }

    public void Pause(string name)
    {
        Sound s = Array.Find(newSoundList, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogError("Sound: " + name + " not found!");
            return;
        }
        s.source.Pause();
    }

    public void UnPause(string name)
    {
        Sound s = Array.Find(newSoundList, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogError("Sound: " + name + " not found!");
            return;
        }
        s.source.UnPause();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(newSoundList, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogError("Sound: " + name + " not found!");
            return;
        }

        //s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volume / 2f, s.volume / 2f)); random volume change
        //s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitch / 2f, s.pitch / 2f)); random pitch change

        s.source.Stop();
    }
}

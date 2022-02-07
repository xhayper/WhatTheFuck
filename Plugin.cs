using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

#pragma warning disable 169

namespace WhatTheFuck
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "io.github.xhayper.whatthefuck";
        private const string PluginName = "WhatTheFuck";
        private const string PluginVersion = "1.0.0.0";

        private readonly List<Sprite> _availableSprite = new();
        private readonly List<AudioClip> _availableAudioClip = new();

        private readonly List<Material> _availableMaterial = new();

        // private readonly List<Mesh> _availableMesh = new();
        private readonly List<int> _alreadyChanged = new();

        private void Awake()
        {
            foreach (var o in Resources.LoadAll(""))
            {
                var objectType = o.GetType();
                if (objectType == typeof(Sprite))
                {
                    _availableSprite.Add((Sprite) o);
                }
                else if (objectType == typeof(AudioClip))
                {
                    _availableAudioClip.Add((AudioClip) o);
                }
                else if (objectType == typeof(Material))
                {
                    _availableMaterial.Add((Material) o);
                }
                // else if (objectType == typeof(Mesh))
                // {
                //     _availableMesh.Add((Mesh) objectType);
                // }
            }

            var harmony = new Harmony(PluginGuid);
            harmony.PatchAll();
        }

        private void Update()
        {
            var allObjects = FindObjectsOfType<GameObject>();
            var cardList =
                (from gameObject in allObjects where gameObject.name == "CardBase" select gameObject.transform)
                .ToList();
            foreach (var gameObject in allObjects)
            {
                if (_alreadyChanged.IndexOf(gameObject.GetInstanceID()) != -1) continue;
                var isInBaseCard =
                    cardList.Any(gameObject2 => gameObject.transform.IsChildOf(gameObject2.transform));

                if (isInBaseCard) continue;
                foreach (var component in gameObject.GetComponents<Object>())
                {
                    var componentType = component.GetType();
                    if (componentType == typeof(SpriteRenderer))
                    {
                        if (component.name == "CursorClone") continue;
                        var spriteRenderer = (SpriteRenderer) component;
                        var filtered = _availableSprite.FindAll(_ /*S*/ => true);
                        spriteRenderer.sprite = filtered[Random.RandomRangeInt(0, filtered.Count - 1)];
                    }
                    else if (componentType == typeof(AudioSource))
                    {
                        var audioSource = (AudioSource) component;
                        var isPlaying = audioSource.isPlaying;
                        var newClip = _availableAudioClip[Random.RandomRangeInt(0, _availableAudioClip.Count - 1)];
                        if (audioSource.time > newClip.length) audioSource.time = 0;
                        audioSource.clip = newClip;
                        if (!isPlaying) continue;
                        audioSource.Play();
                    }
                    // else if (componentType == typeof(MeshFilter))
                    // {
                    //     var meshFilter = (MeshFilter) component;
                    //     meshFilter.mesh = _availableMesh[Random.RandomRangeInt(0, _availableMesh.Count - 1)];
                    // }
                    else if (componentType == typeof(MeshRenderer))
                    {
                        var meshRenderer = (MeshRenderer) component;
                        meshRenderer.material =
                            _availableMaterial[Random.RandomRangeInt(0, _availableMaterial.Count - 1)];
                    }
                }

                _alreadyChanged.Add(gameObject.GetInstanceID());
            }
        }
    }
}
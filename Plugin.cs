using System;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace AtOEnemySkinsForPlayerMod
{
    [BepInPlugin("com.DestroyedClone.FoeFacade", "Foe Facade", "0.1.0")]
    [BepInDependency(AtOSkinExtender.Plugin.modGuid)]
    public class Plugin : BaseUnityPlugin
    {
        public const string identifier = "Foe Facade";
        public static List<string> skinsToFlip = new List<string>();
        public static BepInEx.Logging.ManualLogSource _logger;

        public void Awake()
        {
            _logger = Logger;
            AtOSkinExtender.Plugin.onCreateSkins += CreateEnemySkinsForHeroes;

            //Which is better? Making 100+ copies of the sprite but flipped
            //or just flipping each one wherever they show up?
            On.HeroItem.Start += FlipEnemySkinInCombat;
            //help
            //On.CharPopup.SetHeroAnimated += FlipEnemySkinInPreview;
            On.InitiativePortrait.Init += FlipInitiativePortrait;
            //should this be an IL hook????
            On.MenuSaveButton.SetGameData += FlipSaveDataPortrait;
        }

        private void FlipSaveDataPortrait(On.MenuSaveButton.orig_SetGameData orig, MenuSaveButton self, GameData _gameData)
        {
            orig(self, _gameData);
            Hero[] array2 = JsonHelper.FromJson<Hero>(_gameData.TeamAtO);
            for (int l = 0; l < array2.Length; l++)
            {
                if (array2[l].SubclassName != null && Globals.Instance.GetSubClassData(array2[l].SubclassName) != null)
                {
                    SkinData skinData;
                    if (array2[l].SkinUsed == null || array2[l].SkinUsed == "")
                    {
                        skinData = Globals.Instance.GetSkinData(Globals.Instance.GetSkinBaseIdBySubclass(array2[l].SubclassName));
                    }
                    else
                    {
                        skinData = Globals.Instance.GetSkinData(array2[l].SkinUsed);
                    }
                    var localScale = self.imgHero[l].transform.localScale;
                    if (skinData != null && skinsToFlip.Contains(skinData.SkinId))
                    {
                        self.imgHero[l].transform.localScale = new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z);
                    } else
                    {
                        self.imgHero[l].transform.localScale = new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z);
                    }
                }
            }
        }

        private void FlipInitiativePortrait(On.InitiativePortrait.orig_Init orig, InitiativePortrait self, int _position)
        {
            orig(self, _position);
            if (self.charSprite && self.Hero != null && skinsToFlip.Contains(self.Hero.skinUsed))
            {
                var localScale = self.charSprite.transform.localScale;
                self.charSprite.transform.localScale = new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z);
            }
        }

        private void FlipEnemySkinInPreview(On.CharPopup.orig_SetHeroAnimated orig, CharPopup self, GameObject heroSkin)
        {
            orig(self, heroSkin);
            if (self.heroAnimated)
            {
                string activeSkin = PlayerManager.Instance.GetActiveSkin(self.SCD.Id);
                var localScale = self.heroAnimated.transform.localScale;
                var newX = Mathf.Abs(localScale.x);
                if (skinsToFlip.Contains(activeSkin))
                {
                    newX *= -1;
                }
                self.heroAnimated.transform.localScale = new Vector3(newX, localScale.y, localScale.z);
            }
        }

        public static void FlipEnemySkinInCombat(On.HeroItem.orig_Start orig, HeroItem self)
        {
            orig(self);
            if (self.CharImageT && self.Hero != null && skinsToFlip.Contains(self.Hero.skinUsed))
            {
                var localScale = self.CharImageT.localScale;
                self.CharImageT.localScale = new Vector3(-localScale.x, localScale.y, localScale.z);
            }
        }

        public static void CreateEnemySkinsForHeroes()
        {
            _logger.LogMessage("Creating enemy skins!");

            //There's a bunch of alternate ones that I think(?) are identical with some purpose
            //like {ariel} and {ariel_b}
            //fackin annoying!!!!
            List<GameObject> usedNPCDisplays = new List<GameObject>();

            foreach (var npcSource in Globals.Instance._NPCsSource)
            {
                var npc = npcSource.Value;
                //loading the altar gameobject breaks the preview because its a single thing
                //and its expected an animated something idk
                if (npc.NPCName == "Imp Altar") continue;

                if (usedNPCDisplays.Contains(npc.GameObjectAnimated))
                    continue;
                usedNPCDisplays.Add(npc.GameObjectAnimated);
                //Debug.Log($"====NPC {npcSource.Key}");
                foreach (var subclass in Globals.Instance._SubClassSource)
                {
                    //they're not normally selectable so reducing memory burden a bit
                    //if they're able to then ill remove this clause
                    if (subclass.Key.StartsWith("young"))
                        continue;
                    //Debug.Log($"====NPC {npc.NPCName} for {subclass.Key}");
                    var skinData = AtOSkinExtender.Assets.CreateSkinData(subclass.Key,
                        npc.NPCName,
                        $"{subclass.Key}Skin{npc.GameObjectAnimated.name}",
                        0,
                        npc.SpriteSpeed,
                        npc.SpritePortrait,
                        npc.SpriteSpeed,
                        npc.SpritePortrait,
                        npc.GameObjectAnimated);
                    AtOSkinExtender.Assets.AddSkinDataToPack(skinData, identifier);
                    skinsToFlip.Add(skinData.SkinId);
                }
            }
            //is jork
            /*var heroCount = Globals.Instance._SubClassSource.Count;
            foreach (var npcSource in Globals.Instance._NPCsSource)
            {
                var npc = npcSource.Value;
                foreach (var subclass in Globals.Instance._SubClassSource)
                {
                    var result = UnityEngine.Random.Range(0, 2);
                    if (result > 0)
                    {

                    }
                }
            }*/
        }
    }
}

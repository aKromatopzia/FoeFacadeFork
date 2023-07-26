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
        public static List<string> skinsToFlip = new List<string>();

        public void Awake()
        {
            AtOSkinExtender.Plugin.onCreateSkins += CreateEnemySkinsForHeroes;

            On.HeroItem.Start += FlipEnemySkin;
        }
        public static void FlipEnemySkin(On.HeroItem.orig_Start orig, HeroItem self)
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
            foreach (var npcSource in Globals.Instance._NPCsSource)
            {
                var npc = npcSource.Value;
                foreach (var subclass in Globals.Instance._SubClassSource)
                {
                    var skinData = AtOSkinExtender.Assets.CreateSkinData(subclass.Key,
                        npc.NPCName,
                        0,
                        npc.SpritePortrait,
                        npc.SpritePortrait,
                        npc.SpritePortrait,
                        npc.SpritePortrait,
                        npc.GameObjectAnimated);
                    if (skinData == null)
                        continue;
                    skinData.SkinOrder = 7;
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
